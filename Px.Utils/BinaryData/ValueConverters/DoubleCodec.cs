using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Data;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Codec for reading and writing 64-bit floating point values with sentinel-based <see cref="DataValueType"/> mapping.
    /// Uses the sequence of smallest positive subnormal bit patterns as sentinels and preserves the exact bit pattern when writing.
    /// </summary>
    /// <param name="bufferBytes">The size of the internal write buffer in bytes. The minimum effective size is <c>sizeof(double)</c>.</param>
    public sealed class DoubleCodec(int bufferBytes = 64 * 1024) : BinaryValueCodecBase(ElementSize, bufferBytes), IBinaryValueCodec
    {
        /// <summary>
        /// Gets the number of bytes per encoded value for this codec.
        /// </summary>
        public static int ByteCount => sizeof(double);

        private const long MissingSentinelBits = 0x0000000000000001L;
        private const long CanNotRepresentSentinelBits = MissingSentinelBits + 1L;
        private const long ConfidentialSentinelBits = MissingSentinelBits + 2L;
        private const long NotAcquiredSentinelBits = MissingSentinelBits + 3L;
        private const long NotAskedSentinelBits = MissingSentinelBits + 4L;
        private const long EmptySentinelBits = MissingSentinelBits + 5L;
        private const long NillSentinelBits = MissingSentinelBits + 6L;

        private static readonly double Missing = BitConverter.Int64BitsToDouble(MissingSentinelBits);
        private static readonly double CanNotRepresent = BitConverter.Int64BitsToDouble(CanNotRepresentSentinelBits);
        private static readonly double Confidential = BitConverter.Int64BitsToDouble(ConfidentialSentinelBits);
        private static readonly double NotAcquired = BitConverter.Int64BitsToDouble(NotAcquiredSentinelBits);
        private static readonly double NotAsked = BitConverter.Int64BitsToDouble(NotAskedSentinelBits);
        private static readonly double Empty = BitConverter.Int64BitsToDouble(EmptySentinelBits);
        private static readonly double Nill = BitConverter.Int64BitsToDouble(NillSentinelBits);

        private const int ElementSize = sizeof(double);

        /// <summary>
        /// Reads a single double-based data value from an 8-byte little-endian buffer.
        /// </summary>
        /// <param name="bytes">A span containing at least 8 bytes (little-endian) representing a double.</param>
        /// <returns>The decoded <see cref="DoubleDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DoubleDataValue ReadOne(ReadOnlySpan<byte> bytes)
        {
            long bits = BinaryPrimitives.ReadInt64LittleEndian(bytes);
            double value = BitConverter.Int64BitsToDouble(bits);
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DoubleDataValue(value, DataValueType.Exists)
                : new DoubleDataValue(0, type);
        }

        /// <summary>
        /// Reads a single double-based data value from an 8-byte little-endian buffer and converts to decimal.
        /// </summary>
        /// <param name="bytes">A span containing at least 8 bytes (little-endian) representing a double.</param>
        /// <returns>The decoded <see cref="DecimalDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DecimalDataValue ReadOneAsDecimal(ReadOnlySpan<byte> bytes)
        {
            long bits = BinaryPrimitives.ReadInt64LittleEndian(bytes);
            double value = BitConverter.Int64BitsToDouble(bits);
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DecimalDataValue((decimal)value, DataValueType.Exists)
                : new DecimalDataValue(0, type);
        }

        /// <summary>
        /// Reads 64-bit little-endian encoded double values from input bytes into a span of <see cref="DoubleDataValue"/>.
        /// </summary>
        /// <param name="input">Source bytes to decode.</param>
        /// <param name="output">Destination span for decoded values.</param>
        public void Read(ReadOnlySpan<byte> input, Span<DoubleDataValue> output)
        {
            int count = Math.Min(input.Length / ElementSize, output.Length);
            for (int i = 0; i < count; i++)
            {
                output[i] = ReadOne(input.Slice(i * ElementSize, ElementSize));
            }
        }

        /// <summary>
        /// Reads 64-bit little-endian encoded double values from input bytes into a span of <see cref="DecimalDataValue"/>.
        /// </summary>
        /// <param name="input">Source bytes to decode.</param>
        /// <param name="output">Destination span for decoded values.</param>
        public void Read(ReadOnlySpan<byte> input, Span<DecimalDataValue> output)
        {
            int count = Math.Min(input.Length / ElementSize, output.Length);
            for (int i = 0; i < count; i++)
            {
                output[i] = ReadOneAsDecimal(input.Slice(i * ElementSize, ElementSize));
            }
        }

        /// <summary>
        /// Encodes and writes a <see cref="DoubleDataValue"/> to the buffer at the specified offset using 64-bit little-endian encoding.
        /// If the value type is <see cref="DataValueType.Exists"/>, it is encoded as a 64-bit double, unless it collides with the sentinel range,
        /// in which case it is mapped to <see cref="DataValueType.CanNotRepresent"/>. Otherwise, the value is mapped to its corresponding sentinel.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="offset">The offset in the buffer.</param>
        /// <param name="value">The value to encode and write.</param>
        protected override void WriteEncodedValue(Span<byte> buffer, int offset, DoubleDataValue value)
        {
            double dValue = value.Type == DataValueType.Exists ? value.UnsafeValue : MapTo(value.Type);
            long bits = BitConverter.DoubleToInt64Bits(dValue);
            BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(offset, ElementSize), bits);
        }

        /// <summary>
        /// Maps a <see cref="DataValueType"/> to its corresponding sentinel value for this codec.
        /// </summary>
        /// <param name="type">The <see cref="DataValueType"/> to map.</param>
        /// <returns>The corresponding sentinel value as a <see cref="double"/>.</returns>
        private static double MapTo(DataValueType type)
        {
            return type switch
            {
                DataValueType.Missing => Missing,
                DataValueType.CanNotRepresent => CanNotRepresent,
                DataValueType.Confidential => Confidential,
                DataValueType.NotAcquired => NotAcquired,
                DataValueType.NotAsked => NotAsked,
                DataValueType.Empty => Empty,
                DataValueType.Nill => Nill,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        /// <summary>
        /// Maps a sentinel value to its corresponding <see cref="DataValueType"/> for this codec.
        /// </summary>
        /// <param name="value">The sentinel value to map.</param>
        /// <returns>The corresponding <see cref="DataValueType"/>.</returns>
        private static DataValueType MapFrom(double value)
        {
            long bits = BitConverter.DoubleToInt64Bits(value);
            if (bits >= MissingSentinelBits && bits <= NillSentinelBits)
            {
                return bits switch
                {
                    MissingSentinelBits => DataValueType.Missing,
                    CanNotRepresentSentinelBits => DataValueType.CanNotRepresent,
                    ConfidentialSentinelBits => DataValueType.Confidential,
                    NotAcquiredSentinelBits => DataValueType.NotAcquired,
                    NotAskedSentinelBits => DataValueType.NotAsked,
                    EmptySentinelBits => DataValueType.Empty,
                    NillSentinelBits => DataValueType.Nill,
                    _ => DataValueType.Exists
                };
            }
            return DataValueType.Exists;
        }
    }
}
