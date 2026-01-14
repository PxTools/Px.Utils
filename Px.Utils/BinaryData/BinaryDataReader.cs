using Px.Utils.BinaryData.ValueConverters;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Metadata.ExtensionMethods;
using Px.Utils.Models.Metadata;
using System.Buffers;
using System.ComponentModel.DataAnnotations;

namespace Px.Utils.BinaryData
{
    /// <summary>
    /// Provides windowed I/O helpers for reading binary-encoded matrix data and decoding values using the given <typeparamref name="TCodec"/>.
    /// Supports both chunk-based providers and contiguous streams (seekable and non-seekable).
    /// </summary>
    public class BinaryDataReader<TCodec> where TCodec : IBinaryValueCodec
    {
        /// <summary>
        /// Asynchronous chunk provider that returns a readable <see cref="Stream"/> for the requested window of the blob.
        /// </summary>
        /// <param name="offset">Byte offset into the blob.</param>
        /// <param name="length">Requested number of bytes.</param>
        /// <param name="ct">Cancellation token.</param>
        public delegate Task<Stream> AsyncChunkProvider(long offset, long length, CancellationToken ct);

        private const long DEFAULT_MAX_WINDOW_SIZE_BYTES = 1 * 1024 * 1024;
        private const long DEFAULT_MERGE_CAP_BYTES = 64 * 1024;
        private readonly long _maxWindowSizeBytes;
        private readonly long _mergeCapBytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryDataReader{TCodec}"/> class.
        /// </summary>
        /// <param name="maxWindowSizeBytes">Optional maximum window size in bytes to use when reading chunks. Defaults to 1 MiB.</param>
        /// <param name="mergeCapBytes">Optional maximum gap, in bytes, allowed between consecutive targets to merge them into the same window. Defaults to 64 KiB.</param>
        public BinaryDataReader([Range(1, long.MaxValue)] long? maxWindowSizeBytes = null, [Range(1, long.MaxValue)] long? mergeCapBytes = null)
        {
            int bytesPerValue = TCodec.ByteCount;

            long windowValue = maxWindowSizeBytes.GetValueOrDefault(DEFAULT_MAX_WINDOW_SIZE_BYTES);
            if (windowValue <= bytesPerValue)
            {
                throw new ArgumentOutOfRangeException(nameof(maxWindowSizeBytes), "Value must be greater than or equal to the codec byte count.");
            }
            _maxWindowSizeBytes = windowValue;

            long mergeValue = mergeCapBytes.GetValueOrDefault(DEFAULT_MERGE_CAP_BYTES);
            if (mergeValue < bytesPerValue)
            {
                throw new ArgumentOutOfRangeException(nameof(mergeCapBytes), "Value must be greater than or equal to the codec byte count.");
            }
            _mergeCapBytes = mergeValue;
        }

        /// <summary>
        /// Reads values defined by <paramref name="readMap"/> from a chunked blob using windowed I/O and writes them into
        /// <paramref name="buffer"/> at positions defined by <paramref name="bufferMap"/>. Decoding is performed using the codec <typeparamref name="TCodec"/>.
        /// </summary>
        /// <param name="provider">Asynchronous chunk provider that returns a readable stream for the requested window.</param>
        /// <param name="readMap">The selection of values to read from the blob.</param>
        /// <param name="blobMap">The full shape and ordering of values in the blob.</param>
        /// <param name="bufferMap">The target shape and ordering for writing into the output buffer.</param>
        /// <param name="buffer">The flat buffer where decoded <see cref="DoubleDataValue"/>s are written following <paramref name="bufferMap"/> ordering.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task ReadByChunk(AsyncChunkProvider provider, IMatrixMap readMap, IMatrixMap blobMap, IMatrixMap bufferMap, Memory<DoubleDataValue> buffer, CancellationToken ct)
        {
            if (!readMap.IsSubmapOf(blobMap)) throw new ArgumentException("The blob does not contain the entire target set.");
            if (!readMap.IsSubmapOf(bufferMap)) throw new ArgumentException("Can not write the entire target set into the provided buffer.");

            int[][] blobIndices = GetSubIndices(readMap, blobMap);
            int[][] bufferIndices = GetSubIndices(readMap, bufferMap);

            int[] readDimSizes = [.. readMap.DimensionMaps.Select(d => d.ValueCodes.Count)];

            int[] blobRcsp = GetReverseCumulativeSizeProduct(blobMap);
            int[] bufferRcsp = GetReverseCumulativeSizeProduct(bufferMap);

            long readMapSize = readMap.GetSize();
            int bytesPerValue = TCodec.ByteCount;

            int processed = 0;
            int readIndexLength = readMap.DimensionMaps.Count;
            int[] readIndex = new int[readIndexLength];
            int[] probeIndex = new int[readIndex.Length];

            long maxWindowValues = Math.Max(_maxWindowSizeBytes / bytesPerValue, 1);

            while (processed < readMapSize)
            {
                ct.ThrowIfCancellationRequested();

                long startWindowLinearIndx = GetNthIndex(readIndex, blobIndices, blobRcsp);
                long windowOffset = startWindowLinearIndx * bytesPerValue;

                int readTargetsInWindow = 1;
                Array.Copy(readIndex, probeIndex, readIndexLength);

                long endWindowLinearIndx = startWindowLinearIndx;

                // Expand window
                while (readTargetsInWindow < maxWindowValues && (processed + readTargetsInWindow) < readMapSize)
                {
                    RotateToNextIndex(probeIndex, readDimSizes); // Move probe
                    long nextBlobLinearIndex = GetNthIndex(probeIndex, blobIndices, blobRcsp);

                    // Enforce merge cap: limit gap from the last included target to the next.
                    long gapBytes = (nextBlobLinearIndex - endWindowLinearIndx) * bytesPerValue;
                    if (gapBytes > _mergeCapBytes) break;

                    // Also ensure overall window span does not exceed the max window size.
                    long projectedWindowBytes = (nextBlobLinearIndex - startWindowLinearIndx + 1) * bytesPerValue;
                    if (projectedWindowBytes > _maxWindowSizeBytes) break;

                    readTargetsInWindow++;
                    endWindowLinearIndx = nextBlobLinearIndex;
                }

                int windowSizeBytes = checked((int)((endWindowLinearIndx - startWindowLinearIndx + 1) * bytesPerValue));

                using IMemoryOwner<byte> owner = await ReadWindowAsync(windowOffset, windowSizeBytes, provider, ct);
                ReadOnlySpan<byte> span = owner.Memory.Span[..windowSizeBytes];

                // Materialize destination span only within the window scope to avoid spanning an await boundary
                for (int i = 0; i < readTargetsInWindow; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    long readLinearIndex = GetNthIndex(readIndex, blobIndices, blobRcsp);
                    int windowByteOffset = (int)((readLinearIndex - startWindowLinearIndx) * bytesPerValue);
                    DoubleDataValue value = TCodec.ReadOne(span.Slice(windowByteOffset, bytesPerValue));

                    // Read and write indices are aligned, but the linear positions differ.
                    int bufferLinearIndex = (int)GetNthIndex(readIndex, bufferIndices, bufferRcsp);
                    buffer.Span[bufferLinearIndex] = value;

                    RotateToNextIndex(readIndex, readDimSizes);
                    processed++;
                }
            }
        }

        /// <summary>
        /// Reads values defined by <paramref name="readMap"/> directly from a contiguous <see cref="Stream"/> using windowed I/O
        /// and writes them into <paramref name="buffer"/> at positions defined by <paramref name="bufferMap"/>. Decoding is
        /// performed using the codec <typeparamref name="TCodec"/>.
        /// Note: seekable streams are supported; non-seekable streams use sequential, aligned chunking.
        /// </summary>
        /// <param name="source">The source stream containing the blob data.</param>
        /// <param name="readMap">The selection of values to read from the blob.</param>
        /// <param name="blobMap">The full shape and ordering of values in the blob.</param>
        /// <param name="bufferMap">The target shape and ordering for writing into the output buffer.</param>
        /// <param name="buffer">The flat buffer where decoded <see cref="DoubleDataValue"/>s are written following <paramref name="bufferMap"/> ordering.</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task ReadFromStream(Stream source, IMatrixMap readMap, IMatrixMap blobMap, IMatrixMap bufferMap, Memory<DoubleDataValue> buffer, CancellationToken ct)
        {
            if (!readMap.IsSubmapOf(blobMap)) throw new ArgumentException("The blob does not contain the entire target set.");
            if (!readMap.IsSubmapOf(bufferMap)) throw new ArgumentException("Can not write the entire target set into the provided buffer.");

            if (source.CanSeek)
            {
                await ReadFromSeekableStream(source, readMap, blobMap, bufferMap, buffer, ct);
                return;
            }

            await ReadFromNonSeekableStream(source, readMap, blobMap, bufferMap, buffer, ct);
        }

        /// <summary>
        /// Reads target values from a seekable stream using windowed I/O. Computes index mappings and manages internal buffers.
        /// </summary>
        private async Task ReadFromSeekableStream(
            Stream source,
            IMatrixMap readMap,
            IMatrixMap blobMap,
            IMatrixMap bufferMap,
            Memory<DoubleDataValue> buffer,
            CancellationToken ct)
        {
            int[][] blobIndices = GetSubIndices(readMap, blobMap);
            int[][] bufferIndices = GetSubIndices(readMap, bufferMap);

            int[] readDimSizes = [.. readMap.DimensionMaps.Select(d => d.ValueCodes.Count)];
            int[] blobRcsp = GetReverseCumulativeSizeProduct(blobMap);
            int[] bufferRcsp = GetReverseCumulativeSizeProduct(bufferMap);

            long readMapSize = readMap.GetSize();
            int bytesPerValue = TCodec.ByteCount;

            int desiredLen = (int)_maxWindowSizeBytes;
            int chunkSize = desiredLen - (desiredLen % bytesPerValue);
            if (chunkSize <= 0)
            {
                chunkSize = bytesPerValue;
            }

            using IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(chunkSize);
            Memory<byte> mem = owner.Memory[..chunkSize];

            int processed = 0;
            int readIndexLength = readMap.DimensionMaps.Count;
            int[] readIndex = new int[readIndexLength];
            int[] probeIndex = new int[readIndex.Length];
            long maxWindowValues = Math.Max(_maxWindowSizeBytes / bytesPerValue, 1);

            while (processed < readMapSize)
            {
                ct.ThrowIfCancellationRequested();

                long startWindowLinearIndx = GetNthIndex(readIndex, blobIndices, blobRcsp);
                long windowOffset = startWindowLinearIndx * bytesPerValue;

                int readTargetsInWindow = 1;
                Array.Copy(readIndex, probeIndex, readIndex.Length);

                long endWindowLinearIndx = startWindowLinearIndx;

                // Expand window
                while (readTargetsInWindow < maxWindowValues && (processed + readTargetsInWindow) < readMapSize)
                {
                    RotateToNextIndex(probeIndex, readDimSizes); // Move probe
                    long nextBlobLinearIndex = GetNthIndex(probeIndex, blobIndices, blobRcsp);

                    // Enforce merge cap: limit gap from the last included target to the next.
                    long gapBytes = (nextBlobLinearIndex - endWindowLinearIndx) * bytesPerValue;
                    if (gapBytes > _mergeCapBytes) break;

                    // Also ensure overall window span does not exceed the max window size.
                    long projectedWindowBytes = (nextBlobLinearIndex - startWindowLinearIndx + 1) * bytesPerValue;
                    if (projectedWindowBytes > _maxWindowSizeBytes) break;

                    readTargetsInWindow++;
                    endWindowLinearIndx = nextBlobLinearIndex;
                }

                int windowSizeBytes = checked((int)((endWindowLinearIndx - startWindowLinearIndx + 1) * bytesPerValue));

                source.Seek(windowOffset, SeekOrigin.Begin);
                int total = 0;
                while (total < windowSizeBytes)
                {
                    ct.ThrowIfCancellationRequested();
                    int read = await source.ReadAsync(mem.Slice(total, windowSizeBytes - total), ct);
                    if (read == 0) break;
                    total += read;
                }

                if (total < windowSizeBytes)
                {
                    throw new EndOfStreamException("Could not read the requested number of bytes from source stream.");
                }

                ReadOnlySpan<byte> span = mem.Span[..windowSizeBytes];

                // Materialize destination span only within the window scope to avoid spanning an await boundary
                for (int i = 0; i < readTargetsInWindow; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    long readLinearIndex = GetNthIndex(readIndex, blobIndices, blobRcsp);
                    int windowByteOffset = (int)((readLinearIndex - startWindowLinearIndx) * bytesPerValue);
                    DoubleDataValue value = TCodec.ReadOne(span.Slice(windowByteOffset, bytesPerValue));

                    // Read and write indices are aligned, but the linear positions differ.
                    int bufferLinearIndex = (int)GetNthIndex(readIndex, bufferIndices, bufferRcsp);
                    buffer.Span[bufferLinearIndex] = value;

                    RotateToNextIndex(readIndex, readDimSizes);
                    processed++;
                }
            }
        }

        /// <summary>
        /// Reads target values from a non-seekable stream using sequential, aligned chunking and internal buffers.
        /// </summary>
        private async Task ReadFromNonSeekableStream(
            Stream source,
            IMatrixMap readMap,
            IMatrixMap blobMap,
            IMatrixMap bufferMap,
            Memory<DoubleDataValue> buffer,
            CancellationToken ct)
        {
            int[][] blobIndices = GetSubIndices(readMap, blobMap);
            int[][] bufferIndices = GetSubIndices(readMap, bufferMap);
            int[] readDimSizes = [.. readMap.DimensionMaps.Select(d => d.ValueCodes.Count)];
            int[] blobRcsp = GetReverseCumulativeSizeProduct(blobMap);
            int[] bufferRcsp = GetReverseCumulativeSizeProduct(bufferMap);
            long readMapSize = readMap.GetSize();
            int bytesPerValue = TCodec.ByteCount;

            int desiredLen = (int)_maxWindowSizeBytes;
            int chunkSize = desiredLen - (desiredLen % bytesPerValue);
            if (chunkSize <= 0)
            {
                chunkSize = bytesPerValue;
            }

            using IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(chunkSize);
            Memory<byte> mem = owner.Memory[..chunkSize];

            long chunkBase = 0;
            int readFromChunk = 0;
            int processed = 0;

            int[] readIndex = new int[readMap.DimensionMaps.Count];
            long nextReadLinearByteIndex = GetNthIndex(readIndex, blobIndices, blobRcsp) * bytesPerValue;

            while (processed < readMapSize)
            {
                ct.ThrowIfCancellationRequested();

                while (readFromChunk < chunkSize)
                {
                    int read = await source.ReadAsync(mem[readFromChunk..], ct);
                    if (read == 0) break;
                    readFromChunk += read;
                }

                if (readFromChunk == 0)
                {
                    if (processed < readMapSize) throw new EndOfStreamException("Unexpected end of stream while reading blob data.");
                    break;
                }

                if ((readFromChunk % bytesPerValue) != 0)
                {
                    throw new EndOfStreamException("Stream ended mid-value; total bytes are not divisible by the codec byte count.");
                }

                long chunkEnd = chunkBase + readFromChunk;

                while ((nextReadLinearByteIndex + bytesPerValue) <= chunkEnd && processed < readMapSize)
                {
                    int offsetInChunk = (int)(nextReadLinearByteIndex - chunkBase);
                    DoubleDataValue value = TCodec.ReadOne(mem.Span.Slice(offsetInChunk, bytesPerValue));

                    int bufferLinearIndex = (int)GetNthIndex(readIndex, bufferIndices, bufferRcsp);
                    buffer.Span[bufferLinearIndex] = value;

                    RotateToNextIndex(readIndex, readDimSizes);
                    processed++;
                    nextReadLinearByteIndex = GetNthIndex(readIndex, blobIndices, blobRcsp) * bytesPerValue;
                }

                chunkBase = chunkEnd;
                readFromChunk = 0;
            }
        }

        /// <summary>
        /// Reads a fixed window from a chunk provider into a rented buffer and returns the owner.
        /// </summary>
        private static async Task<IMemoryOwner<byte>> ReadWindowAsync(long offset, int size, AsyncChunkProvider provider, CancellationToken ct)
        {
            IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(size);
            Memory<byte> mem = owner.Memory[..size];

            try
            {
                await using Stream stream = await provider(offset, size, ct);
                int total = 0;
                while (total < size)
                {
                    int read = await stream.ReadAsync(mem[total..], ct);
                    if (read == 0) break;
                    total += read;
                }

                if (total < size) throw new EndOfStreamException("Could not read the requested number of bytes from blob storage.");
                return owner;
            }
            catch
            {
                owner.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Builds an index mapping from a sub map to a super map for each dimension.
        /// </summary>
        private static int[][] GetSubIndices(IMatrixMap sub, IMatrixMap super)
        {
            if (sub.DimensionMaps.Count != super.DimensionMaps.Count)
            {
                throw new ArgumentException("Number of dimensions differ between source and target maps, index mapping can not be computed.");
            }

            int[][] result = new int[sub.DimensionMaps.Count][];

            for (int dimIndx = 0; dimIndx < sub.DimensionMaps.Count; dimIndx++)
            {
                IReadOnlyList<string> subDimCodes = sub.DimensionMaps[dimIndx].ValueCodes;
                IReadOnlyList<string> superDimCodes = super.DimensionMaps[dimIndx].ValueCodes;

                int subValIndex = 0;
                int[] valIndices = new int[subDimCodes.Count];
                for (int superValIndex = 0; superValIndex < superDimCodes.Count; superValIndex++)
                {
                    if (subDimCodes[subValIndex] == superDimCodes[superValIndex])
                    {
                        valIndices[subValIndex] = superValIndex;
                        subValIndex++;
                        if (subValIndex >= subDimCodes.Count) break;
                    }
                }
                result[dimIndx] = valIndices;
            }
            return result;
        }

        /// <summary>
        /// Computes reverse cumulative size product (RCSP) multipliers for linear indexing over dimensions.
        /// </summary>
        private static int[] GetReverseCumulativeSizeProduct(IMatrixMap blobMap)
        {
            int dims = blobMap.DimensionMaps.Count;
            int[] rcsp = new int[dims];
            rcsp[^1] = 1;
            for (int i = dims - 2; i >= 0; i--)
            {
                rcsp[i] = rcsp[i + 1] * blobMap.DimensionMaps[i + 1].ValueCodes.Count;
            }
            return rcsp;
        }

        /// <summary>
        /// Converts a multi-dimensional index into a linear index using mapping and RCSP.
        /// </summary>
        private static long GetNthIndex(int[] indices, int[][] map, int[] rcsp)
        {
            long n = 0;
            for (int i = 0; i < indices.Length; i++)
            {
                n += map[i][indices[i]] * rcsp[i];
            }
            return n;
        }

        /// <summary>
        /// Advances a multi-dimensional index by one, rolling over dimensions as needed.
        /// </summary>
        private static void RotateToNextIndex(int[] indices, int[] sizes)
        {
            for (int d = indices.Length - 1; d >= 0; d--)
            {
                indices[d]++;
                if (indices[d] < sizes[d]) return;
                indices[d] = 0;
            }
        }
    }
}
