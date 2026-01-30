using Px.Utils.BinaryData.ValueConverters;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Metadata;
using System.ComponentModel.DataAnnotations;

namespace Px.Utils.BinaryData
{
    public abstract class BinaryDataReader
    {
        /// <summary>
        /// Asynchronous chunk provider that returns a readable <see cref="Stream"/> for the requested window of the blob.
        /// </summary>
        /// <param name="offset">Byte offset into the blob.</param>
        /// <param name="length">Requested number of bytes.</param>
        /// <param name="ct">Cancellation token.</param>
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
        /// <exception cref="NotSupportedException">Thrown when the specified <paramref name="codecType"/> is not supported.</exception>
        public static BinaryDataReader Create(
            BinaryValueCodecType codecType,
            [Range(1, long.MaxValue)] long? maxWindowSizeBytes = null,
            [Range(1, long.MaxValue)] long? mergeCapBytes = null,
            [Range(0, long.MaxValue)] long? headerLengthBytes = null)
        {
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

        public abstract Task ReadByChunkAsync(AsyncChunkProvider provider, IMatrixMap readMap, IMatrixMap blobMap, IMatrixMap bufferMap, Memory<DoubleDataValue> buffer, CancellationToken ct);
        public abstract Task ReadFromStreamAsync(Stream source, IMatrixMap readMap, IMatrixMap blobMap, IMatrixMap bufferMap, Memory<DoubleDataValue> buffer, CancellationToken ct);
        public abstract Task ReadFromStreamAsync(Stream source, IMatrixMap readMap, IMatrixMap blobMap, IMatrixMap bufferMap, Memory<DoubleDataValue> buffer, long? streamDataPositionIndex, CancellationToken ct);
    }
}