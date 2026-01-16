using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Data;
using System.Runtime.CompilerServices;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Codec for reading and writing 24-bit signed integer values with sentinel-based <see cref="DataValueType"/> mapping.
    /// </summary>
    public sealed class Int24Codec(int bufferBytes = 64 * 1024) : BinaryValueCodecBase(ByteCount, bufferBytes), IBinaryValueCodec
    {
        public static int ByteCount => 3;

        internal const int SentinelStart = 8388601; // 0x00800009 relative start in 24-bit signed range top
        private const int Missing = SentinelStart;
        private const int CanNotRepresent = SentinelStart + 1;
        private const int Confidential = SentinelStart + 2;
        private const int NotAcquired = SentinelStart + 3;
        private const int NotAsked = SentinelStart + 4;
        private const int Empty = SentinelStart + 5;
        private const int Nill = SentinelStart + 6; // 0x007FFFFF (signed top in 24-bit)

        private const int SignBitMask24 = 0x00800000;
        private const int SignExtensionMask32 = unchecked((int)0xFF000000);
        private const int Shift8 = 8;
        private const int Shift16 = 16;
        private const int ByteMask = 0xFF;

        /// <summary>
        /// Reads a single 24-bit little-endian encoded signed value into a <see cref="DoubleDataValue"/>.
        /// </summary>
        /// <param name="bytes">A span containing at least 3 bytes (little-endian) representing a signed 24-bit value.</param>
        /// <returns>The decoded <see cref="DoubleDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DoubleDataValue ReadOne(ReadOnlySpan<byte> bytes)
        {
            int b0 = bytes[0];
            int b1 = bytes[1] << Shift8;
            int b2 = bytes[2] << Shift16;
            int value = b0 | b1 | b2;
            if ((value & SignBitMask24) != 0)
            {
                value |= SignExtensionMask32;
            }
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DoubleDataValue(value, DataValueType.Exists)
                : new DoubleDataValue(0, type);
        }

        /// <summary>
        /// Reads a single 24-bit little-endian encoded signed value into a <see cref="DecimalDataValue"/>.
        /// </summary>
        /// <param name="bytes">A span containing at least 3 bytes (little-endian) representing a signed 24-bit value.</param>
        /// <returns>The decoded <see cref="DecimalDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DecimalDataValue ReadOneAsDecimal(ReadOnlySpan<byte> bytes)
        {
            int b0 = bytes[0];
            int b1 = bytes[1] << Shift8;
            int b2 = bytes[2] << Shift16;
            int value = b0 | b1 | b2;
            if ((value & SignBitMask24) != 0)
            {
                value |= SignExtensionMask32;
            }
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DecimalDataValue(value, DataValueType.Exists)
                : new DecimalDataValue(0, type);
        }

        /// <summary>
        /// Reads 24-bit little-endian encoded values from input bytes into a span of <see cref="DoubleDataValue"/>.
        /// </summary>
        /// <param name="input">Source bytes to decode.</param>
        /// <param name="output">Destination span for decoded values.</param>
        public void Read(ReadOnlySpan<byte> input, Span<DoubleDataValue> output)
        {
            int count = Math.Min(input.Length / ByteCount, output.Length);
            for (int i = 0; i < count; i++)
            {
                output[i] = ReadOne(input.Slice(i * ByteCount, ByteCount));
            }
        }

        /// <summary>
        /// Reads 24-bit little-endian encoded values from input bytes into a span of <see cref="DecimalDataValue"/>.
        /// </summary>
        /// <param name="input">Source bytes to decode.</param>
        /// <param name="output">Destination span for decoded values.</param>
        public void Read(ReadOnlySpan<byte> input, Span<DecimalDataValue> output)
        {
            int count = Math.Min(input.Length / ByteCount, output.Length);
            for (int i = 0; i < count; i++)
            {
                output[i] = ReadOneAsDecimal(input.Slice(i * ByteCount, ByteCount));
            }
        }

        /// <summary>
        /// Encodes and writes a <see cref="DoubleDataValue"/> to the buffer at the specified offset using 24-bit little-endian encoding.
        /// If the value type is <see cref="DataValueType.Exists"/>, it is rounded and encoded as a 24-bit signed integer, unless it collides with the sentinel range,
        /// in which case it is mapped to <see cref="DataValueType.CanNotRepresent"/>. Otherwise, the value is mapped to its corresponding sentinel value.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="offset">The offset in the buffer.</param>
        /// <param name="value">The value to encode and write.</param>
        protected override void WriteEncodedValue(Span<byte> buffer, int offset, DoubleDataValue value)
        {
            int iValue;
            if (value.Type == DataValueType.Exists)
            {
                iValue = (int)Math.Round(value.UnsafeValue);
                if (iValue >= SentinelStart)
                {
                    iValue = MapTo(DataValueType.CanNotRepresent);
                }
            }
            else
            {
                iValue = MapTo(value.Type);
            }
            unchecked
            {
                buffer[offset + 0] = (byte)(iValue & ByteMask);
                buffer[offset + 1] = (byte)((iValue >> Shift8) & ByteMask);
                buffer[offset + 2] = (byte)((iValue >> Shift16) & ByteMask);
            }
        }

        /// <summary>
        /// Maps a <see cref="DataValueType"/> to its corresponding sentinel value for this codec.
        /// </summary>
        /// <param name="type">The <see cref="DataValueType"/> to map.</param>
        /// <returns>The corresponding sentinel value as a <see cref="int"/>.</returns>
        private static int MapTo(DataValueType type)
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
        private static DataValueType MapFrom(int value)
        {
            if (value >= SentinelStart)
            {
                return value switch
                {
                    Missing => DataValueType.Missing,
                    CanNotRepresent => DataValueType.CanNotRepresent,
                    Confidential => DataValueType.Confidential,
                    NotAcquired => DataValueType.NotAcquired,
                    NotAsked => DataValueType.NotAsked,
                    Empty => DataValueType.Empty,
                    Nill => DataValueType.Nill,
                    _ => DataValueType.Exists
                };
            }
            return DataValueType.Exists;
        }
    }
}
