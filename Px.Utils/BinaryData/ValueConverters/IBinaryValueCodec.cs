using Px.Utils.Models.Data.DataValue;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Defines methods for encoding and decoding strongly-typed data values to and from binary representations.
    /// </summary>
    /// <remarks>
    /// Implementations are expected to be allocation-free and operate on spans for performance. The <c>Write</c> method
    /// serializes a sequence of <see cref="DoubleDataValue"/> instances to the provided <see cref="Stream"/>, while the
    /// <c>Read</c> methods decode binary input into the provided output spans. Endianness and wire format are defined by
    /// the specific codec implementation.
    /// </remarks>
    public interface IBinaryValueCodec
    {
        public static abstract int ByteCount { get; }

        /// <summary>
        /// Writes the given sequence of <see cref="DoubleDataValue"/> values to the output stream using the codec's binary format.
        /// </summary>
        /// <param name="input">A read-only span of input values to serialize.</param>
        /// <param name="output">The destination stream that receives the encoded bytes.</param>
        /// <remarks>
        /// Implementations should not allocate and should write exactly the number of bytes implied by the codec's format.
        /// </remarks>
        void Write(ReadOnlySpan<DoubleDataValue> input, Stream output);

        /// <summary>
        /// Reads a single value from a little-endian byte span and decodes it using the codec's binary format.
        /// </summary>
        /// <param name="bytes">A span of bytes containing exactly one encoded value for this codec.</param>
        /// <returns>The decoded <see cref="DoubleDataValue"/>.</returns>
        static abstract DoubleDataValue ReadOne(ReadOnlySpan<byte> bytes);

        /// <summary>
        /// Reads binary input into the provided <see cref="DoubleDataValue"/> output span using the codec's binary format.
        /// </summary>
        /// <param name="input">A read-only span of bytes containing the encoded values.</param>
        /// <param name="output">A span that will be populated with the decoded <see cref="DoubleDataValue"/> values.</param>
        /// <remarks>
        /// Implementations should validate that <paramref name="input"/> contains sufficient data for the size of <paramref name="output"/>.
        /// </remarks>
        void Read(ReadOnlySpan<byte> input, Span<DoubleDataValue> output);

        /// <summary>
        /// Reads a single value from a little-endian byte span and decodes it to a <see cref="DecimalDataValue"/> using the codec's binary format.
        /// </summary>
        /// <param name="bytes">A span of bytes containing exactly one encoded value for this codec.</param>
        /// <returns>The decoded <see cref="DecimalDataValue"/>.</returns>
        static abstract DecimalDataValue ReadOneAsDecimal(ReadOnlySpan<byte> bytes);

        /// <summary>
        /// Reads binary input into the provided <see cref="DecimalDataValue"/> output span using the codec's binary format.
        /// </summary>
        /// <param name="input">A read-only span of bytes containing the encoded values.</param>
        /// <param name="output">A span that will be populated with the decoded <see cref="DecimalDataValue"/> values.</param>
        /// <remarks>
        /// Implementations should validate that <paramref name="input"/> contains sufficient data for the size of <paramref name="output"/>.
        /// </remarks>
        void Read(ReadOnlySpan<byte> input, Span<DecimalDataValue> output);
    }
}