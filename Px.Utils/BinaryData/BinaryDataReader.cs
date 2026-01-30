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
    public sealed class BinaryDataReader<TCodec> : BinaryDataReader where TCodec : IBinaryValueCodec
    {
        private const long DEFAULT_MAX_WINDOW_SIZE_BYTES = 1 * 1024 * 1024;
        private const long DEFAULT_MERGE_CAP_BYTES = 64 * 1024;
        private readonly long _maxWindowSizeBytes;
        private readonly long _mergeCapBytes;
        private readonly long _headerLengthBytes;

        /// <inheritdoc />
        public override int ByteCount => TCodec.ByteCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryDataReader{TCodec}"/> class.
        /// </summary>
        /// <param name="maxWindowSizeBytes">Optional maximum window size in bytes to use when reading chunks. Defaults to 1 MiB.</param>
        /// <param name="mergeCapBytes">Optional maximum gap, in bytes, allowed between consecutive targets to merge them into the same window. Defaults to 64 KiB.</param>
        /// <param name="headerLengthBytes">Optional header length in bytes. The tightly packed values start after this offset from the beginning. Defaults to 0.</param>
        public BinaryDataReader(
            [Range(1, long.MaxValue)] long? maxWindowSizeBytes = null,
            [Range(1, long.MaxValue)] long? mergeCapBytes = null,
            [Range(0, long.MaxValue)] long? headerLengthBytes = null)
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

            _headerLengthBytes = headerLengthBytes.GetValueOrDefault(0);
            if (_headerLengthBytes < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(headerLengthBytes), "Header length must be non-negative.");
            }
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
        public override async Task ReadByChunkAsync(AsyncChunkProvider provider, IMatrixMap readMap, IMatrixMap blobMap, IMatrixMap bufferMap, Memory<DoubleDataValue> buffer, CancellationToken ct)
        {
            if (!readMap.IsSubmapOf(blobMap)) throw new ArgumentException("The blob does not contain the entire target set.");
            if (!readMap.IsSubmapOf(bufferMap)) throw new ArgumentException($"Can not write the entire target set into the provided {nameof(buffer)}.");

            int[][] blobIndices = blobMap.GetIndicesOfSubmap(readMap);
            int[][] bufferIndices = bufferMap.GetIndicesOfSubmap(readMap);

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
                long windowOffset = _headerLengthBytes + (startWindowLinearIndx * bytesPerValue);

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
        public override async Task ReadFromStreamAsync(Stream source, IMatrixMap readMap, IMatrixMap blobMap, IMatrixMap bufferMap, Memory<DoubleDataValue> buffer, CancellationToken ct)
        {
            if (!readMap.IsSubmapOf(blobMap)) throw new ArgumentException("The blob does not contain the entire target set.");
            if (!readMap.IsSubmapOf(bufferMap)) throw new ArgumentException($"Can not write the entire target set into the provided {nameof(buffer)}.");

            if (source.CanSeek)
            {
                await ReadFromSeekableStreamAsync(source, readMap, blobMap, bufferMap, buffer, null, ct);
                return;
            }

            await ReadFromNonSeekableStreamAsync(source, readMap, blobMap, bufferMap, buffer, null, ct);
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
        /// <param name="streamDataPositionIndex">Optional stream data start index. Defaults to null (interpreted as stream at beginning of header).</param>
        /// <param name="ct">Cancellation token.</param>
        public override async Task ReadFromStreamAsync(Stream source, IMatrixMap readMap, IMatrixMap blobMap, IMatrixMap bufferMap, Memory<DoubleDataValue> buffer, long? streamDataPositionIndex, CancellationToken ct)
        {
            if (!readMap.IsSubmapOf(blobMap)) throw new ArgumentException("The blob does not contain the entire target set.");
            if (!readMap.IsSubmapOf(bufferMap)) throw new ArgumentException($"Can not write the entire target set into the provided {nameof(buffer)}.");

            if (source.CanSeek)
            {
                await ReadFromSeekableStreamAsync(source, readMap, blobMap, bufferMap, buffer, streamDataPositionIndex, ct);
                return;
            }

            await ReadFromNonSeekableStreamAsync(source, readMap, blobMap, bufferMap, buffer, streamDataPositionIndex, ct);
        }

        /// <summary>
        /// Reads target values from a seekable stream using windowed I/O. Computes index mappings and manages internal buffers.
        /// </summary>
        private async Task ReadFromSeekableStreamAsync(
            Stream source,
            IMatrixMap readMap,
            IMatrixMap blobMap,
            IMatrixMap bufferMap,
            Memory<DoubleDataValue> buffer,
            long? streamDataStartLinearIndex,
            CancellationToken ct)
        {
            int[][] blobIndices = blobMap.GetIndicesOfSubmap(readMap);
            int[][] bufferIndices = bufferMap.GetIndicesOfSubmap(readMap);
            int[] readDimSizes = [.. readMap.DimensionMaps.Select(d => d.ValueCodes.Count)];
            int[] blobRcsp = GetReverseCumulativeSizeProduct(blobMap);
            int[] bufferRcsp = GetReverseCumulativeSizeProduct(bufferMap);

            long readMapSize = readMap.GetSize();
            int bytesPerValue = TCodec.ByteCount;

            long absoluteDataStartOffset;
            if (!streamDataStartLinearIndex.HasValue)
            {
                // Stream is positioned at the beginning of the header.
                absoluteDataStartOffset = checked(source.Position + _headerLengthBytes);
            }
            else
            {
                if (streamDataStartLinearIndex.Value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(streamDataStartLinearIndex), "Value must be non-negative.");
                }

                // Provided 0 means the stream is positioned at data index 0 (header already skipped).
                // Provided N means the stream is positioned at data index N.
                absoluteDataStartOffset = checked(source.Position - (streamDataStartLinearIndex.Value * bytesPerValue));
            }

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
                long windowOffset = absoluteDataStartOffset + (startWindowLinearIndx * bytesPerValue);

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

                // Seek relative to current position. Anchoring uses the provided stream-data-start index or assumes header start.
                long relativeOffset = windowOffset - source.Position;
                if (relativeOffset != 0) source.Seek(relativeOffset, SeekOrigin.Current);
                await source.ReadExactlyAsync(mem[..windowSizeBytes], ct);

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
        private async Task ReadFromNonSeekableStreamAsync(
            Stream source,
            IMatrixMap readMap,
            IMatrixMap blobMap,
            IMatrixMap bufferMap,
            Memory<DoubleDataValue> buffer,
            long? streamDataStartLinearIndex,
            CancellationToken ct)
        {
            int[][] blobIndices = blobMap.GetIndicesOfSubmap(readMap);
            int[][] bufferIndices = bufferMap.GetIndicesOfSubmap(readMap);
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

            long chunkBase;

            if (!streamDataStartLinearIndex.HasValue)
            {
                // Stream is positioned at the beginning of the header.
                if (_headerLengthBytes > 0)
                {
                    long remaining = _headerLengthBytes;
                    while (remaining > 0)
                    {
                        ct.ThrowIfCancellationRequested();
                        int toRead = (int)Math.Min(remaining, mem.Length);
                        int read = await source.ReadAsync(mem[..toRead], ct);
                        if (read == 0)
                        {
                            throw new EndOfStreamException("Unexpected end of stream while skipping header.");
                        }
                        remaining -= read;
                    }
                }

                chunkBase = _headerLengthBytes;
            }
            else
            {
                if (streamDataStartLinearIndex.Value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(streamDataStartLinearIndex), "Value must be non-negative.");
                }

                // Stream is positioned at data index N (header already skipped). No skipping is performed.
                chunkBase = _headerLengthBytes + (streamDataStartLinearIndex.Value * bytesPerValue);
            }

            int readFromChunk = 0;
            int processed = 0;

            int[] readIndex = new int[readMap.DimensionMaps.Count];
            long nextReadLinearByteIndex = (GetNthIndex(readIndex, blobIndices, blobRcsp) * bytesPerValue) + _headerLengthBytes;

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
                    nextReadLinearByteIndex = (GetNthIndex(readIndex, blobIndices, blobRcsp) * bytesPerValue) + _headerLengthBytes;
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
                await stream.ReadExactlyAsync(mem, ct);
                return owner;
            }
            catch
            {
                owner.Dispose();
                throw;
            }
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
