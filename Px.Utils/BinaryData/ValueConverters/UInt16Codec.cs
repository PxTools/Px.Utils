using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Data;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Codec for reading and writing 16-bit unsigned integer values with sentinel-based <see cref="DataValueType"/> mapping.
    /// </summary>
    /// <param name="bufferBytes">The size of the buffer in bytes. Default is 64KB.</param>
    public sealed class UInt16Codec(int bufferBytes = 64 * 1024) : BinaryValueCodecBase(ByteCount, bufferBytes), IBinaryValueCodec
    {
        /// <summary>
        /// The number of bytes per encoded value for this codec.
        /// </summary>
        public const int ByteCount = sizeof(ushort);
        static int IBinaryValueCodec.ByteCount => ByteCount;

        // Continuous sentinel range at the top of ushort
        internal const ushort SentinelStart = ushort.MaxValue - 6; // 65529
        private const ushort Missing = SentinelStart;
        private const ushort CanNotRepresent = SentinelStart + 1;
        private const ushort Confidential = SentinelStart + 2;
        private const ushort NotAcquired = SentinelStart + 3;
        private const ushort NotAsked = SentinelStart + 4;
        private const ushort Empty = SentinelStart + 5;
        private const ushort Nill = SentinelStart + 6; // 65535

        /// <summary>
        /// Reads a single 16-bit little-endian encoded value into a <see cref="DoubleDataValue"/>.
        /// </summary>
        /// <param name="bytes">A span containing at least 2 bytes (little-endian) representing a UInt16.</param>
        /// <returns>The decoded <see cref="DoubleDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DoubleDataValue ReadOne(ReadOnlySpan<byte> bytes)
        {
            ushort value = BinaryPrimitives.ReadUInt16LittleEndian(bytes);
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DoubleDataValue(value, DataValueType.Exists)
                : new DoubleDataValue(0, type);
        }

        /// <summary>
        /// Reads a single 16-bit little-endian encoded value into a <see cref="DecimalDataValue"/>.
        /// </summary>
        /// <param name="bytes">A span containing at least 2 bytes (little-endian) representing a UInt16.</param>
        /// <returns>The decoded <see cref="DecimalDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DecimalDataValue ReadOneAsDecimal(ReadOnlySpan<byte> bytes)
        {
            ushort value = BinaryPrimitives.ReadUInt16LittleEndian(bytes);
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DecimalDataValue(value, DataValueType.Exists)
                : new DecimalDataValue(0, type);
        }

        /// <summary>
        /// Reads 16-bit little-endian encoded values from input bytes into a span of <see cref="DoubleDataValue"/>.
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
        /// Reads 16-bit little-endian encoded values from input bytes into a span of <see cref="DecimalDataValue"/>.
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
        /// Encodes and writes a <see cref="DoubleDataValue"/> to the buffer at the specified offset using 16-bit little-endian encoding.
        /// If the value type is <see cref="DataValueType.Exists"/>, it is rounded and encoded as a 16-bit unsigned integer, unless it collides with the sentinel range,
        /// in which case it is mapped to <see cref="DataValueType.CanNotRepresent"/>. Otherwise, the value is mapped to its corresponding sentinel.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="offset">The offset in the buffer.</param>
        /// <param name="value">The value to encode and write.</param>
        protected override void WriteEncodedValue(Span<byte> buffer, int offset, DoubleDataValue value)
        {
            ushort usValue;
            if (value.Type == DataValueType.Exists)
            {
                double v = value.UnsafeValue;
                usValue = v < 0 ? (ushort)0 : (ushort)Math.Round(v);
                if (usValue >= SentinelStart)
                {
                    usValue = MapTo(DataValueType.CanNotRepresent);
                }
            }
            else
            {
                usValue = MapTo(value.Type);
            }
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, ByteCount), usValue);
        }

        /// <summary>
        /// Maps a <see cref="DataValueType"/> to its corresponding sentinel value for this codec.
        /// </summary>
        /// <param name="type">The <see cref="DataValueType"/> to map.</param>
        /// <returns>The corresponding sentinel value as a <see cref="ushort"/>.</returns>
        private static ushort MapTo(DataValueType type)
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
        private static DataValueType MapFrom(ushort value)
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
