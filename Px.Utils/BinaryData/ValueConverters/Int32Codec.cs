using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Data;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Codec for reading and writing 32-bit signed integer values with sentinel-based <see cref="DataValueType"/> mapping.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Int32Codec"/> class.
    /// </remarks>
    /// <param name="bufferBytes">The size of the internal write buffer in bytes. The minimum effective size is 4.</param>
    public sealed class Int32Codec(int bufferBytes = 64 * 1024) : BinaryValueCodecBase(ElementSize, bufferBytes), IBinaryValueCodec
    {
        /// <summary>
        /// Gets the number of bytes per encoded value for this codec.
        /// </summary>
        public static int ByteCount => sizeof(int);

        internal const int SentinelStart = int.MaxValue - 6;
        private const int Missing = SentinelStart;
        private const int CanNotRepresent = SentinelStart + 1;
        private const int Confidential = SentinelStart + 2;
        private const int NotAcquired = SentinelStart + 3;
        private const int NotAsked = SentinelStart + 4;
        private const int Empty = SentinelStart + 5;
        private const int Nill = SentinelStart + 6; // int.MaxValue

        private const int ElementSize = sizeof(int);

        /// <summary>
        /// Reads a single 32-bit little-endian encoded value into a <see cref="DoubleDataValue"/>.
        /// </summary>
        /// <param name="bytes">A span containing at least 4 bytes (little-endian) representing an Int32.</param>
        /// <returns>The decoded <see cref="DoubleDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DoubleDataValue ReadOne(ReadOnlySpan<byte> bytes)
        {
            int value = BinaryPrimitives.ReadInt32LittleEndian(bytes);
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DoubleDataValue(value, DataValueType.Exists)
                : new DoubleDataValue(0, type);
        }

        /// <summary>
        /// Reads a single 32-bit little-endian encoded value into a <see cref="DecimalDataValue"/>.
        /// </summary>
        /// <param name="bytes">A span containing at least 4 bytes (little-endian) representing an Int32.</param>
        /// <returns>The decoded <see cref="DecimalDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DecimalDataValue ReadOneAsDecimal(ReadOnlySpan<byte> bytes)
        {
            int value = BinaryPrimitives.ReadInt32LittleEndian(bytes);
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DecimalDataValue(value, DataValueType.Exists)
                : new DecimalDataValue(0, type);
        }

        /// <summary>
        /// Reads 32-bit little-endian encoded values from input bytes into a span of <see cref="DoubleDataValue"/>.
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
        /// Reads 32-bit little-endian encoded values from input bytes into a span of <see cref="DecimalDataValue"/>.
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
        /// If the value type is <see cref="DataValueType.Exists"/>, it is rounded and encoded as a 32-bit signed integer, unless it collides with the sentinel range,
        /// in which case it is mapped to <see cref="DataValueType.CanNotRepresent"/>. Otherwise, the value is mapped to its corresponding sentinel.
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
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset, ElementSize), iValue);
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