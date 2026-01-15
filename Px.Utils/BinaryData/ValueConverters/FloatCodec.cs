using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Data;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Codec for reading and writing 32-bit floating point values with sentinel-based <see cref="DataValueType"/> mapping.
    /// </summary>
    /// <param name="bufferBytes">The size of the internal write buffer in bytes. The minimum effective size is <c>sizeof(float)</c>.</param>
    public sealed class FloatCodec(int bufferBytes = 64 * 1024) : BinaryValueCodecBase(ElementSize, bufferBytes), IBinaryValueCodec
    {
        /// <summary>
        /// Gets the number of bytes per encoded value for this codec.
        /// </summary>
        public static int ByteCount => sizeof(float);

        private const int MissingSentinelBits = 0x00000001;
        private const int CanNotRepresentSentinelBits = MissingSentinelBits + 1;
        private const int ConfidentialSentinelBits = MissingSentinelBits + 2;
        private const int NotAcquiredSentinelBits = MissingSentinelBits + 3;
        private const int NotAskedSentinelBits = MissingSentinelBits + 4;
        private const int EmptySentinelBits = MissingSentinelBits + 5;
        private const int NillSentinelBits = MissingSentinelBits + 6;

        private static readonly float Missing = BitConverter.Int32BitsToSingle(MissingSentinelBits);
        private static readonly float CanNotRepresent = BitConverter.Int32BitsToSingle(CanNotRepresentSentinelBits);
        private static readonly float Confidential = BitConverter.Int32BitsToSingle(ConfidentialSentinelBits);
        private static readonly float NotAcquired = BitConverter.Int32BitsToSingle(NotAcquiredSentinelBits);
        private static readonly float NotAsked = BitConverter.Int32BitsToSingle(NotAskedSentinelBits);
        private static readonly float Empty = BitConverter.Int32BitsToSingle(EmptySentinelBits);
        private static readonly float Nill = BitConverter.Int32BitsToSingle(NillSentinelBits);

        private const int ElementSize = sizeof(float);

        /// <summary>
        /// Reads a single 32-bit little-endian encoded float value into a <see cref="DoubleDataValue"/>.
        /// </summary>
        /// <param name="bytes">A span containing at least 4 bytes (little-endian) representing a float.</param>
        /// <returns>The decoded <see cref="DoubleDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DoubleDataValue ReadOne(ReadOnlySpan<byte> bytes)
        {
            int bits = BinaryPrimitives.ReadInt32LittleEndian(bytes);
            float value = BitConverter.Int32BitsToSingle(bits);
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DoubleDataValue(value, DataValueType.Exists)
                : new DoubleDataValue(0, type);
        }

        /// <summary>
        /// Reads a single 32-bit little-endian encoded float value into a <see cref="DecimalDataValue"/>.
        /// </summary>
        /// <param name="bytes">A span containing at least 4 bytes (little-endian) representing a float.</param>
        /// <returns>The decoded <see cref="DecimalDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DecimalDataValue ReadOneAsDecimal(ReadOnlySpan<byte> bytes)
        {
            int bits = BinaryPrimitives.ReadInt32LittleEndian(bytes);
            float value = BitConverter.Int32BitsToSingle(bits);
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DecimalDataValue((decimal)value, DataValueType.Exists)
                : new DecimalDataValue(0, type);
        }

        /// <summary>
        /// Reads 32-bit little-endian encoded float values from input bytes into a span of <see cref="DoubleDataValue"/>.
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
        /// Reads 32-bit little-endian encoded float values from input bytes into a span of <see cref="DecimalDataValue"/>.
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
        /// Encodes and writes a <see cref="DoubleDataValue"/> to the buffer at the specified offset using 32-bit little-endian encoding.
        /// If the value type is <see cref="DataValueType.Exists"/>, it is cast and encoded as a 32-bit float, unless it collides with the sentinel range,
        /// in which case it is mapped to <see cref="DataValueType.CanNotRepresent"/>. Otherwise, the value is mapped to its corresponding sentinel.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="offset">The offset in the buffer.</param>
        /// <param name="value">The value to encode and write.</param>
        protected override void WriteEncodedValue(Span<byte> buffer, int offset, DoubleDataValue value)
        {
            float fValue = value.Type == DataValueType.Exists ? (float)value.UnsafeValue : MapTo(value.Type);
            int bits = BitConverter.SingleToInt32Bits(fValue);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset, ElementSize), bits);
        }

        /// <summary>
        /// Maps a <see cref="DataValueType"/> to its corresponding sentinel value for this codec.
        /// </summary>
        /// <param name="type">The <see cref="DataValueType"/> to map.</param>
        /// <returns>The corresponding sentinel value as a <see cref="float"/>.</returns>
        private static float MapTo(DataValueType type)
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
        private static DataValueType MapFrom(float value)
        {
            int bits = BitConverter.SingleToInt32Bits(value);
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