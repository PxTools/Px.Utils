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

            using CountingSeekableStream stream = new(blob);
            BinaryDataReader<UInt32Codec> reader = new(16);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            // With a window size of 16 bytes and each UInt32 value being 4 bytes, we expect 4 values per window.
            Assert.HasCount(0, stream.SeekOffsets, "No seek calls expected. Data is fully sequential.");
            for (int i = 0; i < stream.SeekOffsets.Count; i++)
            {
                Assert.AreEqual(i * 16, stream.SeekOffsets[i], "Seek offset mismatch at index " + i);
            }
            for (int i = 0; i < total; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type, "Data value type mismatch at index " + i);
                Assert.AreEqual((double)i, dst[i].UnsafeValue, "Data value mismatch at index " + i);
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

            using CountingSeekableStream stream = new(blob);
            BinaryDataReader<UInt32Codec> reader = new(32);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            Assert.IsGreaterThanOrEqualTo(1, stream.SeekOffsets.Count);
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

            using CountingSeekableStream stream = new(blob);
            BinaryDataReader<UInt32Codec> reader = new(4096, 1024 * 1024);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            Assert.HasCount(0, stream.SeekOffsets, "No seek calls expected.");
            for (int i = 0; i < total; i += 4)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamSeekableMergeCapLowSplitsIntoManyWindows()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 64).Select(i => $"d2_{i}")]);
            MatrixMap readMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 64).Where(i => i % 4 == 0).Select(i => $"d2_{i}")]);
            MatrixMap bufferMap = blobMap;

            int total = 64;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] blob = EncodeWithUInt32(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            using CountingSeekableStream stream = new(blob);
            BinaryDataReader<UInt32Codec> reader = new(4096, 8);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            int expectedCalls = (64 / 4) - 1; // With relative seek, first window requires no seek; subsequent windows require relative gap seeks
            Assert.HasCount(expectedCalls, stream.SeekOffsets, "Expected number of seek calls mismatch.");

            // Each selected target is 4 values apart (16 bytes). Each window reads 1 value (4 bytes) due to low merge cap.
            // Therefore the relative seek to the next window is 16 - 4 = 12 bytes for each transition.
            for (int i = 0; i < stream.SeekOffsets.Count; i++)
            {
                Assert.AreEqual(12, stream.SeekOffsets[i], "Seek offset mismatch at index " + i);
            }

            for (int i = 0; i < total; i += 4)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type, "Data value type mismatch at index " + i);
                Assert.AreEqual((double)i, dst[i].UnsafeValue, "Data value mismatch at index " + i);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamSeekableCodecUInt16WritesAllValues()
        {
            MatrixMap blobMap = BuildMap(["d0_0", "d0_1"], ["d1_0"], ["d2_0", "d2_1", "d2_2"]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            DoubleDataValue[] sourceValues = MakeSequence(6);
            byte[] blob = EncodeWithUInt16(sourceValues);

            DoubleDataValue[] dst = new DoubleDataValue[6];
            Memory<DoubleDataValue> buffer = new(dst);

            using CountingSeekableStream stream = new(blob);
            BinaryDataReader<UInt16Codec> reader = new();
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            Assert.HasCount(0, stream.SeekOffsets, "No seek calls expected.");
            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type, "Data value type mismatch at index " + i);
                Assert.AreEqual((double)i, dst[i].UnsafeValue, "Data value mismatch at index " + i);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamSeekableDifferentBlobAndBufferMapsWritesReorderedBuffer()
        {
            MatrixMap baseMap = BuildMap(["d0_0", "d0_1"], ["d1_0", "d1_1", "d1_2"], ["d2_0", "d2_1", "d2_2", "d2_3", "d2_4"]);
            MatrixMap readMap = BuildMap(["d0_0"], ["d1_1", "d1_2"], ["d2_1", "d2_3"]);
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0", "d1_1", "d1_2"], ["d2_0", "d2_1", "d2_2", "d2_3", "d2_4"]);
            MatrixMap bufferMap = baseMap;

            // Both source and buffer are subsets of the complete data.
            // In this test case we use base map also as bubber map since identical map is also a valid submap.

            int blobTotal = 1 * 3 * 5;
            DoubleDataValue[] sourceValues = MakeSequence(blobTotal);
            byte[] blob = EncodeWithUInt32(sourceValues);

            int sourceTotal = 2 * 3 * 5;
            DoubleDataValue[] dst = new DoubleDataValue[sourceTotal];
            Memory<DoubleDataValue> buffer = new(dst);

            using CountingSeekableStream stream = new(blob);
            BinaryDataReader<UInt32Codec> reader = new();
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            Assert.IsGreaterThanOrEqualTo(1, stream.SeekOffsets.Count);
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

            using CountingSeekableStream stream = new(blob);
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

            using CountingSeekableStream stream = new(truncated);
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

        [TestMethod]
        public async Task ReadFromStreamSeekableRespectsHeaderLengthOnSeeksAndValues()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 16).Select(i => $"d2_{i}")]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            int total = 16;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] payload = EncodeWithUInt32(sourceValues);

            int headerLength = 12;
            byte[] header = new byte[headerLength];
            byte[] blob = new byte[header.Length + payload.Length];
            Array.Copy(header, 0, blob, 0, header.Length);
            Array.Copy(payload, 0, blob, header.Length, payload.Length);

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            CountingSeekableStream stream = new(blob);
            BinaryDataReader<UInt32Codec> reader = new(64, null, headerLength);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            Assert.HasCount(1, stream.SeekOffsets);
            Assert.AreEqual(headerLength, stream.SeekOffsets[0]);
            for (int i = 0; i < total; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamNonSeekableRespectsHeaderLengthSkipsAndValues()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 16).Select(i => $"d2_{i}")]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            int total = 16;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] payload = EncodeWithUInt32(sourceValues);

            int headerLength = 8;
            byte[] header = new byte[headerLength];
            byte[] blob = new byte[header.Length + payload.Length];
            Array.Copy(header, 0, blob, 0, header.Length);
            Array.Copy(payload, 0, blob, header.Length, payload.Length);

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            using NonSeekableReadOnlyStream stream = new(blob);
            BinaryDataReader<UInt32Codec> reader = new(64, null, headerLength);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, CancellationToken.None);

            for (int i = 0; i < total; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamSeekableWithNonZeroInitialPositionAssumesBeginningOfHeader()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 8).Select(i => $"d2_{i}")]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            int total = 8;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] payload = EncodeWithUInt32(sourceValues);

            int prefixLength = 7;
            int headerLength = 12;
            byte[] header = new byte[headerLength];
            byte[] blob = new byte[prefixLength + header.Length + payload.Length];
            Array.Copy(header, 0, blob, prefixLength, header.Length);
            Array.Copy(payload, 0, blob, prefixLength + header.Length, payload.Length);

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            using CountingSeekableStream stream = new(blob);

            stream.SeekForSetup(prefixLength, SeekOrigin.Begin);

            BinaryDataReader<UInt32Codec> reader = new(64, null, headerLength);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, streamDataPositionIndex: null, CancellationToken.None);

            Assert.HasCount(1, stream.SeekOffsets);
            Assert.AreEqual(headerLength, stream.SeekOffsets[0]);
            for (int i = 0; i < total; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual(i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamSeekableWithStreamAtDataStartIndex0ReadsCorrectly()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 8).Select(i => $"d2_{i}")]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            int total = 8;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] payload = EncodeWithUInt32(sourceValues);

            int headerLength = 12;
            byte[] header = new byte[headerLength];
            byte[] blob = new byte[header.Length + payload.Length];
            Array.Copy(header, 0, blob, 0, header.Length);
            Array.Copy(payload, 0, blob, header.Length, payload.Length);

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            using CountingSeekableStream stream = new(blob);

            stream.SeekForSetup(headerLength, SeekOrigin.Begin);

            BinaryDataReader<UInt32Codec> reader = new(64, null, headerLength);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, streamDataPositionIndex: 0, CancellationToken.None);

            Assert.HasCount(0, stream.SeekOffsets, "Expected no seeks when stream is already at data start index 0.");
            for (int i = 0; i < total; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual(i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamNonSeekableWithStreamAtDataStartIndex0ReadsCorrectly()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 8).Select(i => $"d2_{i}")]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            int total = 8;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] payload = EncodeWithUInt32(sourceValues);

            int headerLength = 12;
            byte[] header = new byte[headerLength];
            byte[] blob = new byte[header.Length + payload.Length];
            Array.Copy(header, 0, blob, 0, header.Length);
            Array.Copy(payload, 0, blob, header.Length, payload.Length);

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            // Non-seekable stream is already positioned at data index 0 (header already removed).
            byte[] streamBytes = blob[headerLength..];
            using NonSeekableReadOnlyStream stream = new(streamBytes);

            BinaryDataReader<UInt32Codec> reader = new(64, null, headerLength);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, streamDataPositionIndex: 0, CancellationToken.None);

            for (int i = 0; i < total; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamSeekableWithStreamAtDataIndexNReadsCorrectlyWithoutExtraSeeks()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 16).Select(i => $"d2_{i}")]);
            MatrixMap readMap = BuildMap(["d0_0"], ["d1_0"], ["d2_8", "d2_9", "d2_10"]);
            MatrixMap bufferMap = blobMap;

            int total = 16;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] payload = EncodeWithUInt32(sourceValues);

            int headerLength = 12;
            byte[] header = new byte[headerLength];
            byte[] blob = new byte[header.Length + payload.Length];
            Array.Copy(header, 0, blob, 0, header.Length);
            Array.Copy(payload, 0, blob, header.Length, payload.Length);

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            using CountingSeekableStream stream = new(blob);

            const int dataIndex = 8;
            int dataOffsetBytes = headerLength + (dataIndex * UInt32Codec.ByteCount);
            stream.SeekForSetup(dataOffsetBytes, SeekOrigin.Begin);

            BinaryDataReader<UInt32Codec> reader = new(128, null, headerLength);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, streamDataPositionIndex: dataIndex, CancellationToken.None);

            Assert.HasCount(0, stream.SeekOffsets, "Expected no seeks when the stream is already positioned at the first requested item.");

            for (int i = 8; i <= 10; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual(i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamNonSeekableWithStreamAtDataIndexNReadsCorrectly()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 16).Select(i => $"d2_{i}")]);
            MatrixMap readMap = BuildMap(["d0_0"], ["d1_0"], ["d2_8", "d2_9", "d2_10"]);
            MatrixMap bufferMap = blobMap;

            int total = 16;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] payload = EncodeWithUInt32(sourceValues);

            int headerLength = 12;
            byte[] header = new byte[headerLength];
            byte[] blob = new byte[header.Length + payload.Length];
            Array.Copy(header, 0, blob, 0, header.Length);
            Array.Copy(payload, 0, blob, header.Length, payload.Length);

            // Provide a non-seekable stream that starts at data index 8 (header removed and first 8 values skipped).
            const int dataIndex = 8;
            int payloadOffsetBytes = dataIndex * UInt32Codec.ByteCount;
            byte[] streamBytes = payload[payloadOffsetBytes..];

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            using NonSeekableReadOnlyStream stream = new(streamBytes);

            BinaryDataReader<UInt32Codec> reader = new(128, null, headerLength);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, streamDataPositionIndex: dataIndex, CancellationToken.None);

            for (int i = 8; i <= 10; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamNonSeekableWithInvalidNegativeDataIndexThrows()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], ["d2_0"]);
            MatrixMap readMap = blobMap;
            MatrixMap bufferMap = blobMap;

            byte[] payload = EncodeWithUInt32(MakeSequence(1));

            using NonSeekableReadOnlyStream stream = new(payload);
            BinaryDataReader<UInt32Codec> reader = new(64, null, headerLengthBytes: 0);

            await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(async () =>
            {
                await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, new Memory<DoubleDataValue>(new DoubleDataValue[1]), streamDataPositionIndex: -1, CancellationToken.None);
            });
        }

        [TestMethod]
        public async Task ReadFromStreamSeekableWithStreamAtDataIndexNAndFirstTargetAfterNPerformsSingleForwardSeek()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 32).Select(i => $"d2_{i}")]);
            MatrixMap readMap = BuildMap(["d0_0"], ["d1_0"], ["d2_12", "d2_13"]);
            MatrixMap bufferMap = blobMap;

            const int total = 32;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] payload = EncodeWithUInt32(sourceValues);

            const int headerLength = 12;
            byte[] header = new byte[headerLength];
            byte[] blob = new byte[header.Length + payload.Length];
            Array.Copy(header, 0, blob, 0, header.Length);
            Array.Copy(payload, 0, blob, header.Length, payload.Length);

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            using CountingSeekableStream stream = new(blob);

            const int streamAtDataIndex = 8;
            int streamOffsetBytes = headerLength + (streamAtDataIndex * UInt32Codec.ByteCount);
            stream.SeekForSetup(streamOffsetBytes, SeekOrigin.Begin);

            BinaryDataReader<UInt32Codec> reader = new(128, null, headerLength);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, streamDataPositionIndex: streamAtDataIndex, CancellationToken.None);

            // First requested index is 12, so we must seek forward 4 values = 16 bytes.
            Assert.HasCount(1, stream.SeekOffsets);
            Assert.AreEqual(4 * UInt32Codec.ByteCount, stream.SeekOffsets[0]);

            for (int i = 12; i <= 13; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual(i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamNonSeekableWithStreamAtDataIndexNAndFirstTargetAfterNReadsCorrectly()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 32).Select(i => $"d2_{i}")]);
            MatrixMap readMap = BuildMap(["d0_0"], ["d1_0"], ["d2_12", "d2_13"]);
            MatrixMap bufferMap = blobMap;

            const int total = 32;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] payload = EncodeWithUInt32(sourceValues);

            const int headerLength = 12;

            // Provide a non-seekable stream that starts at data index 8.
            const int streamAtDataIndex = 8;
            int payloadOffsetBytes = streamAtDataIndex * UInt32Codec.ByteCount;
            byte[] streamBytes = payload[payloadOffsetBytes..];

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            using NonSeekableReadOnlyStream stream = new(streamBytes);

            BinaryDataReader<UInt32Codec> reader = new(128, null, headerLength);
            await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, streamDataPositionIndex: streamAtDataIndex, CancellationToken.None);

            for (int i = 12; i <= 13; i++)
            {
                Assert.AreEqual(DataValueType.Exists, dst[i].Type);
                Assert.AreEqual((double)i, dst[i].UnsafeValue);
            }
        }

        [TestMethod]
        public async Task ReadFromStreamNonSeekableWithStreamAtDataIndexNAndFirstTargetBeforeNThrowsArgumentOutOfRange()
        {
            MatrixMap blobMap = BuildMap(["d0_0"], ["d1_0"], [.. Enumerable.Range(0, 16).Select(i => $"d2_{i}")]);
            MatrixMap readMap = BuildMap(["d0_0"], ["d1_0"], ["d2_2", "d2_3"]);
            MatrixMap bufferMap = blobMap;

            const int total = 16;
            DoubleDataValue[] sourceValues = MakeSequence(total);
            byte[] payload = EncodeWithUInt32(sourceValues);

            const int streamAtDataIndex = 8;
            int payloadOffsetBytes = streamAtDataIndex * UInt32Codec.ByteCount;
            byte[] streamBytes = payload[payloadOffsetBytes..];

            DoubleDataValue[] dst = new DoubleDataValue[total];
            Memory<DoubleDataValue> buffer = new(dst);

            using NonSeekableReadOnlyStream stream = new(streamBytes);

            const int headerLength = 0;
            BinaryDataReader<UInt32Codec> reader = new(128, null, headerLength);

            await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(async () =>
            {
                await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, buffer, streamDataPositionIndex: streamAtDataIndex, CancellationToken.None);
            });
        }
    }
}
