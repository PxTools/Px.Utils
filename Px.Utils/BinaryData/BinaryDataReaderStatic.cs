using Px.Utils.BinaryData.ValueConverters;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Metadata;
using System.ComponentModel.DataAnnotations;

namespace Px.Utils.BinaryData
{
    /// <summary>
    /// Base class for reading binary-encoded matrix data and decoding it into <see cref="DoubleDataValue"/> values.
    /// </summary>
    public abstract class BinaryDataReader
    {
        private const long MaxWindowSizeBytesMinValue = 1;
        private const long MergeCapBytesMinValue = 1;
        private const long HeaderLengthBytesMinValue = 0;

        private const long MaxWindowSizeBytesMaxValue = long.MaxValue;
        private const long MergeCapBytesMaxValue = long.MaxValue;
        private const long HeaderLengthBytesMaxValue = long.MaxValue;

        /// <summary>
        /// Asynchronous chunk provider that returns a readable <see cref="Stream"/> for the requested window of the blob.
        /// </summary>
        /// <param name="offset">Byte offset into the blob.</param>
        /// <param name="length">Requested number of bytes.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task that returns a readable <see cref="Stream"/> containing the requested window.</returns>
        public delegate Task<Stream> AsyncChunkProvider(long offset, long length, CancellationToken ct);


        /// <summary>
        /// Gets the byte count of a single encoded data value for this reader.
        /// </summary>
        public abstract int ByteCount { get; }


        /// <summary>
        /// Creates a <see cref="BinaryDataReader"/> instance specialized for the specified <paramref name="codecType"/>.
        /// </summary>
        /// <param name="codecType">The value codec that defines how binary values are decoded.</param>
        /// <param name="maxWindowSizeBytes">Optional maximum window size in bytes for chunked reads; must be greater than or equal to 1 if specified.</param>
        /// <param name="mergeCapBytes">Optional maximum merge size in bytes for adjacent chunks; must be greater than or equal to 1 if specified.</param>
        /// <param name="headerLengthBytes">Optional header length in bytes to skip before the data region; must be greater than or equal to 0 if specified.</param>
        /// <returns>A <see cref="BinaryDataReader"/> specialized for the provided codec type.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="maxWindowSizeBytes"/>, <paramref name="mergeCapBytes"/>, or <paramref name="headerLengthBytes"/>
        /// are outside of their allowed ranges.
        /// </exception>
        /// <exception cref="NotSupportedException">Thrown when the specified <paramref name="codecType"/> is not supported.</exception>
        public static BinaryDataReader Create(
            BinaryValueCodecType codecType,
            [Range(MaxWindowSizeBytesMinValue, MaxWindowSizeBytesMaxValue)] long? maxWindowSizeBytes = null,
            [Range(MergeCapBytesMinValue, MergeCapBytesMaxValue)] long? mergeCapBytes = null,
            [Range(HeaderLengthBytesMinValue, HeaderLengthBytesMaxValue)] long? headerLengthBytes = null)
        {
            if (maxWindowSizeBytes is < MaxWindowSizeBytesMinValue)
            {
                throw new ArgumentOutOfRangeException(nameof(maxWindowSizeBytes), maxWindowSizeBytes, $"Value must be greater than or equal to {MaxWindowSizeBytesMinValue}.");
            }

            if (mergeCapBytes is < MergeCapBytesMinValue)
            {
                throw new ArgumentOutOfRangeException(nameof(mergeCapBytes), mergeCapBytes, $"Value must be greater than or equal to {MergeCapBytesMinValue}.");
            }

            if (headerLengthBytes is < HeaderLengthBytesMinValue)
            {
                throw new ArgumentOutOfRangeException(nameof(headerLengthBytes), headerLengthBytes, $"Value must be greater than or equal to {HeaderLengthBytesMinValue}.");
            }

            return codecType switch
            {
                BinaryValueCodecType.UInt16Codec => new BinaryDataReader<UInt16Codec>(maxWindowSizeBytes, mergeCapBytes, headerLengthBytes),
                BinaryValueCodecType.Int16Codec => new BinaryDataReader<Int16Codec>(maxWindowSizeBytes, mergeCapBytes, headerLengthBytes),
                BinaryValueCodecType.UInt24Codec => new BinaryDataReader<UInt24Codec>(maxWindowSizeBytes, mergeCapBytes, headerLengthBytes),
                BinaryValueCodecType.Int24Codec => new BinaryDataReader<Int24Codec>(maxWindowSizeBytes, mergeCapBytes, headerLengthBytes),
                BinaryValueCodecType.UInt32Codec => new BinaryDataReader<UInt32Codec>(maxWindowSizeBytes, mergeCapBytes, headerLengthBytes),
                BinaryValueCodecType.Int32Codec => new BinaryDataReader<Int32Codec>(maxWindowSizeBytes, mergeCapBytes, headerLengthBytes),
                BinaryValueCodecType.FloatCodec => new BinaryDataReader<FloatCodec>(maxWindowSizeBytes, mergeCapBytes, headerLengthBytes),
                BinaryValueCodecType.DoubleCodec => new BinaryDataReader<DoubleCodec>(maxWindowSizeBytes, mergeCapBytes, headerLengthBytes),
                _ => throw new NotSupportedException($"Codec type {codecType} is not supported."),
            };
        }

        /// <summary>
        /// Reads values defined by <paramref name="readMap"/> from a chunked blob and writes them into <paramref name="buffer"/>.
        /// </summary>
        /// <param name="provider">Chunk provider that returns a readable stream for the requested window.</param>
        /// <param name="readMap">The selection of values to read from the blob.</param>
        /// <param name="blobMap">The full shape and ordering of values in the blob.</param>
        /// <param name="bufferMap">The target shape and ordering for writing into the output buffer.</param>
        /// <param name="buffer">Destination buffer for decoded values.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task representing the asynchronous read operation.</returns>
        public abstract Task ReadByChunkAsync(AsyncChunkProvider provider, IMatrixMap readMap, IMatrixMap blobMap, IMatrixMap bufferMap, Memory<DoubleDataValue> buffer, CancellationToken ct);

        /// <summary>
        /// Reads values defined by <paramref name="readMap"/> directly from a contiguous <see cref="Stream"/> and writes them into <paramref name="buffer"/>.
        /// </summary>
        /// <param name="source">Source stream containing the blob data.</param>
        /// <param name="readMap">The selection of values to read from the blob.</param>
        /// <param name="blobMap">The full shape and ordering of values in the blob.</param>
        /// <param name="bufferMap">The target shape and ordering for writing into the output buffer.</param>
        /// <param name="buffer">Destination buffer for decoded values.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task representing the asynchronous read operation.</returns>
        public abstract Task ReadFromStreamAsync(Stream source, IMatrixMap readMap, IMatrixMap blobMap, IMatrixMap bufferMap, Memory<DoubleDataValue> buffer, CancellationToken ct);

        /// <summary>
        /// Reads values defined by <paramref name="readMap"/> directly from a contiguous <see cref="Stream"/> and writes them into <paramref name="buffer"/>,
        /// using <paramref name="streamDataPositionIndex"/> to indicate the current position of <paramref name="source"/> within the data region.
        /// </summary>
        /// <param name="source">Source stream containing the blob data.</param>
        /// <param name="readMap">The selection of values to read from the blob.</param>
        /// <param name="blobMap">The full shape and ordering of values in the blob.</param>
        /// <param name="bufferMap">The target shape and ordering for writing into the output buffer.</param>
        /// <param name="buffer">Destination buffer for decoded values.</param>
        /// <param name="streamDataPositionIndex">
        /// Optional start index in the data region indicating the current position of the stream.
        /// A value of 0 means the stream is positioned at the first data value (header already skipped).
        /// </param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task representing the asynchronous read operation.</returns>
        public abstract Task ReadFromStreamAsync(Stream source, IMatrixMap readMap, IMatrixMap blobMap, IMatrixMap bufferMap, Memory<DoubleDataValue> buffer, long? streamDataPositionIndex, CancellationToken ct);
    }
}