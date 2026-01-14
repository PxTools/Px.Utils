using Px.Utils.BinaryData;
using Px.Utils.BinaryData.ValueConverters;
using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Metadata;

namespace Px.Utils.UnitTests.BinaryData
{
    [TestClass]
    public class BinaryDataReaderStreamTests
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

        [TestMethod]
        public async Task ReadFromStreamSeekableMultipleWindowsFullReadWritesAllValues()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 64).Select(i => $"d2_{i}")]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            int total = 64;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] blob = EncodeWithUInt32(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            CountingSeekableStream stream = new(blob);
            BinaryDataReader<UInt32Codec> reader = new(16);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);
            stream.CompleteCurrentWindow();

            Assert.AreEqual(16, stream.SeekOffsets.Count);
            for (int i = 0; i < stream.SeekOffsets.Count; i++)
            {
                Assert.AreEqual(i * 16, stream.SeekOffsets[i]);
            }
            for (int i = 0; i < total; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamSeekableSubsetSelectionWritesCorrectPositions()
        {
            MatrixMap blobMap = BuildMap(["d0_0", "d0_1"], ["d1_0", "d1_1"], [.. Enumerable.Range(0, 32).Select(i => $"d2_{i}")]);
            MatrixMap readMap = BuildMap(["d0_1"], ["d1_0"], [.. Enumerable.Range(0, 32).Where(i => i % 3 == 0).Select(i => $"d2_{i}")]);
            MatrixMap bufferMap = blobMap;

            int total = 2 * 2 * 32;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] blob = EncodeWithUInt32(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            CountingSeekableStream stream = new(blob);
            BinaryDataReader<UInt32Codec> reader = new(32);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);
            stream.CompleteCurrentWindow();

            Assert.IsTrue(stream.SeekOffsets.Count >= 1);
            Assert.AreEqual(256, stream.SeekOffsets[0]);

            for (int k = 0; k < 32; k += 3)
            {
                int pos = 1 * 64 + 0 * 32 + k;
                Assert.AreEqual(DataValueType.Exists, dst[pos].Type);
                Assert.AreEqual((double)pos, dst[pos].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamSeekableMergeCapHighMergesIntoSingleWindow()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 64).Select(i => $"d2_{i}")]);
            MatrixMap readMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 64).Where(i => i % 4 == 0).Select(i => $"d2_{i}")]);
            MatrixMap bufferMap = blobMap;

            int total = 64;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] blob = EncodeWithUInt32(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            CountingSeekableStream stream = new(blob);
            BinaryDataReader<UInt32Codec> reader = new(4096, 1024 * 1024);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);
            stream.CompleteCurrentWindow();

            Assert.AreEqual(1, stream.SeekOffsets.Count);
            Assert.AreEqual(0, stream.SeekOffsets[0]);
            for (int i = 0; i < total; i += 4)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamSeekableMergeCapLowSplitsIntoManyWindows()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], Enumerable.Range(0, 64).Select(i => $"d2_{i}").ToArray());
            MatrixMap readMap = BuildMap(["d0_0"], ["d1_0"], Enumerable.Range(0, 64).Where(i => i % 4 == 0).Select(i => $"d2_{i}").ToArray());
            MatrixMap bufferMap = blobMap;

            int total = 64;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] blob = EncodeWithUInt32(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            CountingSeekableStream stream = new(blob);
            BinaryDataReader<UInt32Codec> reader = new(4096, 8);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);
            stream.CompleteCurrentWindow();

            int expectedCalls = 64 / 4;
            Assert.AreEqual(expectedCalls, stream.SeekOffsets.Count);
            for (int i = 0; i < stream.SeekOffsets.Count; i++)
            {
                Assert.AreEqual(i * 16, stream.SeekOffsets[i]);
            }
            for (int i = 0; i < total; i += 4)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamSeekableDifferentCodecUInt16WritesAllValues()
        {
            MatrixMap blobMap = BuildMap(["d0_0", "d0_1"], ["d1_0"], ["d2_0", "d2_1", "d2_2"]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            DoubleDataValue[] sourceValues = MakeSequence(6);
            byte[] blob = EncodeWithUInt16(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[6];
            Memory<DoubleDataValue> buffer = new(dst);

            CountingSeekableStream stream = new(blob);
            BinaryDataReader<UInt16Codec> reader = new();
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);
            stream.CompleteCurrentWindow();

            Assert.AreEqual(1, stream.SeekOffsets.Count);
            Assert.AreEqual(0, stream.SeekOffsets[0]);
            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamSeekableDifferentBlobAndBufferMapsWritesReorderedBuffer()
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

            CountingSeekableStream stream = new(blob);
            BinaryDataReader<UInt32Codec> reader = new();
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);
            stream.CompleteCurrentWindow();

            Assert.IsTrue(stream.SeekOffsets.Count >= 1);
            Assert.AreEqual(24, stream.SeekOffsets[0]);
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
        public async Task ReadFromStreamSeekableCancellationThrowsTaskCanceled()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], ["d2_0", "d2_1", "d2_2", "d2_3"]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            DoubleDataValue[] sourceValues = MakeSequence(4);
            byte[] blob = EncodeWithUInt32(sourceValues);

            CountingSeekableStream stream = new(blob);
            CancellationTokenSource cts = new();
            cts.Cancel();

            BinaryDataReader<UInt32Codec> reader = new();
            await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
            {
                await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, new Memory<DoubleDataValue>(new DoubleDataValue[4]), cts.Token);
            });
        }

        [TestMethod]
        public async Task ReadFromStreamSeekableShortReadThrowsEndOfStream()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], ["d2_0", "d2_1"]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            DoubleDataValue[] sourceValues = MakeSequence(2);
            byte[] full = EncodeWithUInt32(sourceValues);
            byte[] truncated = full.Take(6).ToArray();

            CountingSeekableStream stream = new(truncated);
            BinaryDataReader<UInt32Codec> reader = new();
            await Assert.ThrowsExactlyAsync<EndOfStreamException>(async () =>
            {
                await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, new Memory<DoubleDataValue>(new DoubleDataValue[2]), CancellationToken.None);
            });
        }

        [TestMethod]
        public async Task ReadFromStreamNonSeekableFullReadAcrossChunksWritesAllValues()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 64).Select(i => $"d2_{i}")]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            DoubleDataValue[] sourceValues = MakeSequence(64);
            byte[] blob = EncodeWithUInt32(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[64];
            Memory<DoubleDataValue> buffer = new(dst);

            using NonSeekableReadOnlyStream stream = new(blob);
            BinaryDataReader<UInt32Codec> reader = new(16);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            for (int i = 0; i < 64; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamNonSeekableUnexpectedEofThrowsEndOfStream()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], ["d2_0", "d2_1", "d2_2", "d2_3"]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            DoubleDataValue[] values = MakeSequence(2);
            byte[] blob = EncodeWithUInt32(values);

            using NonSeekableReadOnlyStream stream = new(blob);
            BinaryDataReader<UInt32Codec> reader = new(64);
            await Assert.ThrowsExactlyAsync<EndOfStreamException>(async () =>
            {
                await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, new Memory<DoubleDataValue>(new DoubleDataValue[4]), CancellationToken.None);
            });
        }

        [TestMethod]
        public async Task ReadFromStreamNonSeekableMisalignedBytesThrowsEndOfStream()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], ["d2_0"]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            byte[] misaligned = new byte[] { 1, 2, 3 };
            using NonSeekableReadOnlyStream stream = new(misaligned);

            BinaryDataReader<UInt32Codec> reader = new(64);
            await Assert.ThrowsExactlyAsync<EndOfStreamException>(async () =>
            {
                await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, new Memory<DoubleDataValue>(new DoubleDataValue[1]), CancellationToken.None);
            });
        }

        [TestMethod]
        public async Task ReadFromStreamNonSeekableCancellationThrowsTaskCanceled()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], ["d2_0", "d2_1"]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            DoubleDataValue[] values = MakeSequence(2);
            byte[] blob = EncodeWithUInt32(values);
            using NonSeekableReadOnlyStream stream = new(blob);

            CancellationTokenSource cts = new();
            cts.Cancel();

            BinaryDataReader<UInt32Codec> reader = new(16);
            await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
            {
                await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, new Memory<DoubleDataValue>(new DoubleDataValue[2]), cts.Token);
            });
        }

        [TestMethod]
        public async Task ReadFromStreamNonSeekableDifferentBlobAndBufferMapsWritesReorderedBuffer()
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

            using NonSeekableReadOnlyStream stream = new(blob);
            BinaryDataReader<UInt32Codec> reader = new();
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            int[] expectedBufferPositions = [6, 8, 11, 13];
            double[] expectedValues = [6, 8, 11, 13];
            for (int i = 0; i < expectedBufferPositions.Length; i++)
            {
                int pos = expectedBufferPositions[i];
                Assert.AreEqual(DataValueType.Exists, dst[pos].Type);
                Assert.AreEqual(expectedValues[i], dst[pos].UnsafeValue);
            }
        }
    }
}
