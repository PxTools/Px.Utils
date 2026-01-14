using Px.Utils.BinaryData.ValueConverters;
using Px.Utils.BinaryData;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Data;
using Px.Utils.Models.Metadata;

namespace Px.Utils.UnitTests.BinaryData
{
    [TestClass]
    public class BinaryDataReaderChunkTests
    {
        private static MatrixMap BuildMap(params string[][] dims)
        {
            List<DimensionMap> maps = [];
            for (int i = 0; i < dims.Length; i++)
            {
                maps.Add(new DimensionMap($"dim{i}", [.. dims[i]]));
            }
            return new MatrixMap([.. maps]);
        }

        private static DoubleDataValue[] MakeSequence(int count)
        {
            DoubleDataValue[] values = new DoubleDataValue[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = new DoubleDataValue(i, DataValueType.Exists);
            }
            return values;
        }

        private static byte[] EncodeWithUInt32(DoubleDataValue[] values)
        {
            UInt32Codec codec = new();
            using MemoryStream ms = new();
            codec.Write(values, ms);
            return ms.ToArray();
        }

        private static byte[] EncodeWithUInt16(DoubleDataValue[] values)
        {
            UInt16Codec codec = new();
            using MemoryStream ms = new();
            codec.Write(values, ms);
            return ms.ToArray();
        }

        private static BinaryDataReader<TCodec>.AsyncChunkProvider MakeProvider<TCodec>(byte[] blob, List<(long offset, long length)> calls) where TCodec : IBinaryValueCodec
        {
            return (offset, length, ct) =>
            {
                calls.Add((offset, length));
                Stream stream = new MemoryStream(blob, (int)offset, (int)length, writable: false);
                return Task.FromResult(stream);
            };
        }

        [TestMethod]
        public async Task ReadByChunkMultipleWindowsFullReadWritesAllValues()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 64).Select(i => $"d2_{i}")]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            int total = 64;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] blob = EncodeWithUInt32(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            List<(long offset, long length)> calls = [];
            BinaryDataReader<UInt32Codec> reader = new(16);
            BinaryDataReader<UInt32Codec>.AsyncChunkProvider provider = MakeProvider<UInt32Codec>(blob, calls);

            await reader.ReadByChunk(provider, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            Assert.AreEqual(16, calls.Count);
            for (int i = 0; i < calls.Count; i++)
            {
                Assert.AreEqual(i * 16, calls[i].offset);
            }
            for (int i = 0; i < total; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadByChunkMultipleWindowsSubsetSelectionWritesCorrectPositions()
        {
            MatrixMap blobMap = BuildMap(["d0_0", "d0_1"], ["d1_0", "d1_1"], [.. Enumerable.Range(0, 32).Select(i => $"d2_{i}")]);
            MatrixMap readMap = BuildMap(["d0_1"], ["d1_0"], [.. Enumerable.Range(0, 32).Where(i => i % 3 == 0).Select(i => $"d2_{i}")]);
            MatrixMap bufferMap = blobMap;

            int total = 2 * 2 * 32;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] blob = EncodeWithUInt32(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new Memory<DoubleDataValue>(dst);

            BinaryDataReader<UInt32Codec> reader = new BinaryDataReader<UInt32Codec>(32);
            List<(long offset, long length)> calls = [];
            BinaryDataReader<UInt32Codec>.AsyncChunkProvider provider = MakeProvider<UInt32Codec>(blob, calls);

            await reader.ReadByChunk(provider, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            Assert.IsTrue(calls.Count >= 1);
            Assert.AreEqual(256, calls[0].offset);
            for (int k = 0; k < 32; k += 3)
            {
                int pos = 1 * 64 + 0 * 32 + k;
                Assert.AreEqual(DataValueType.Exists, dst[pos].Type);
                Assert.AreEqual((double)pos, dst[pos].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadByChunkMergeCapHighMergesIntoSingleWindow()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 64).Select(i => $"d2_{i}")]);
            MatrixMap readMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 64).Where(i => i % 4 == 0).Select(i => $"d2_{i}")]);
            MatrixMap bufferMap = blobMap;

            int total = 64;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] blob = EncodeWithUInt32(sourceValues);

            List<(long offset, long length)> calls = [];
            Task<Stream> provider(long offset, long length, CancellationToken ct)
            {
                calls.Add((offset, length));
                Stream stream = new MemoryStream(blob, (int)offset, (int)length, writable: false);
                return Task.FromResult(stream);
            }

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            BinaryDataReader<UInt32Codec> reader = new(4096, 1024 * 1024);
            await reader.ReadByChunk(provider, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            Assert.AreEqual(1, calls.Count);
            Assert.AreEqual(0, calls[0].offset);
            for (int i = 0; i < total; i += 4)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadByChunkMergeCapLowSplitsIntoManyWindows()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], Enumerable.Range(0, 64).Select(i => $"d2_{i}").ToArray());
            MatrixMap readMap = BuildMap(["d0_0"], ["d1_0"], Enumerable.Range(0, 64).Where(i => i % 4 == 0).Select(i => $"d2_{i}").ToArray());
            MatrixMap bufferMap = blobMap;

            int total = 64;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] blob = EncodeWithUInt32(sourceValues);

            List<(long offset, long length)> calls = new();
            BinaryDataReader<UInt32Codec>.AsyncChunkProvider provider = (offset, length, ct) =>
            {
                calls.Add((offset, length));
                Stream stream = new MemoryStream(blob, (int)offset, (int)length, writable: false);
                return Task.FromResult(stream);
            };

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            BinaryDataReader<UInt32Codec> reader = new(4096, 8);
            await reader.ReadByChunk(provider, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            int expectedCalls = 64 / 4;
            Assert.AreEqual(expectedCalls, calls.Count);
            for (int i = 0; i < calls.Count; i++)
            {
                Assert.AreEqual(i * 16, calls[i].offset);
            }
            for (int i = 0; i < total; i += 4)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadByChunkFullReadWritesAllValues()
        {
            MatrixMap blobMap = BuildMap(["d0_0", "d0_1"], ["d1_0", "d1_1"], ["d2_0", "d2_1", "d2_2", "d2_3", "d2_4"]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            int total = 2 * 2 * 5;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] blob = EncodeWithUInt32(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            List<(long offset, long length)> calls = [];
            BinaryDataReader<UInt32Codec>.AsyncChunkProvider provider = MakeProvider<UInt32Codec>(blob, calls);
            BinaryDataReader<UInt32Codec> reader = new();
            await reader.ReadByChunk(provider, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            Assert.AreEqual(1, calls.Count);
            Assert.AreEqual(0, calls[0].offset);
            for (int i = 0; i < total; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadByChunkSubsetSelectionWritesToCorrectPositions()
        {
            MatrixMap blobMap = BuildMap(["d0_0", "d0_1"], ["d1_0", "d1_1"], ["d2_0", "d2_1", "d2_2", "d2_3", "d2_4"]);
            MatrixMap readMap = BuildMap(["d0_1"], ["d1_0", "d1_1"], ["d2_1", "d2_3"]);
            MatrixMap bufferMap = blobMap;

            int total = 20;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] blob = EncodeWithUInt32(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[20];
            Memory<DoubleDataValue> buffer = new(dst);

            List<(long offset, long length)> calls = [];
            BinaryDataReader<UInt32Codec>.AsyncChunkProvider provider = MakeProvider<UInt32Codec>(blob, calls);

            BinaryDataReader<UInt32Codec> reader = new();
            await reader.ReadByChunk(provider, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            Assert.AreEqual(1, calls.Count);
            Assert.AreEqual(44, calls[0].offset);
            int[] expectedPositions = [11, 13, 16, 18];
            double[] expectedValues = [11, 13, 16, 18];

            for (int i = 0; i < expectedPositions.Length; i++)
            {
                int pos = expectedPositions[i];
                Assert.AreEqual(DataValueType.Exists, dst[pos].Type);
                Assert.AreEqual(expectedValues[i], dst[pos].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadByChunkSparseButWithinWindowUsesSingleProviderCall()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], Enumerable.Range(0, 32).Select(i => $"d2_{i}").ToArray());
            MatrixMap readMap = BuildMap(["d0_0"], ["d1_0"], Enumerable.Range(0, 32).Where(i => i % 2 == 0).Select(i => $"d2_{i}").ToArray());
            MatrixMap bufferMap = blobMap;

            int total = 32;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] blob = EncodeWithUInt32(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[32];
            Memory<DoubleDataValue> buffer = new(dst);

            List<(long offset, long length)> calls = [];
            BinaryDataReader<UInt32Codec>.AsyncChunkProvider provider = MakeProvider<UInt32Codec>(blob, calls);

            BinaryDataReader<UInt32Codec> reader = new();
            await reader.ReadByChunk(provider, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            Assert.AreEqual(1, calls.Count);
            Assert.AreEqual(0, calls[0].offset);
            for (int i = 0; i < total; i += 2)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadByChunkDifferentBlobAndBufferMapsWritesIntoReorderedBuffer()
        {
            MatrixMap baseMap = BuildMap(["d0_0", "d0_1"], ["d1_0", "d1_1", "d1_2"], ["d2_0", "d2_1", "d2_2", "d2_3", "d2_4"]);
            MatrixMap readMap = BuildMap(["d0_0"], ["d1_1", "d1_2"], ["d2_1", "d2_3"]);
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0", "d1_1", "d1_2"], ["d2_0", "d2_1", "d2_2", "d2_3", "d2_4"]);
            MatrixMap bufferMap = baseMap;

            int blobTotal = 2 * 3 * 5;
            DoubleDataValue[] sourceValues = MakeSequence(blobTotal);
            byte[] blob = EncodeWithUInt32(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[blobTotal];
            Memory<DoubleDataValue> buffer = new(dst);

            List<(long offset, long length)> calls = [];
            BinaryDataReader<UInt32Codec>.AsyncChunkProvider provider = MakeProvider<UInt32Codec>(blob, calls);
            BinaryDataReader<UInt32Codec> reader = new();
            await reader.ReadByChunk(provider, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            Assert.AreEqual(1, calls.Count);
            Assert.AreEqual(24, calls[0].offset);
            int[] expectedBufferPositions = [6, 8, 11, 13];
            double[] expectedValues = [6, 8, 11, 13];

            for (int i = 0; i < expectedBufferPositions.Length; i++)
            {
                int pos = expectedBufferPositions[i];
                Assert.AreEqual(DataValueType.Exists, dst[pos].Type);
                Assert.AreEqual(expectedValues[i], dst[pos].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadByChunkDifferentCodecUInt16WritesAllValues()
        {
            MatrixMap blobMap = BuildMap(["d0_0", "d0_1"], ["d1_0"], ["d2_0", "d2_1", "d2_2"]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            DoubleDataValue[] sourceValues = MakeSequence(6);
            byte[] blob = EncodeWithUInt16(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[6];
            Memory<DoubleDataValue> buffer = new(dst);

            List<(long offset, long length)> calls = [];
            BinaryDataReader<UInt16Codec>.AsyncChunkProvider provider = MakeProvider<UInt16Codec>(blob, calls);
            BinaryDataReader<UInt16Codec> reader = new();
            await reader.ReadByChunk(provider, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            Assert.AreEqual(1, calls.Count);
            Assert.AreEqual(0, calls[0].offset);
            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadByChunkMissingSentinelsMappedToTypes()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], ["d2_0", "d2_1", "d2_2", "d2_3", "d2_4", "d2_5", "d2_6"]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            UInt32Codec codec = new();
            DoubleDataValue[] sentinels =
            [
                new DoubleDataValue(0, DataValueType.Missing),
                new DoubleDataValue(0, DataValueType.CanNotRepresent),
                new DoubleDataValue(0, DataValueType.Confidential),
                new DoubleDataValue(0, DataValueType.NotAcquired),
                new DoubleDataValue(0, DataValueType.NotAsked),
                new DoubleDataValue(0, DataValueType.Empty),
                new DoubleDataValue(0, DataValueType.Nill)
            ];
            using MemoryStream ms = new();
            codec.Write(sentinels, ms);
            byte[] blob = ms.ToArray();

            DoubleDataValue[] dst = new DoubleDataValue[7];
            Memory<DoubleDataValue> buffer = new(dst);

            List<(long offset, long length)> calls = [];
            BinaryDataReader<UInt32Codec>.AsyncChunkProvider provider = MakeProvider<UInt32Codec>(blob, calls);
            BinaryDataReader<UInt32Codec> reader = new();
            await reader.ReadByChunk(provider, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            Assert.AreEqual(1, calls.Count);
            Assert.AreEqual(0, calls[0].offset);
            Assert.AreEqual(DataValueType.Missing, dst[0].Type);
            Assert.AreEqual(DataValueType.CanNotRepresent, dst[1].Type);
            Assert.AreEqual(DataValueType.Confidential, dst[2].Type);
            Assert.AreEqual(DataValueType.NotAcquired, dst[3].Type);
            Assert.AreEqual(DataValueType.NotAsked, dst[4].Type);
            Assert.AreEqual(DataValueType.Empty, dst[5].Type);
            Assert.AreEqual(DataValueType.Nill, dst[6].Type);
        }

        [TestMethod]
        public async Task ReadByChunkReadMapStartsMidwayWritesCorrectValues()
        {
            MatrixMap blobMap = BuildMap(["d0_0", "d0_1"], ["d1_0", "d1_1"], ["d2_0", "d2_1", "d2_2", "d2_3", "d2_4"]);
            MatrixMap readMap = BuildMap(["d0_0", "d0_1"], ["d1_0"], ["d2_2", "d2_3", "d2_4"]);
            MatrixMap bufferMap = blobMap;

            DoubleDataValue[] sourceValues = MakeSequence(20);
            byte[] blob = EncodeWithUInt32(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[20];
            Memory<DoubleDataValue> buffer = new(dst);

            List<(long offset, long length)> calls = [];
            BinaryDataReader<UInt32Codec>.AsyncChunkProvider provider = MakeProvider<UInt32Codec>(blob, calls);
            BinaryDataReader<UInt32Codec> reader = new();
            await reader.ReadByChunk(provider, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            Assert.AreEqual(1, calls.Count);
            Assert.AreEqual(8, calls[0].offset);
            int[] positions = [2, 3, 4, 12, 13, 14];
            for (int i = 0; i < positions.Length; i++)
            {
                int pos = positions[i];
                Assert.AreEqual((double)pos, dst[pos].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadByChunkCancellationThrowsOperationCanceled()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], ["d2_0", "d2_1", "d2_2", "d2_3"]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            DoubleDataValue[] sourceValues = MakeSequence(4);
            byte[] blob = EncodeWithUInt32(sourceValues);

            CancellationTokenSource cts = new();
            Task<Stream> provider(long offset, long length, CancellationToken ct)
            {
                cts.Cancel();
                return Task.FromResult<Stream>(new MemoryStream(blob, (int)offset, (int)length, writable: false));
            }

            BinaryDataReader<UInt32Codec> reader = new();
            await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () =>
            {
                await reader.ReadByChunk(provider, readMap, blobMap, bufferMap, new Memory<DoubleDataValue>(new DoubleDataValue[4]), cts.Token);
            });
        }

        [TestMethod]
        public async Task ReadByChunkShortReadThrowsEndOfStream()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], ["d2_0", "d2_1"]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            DoubleDataValue[] sourceValues = MakeSequence(2);
            byte[] fullBlob = EncodeWithUInt32(sourceValues);

            Task<Stream> provider(long offset, long length, CancellationToken ct)
            {
                int shortLen = Math.Max(0, (int)length - 2);
                Stream stream = new MemoryStream(fullBlob, (int)offset, shortLen, writable: false);
                return Task.FromResult(stream);
            }

            BinaryDataReader<UInt32Codec> reader = new();
            await Assert.ThrowsExactlyAsync<EndOfStreamException>(async () =>
            {
                await reader.ReadByChunk(provider, readMap, blobMap, bufferMap, new Memory<DoubleDataValue>(new DoubleDataValue[2]), CancellationToken.None);
            });
        }
    }
}
