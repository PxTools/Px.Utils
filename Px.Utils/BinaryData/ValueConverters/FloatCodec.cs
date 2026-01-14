using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Codec for reading and writing 32-bit floating point values with sentinel-based <see cref="DataValueType"/> mapping.
    /// </summary>
    public sealed class FloatCodec(int bufferBytes = 64 * 1024) : IBinaryValueCodec
    {
        public static int ByteCount => sizeof(float);

        // Use contiguous smallest positive subnormals by bit pattern
        internal const int SentinelBitsStart = 0x00000001; // next after 0x00000000
        private static readonly float Missing = BitConverter.Int32BitsToSingle(SentinelBitsStart);
        private static readonly float CanNotRepresent = BitConverter.Int32BitsToSingle(SentinelBitsStart + 1);
        private static readonly float Confidential = BitConverter.Int32BitsToSingle(SentinelBitsStart + 2);
        private static readonly float NotAcquired = BitConverter.Int32BitsToSingle(SentinelBitsStart + 3);
        private static readonly float NotAsked = BitConverter.Int32BitsToSingle(SentinelBitsStart + 4);
        private static readonly float Empty = BitConverter.Int32BitsToSingle(SentinelBitsStart + 5);
        private static readonly float Nill = BitConverter.Int32BitsToSingle(SentinelBitsStart + 6);

        private readonly int _bufferBytes = Math.Max(sizeof(float), bufferBytes);

        /// <summary>
        /// Writes a span of <see cref="DoubleDataValue"/> entries to the output stream using 32-bit little-endian encoding of float.
        /// </summary>
        /// <param name="input">Input values to encode.</param>
        /// <param name="output">Destination stream to write to.</param>
        public void Write(ReadOnlySpan<DoubleDataValue> input, Stream output)
        {
            ArgumentNullException.ThrowIfNull(output);

            const int elemSize = sizeof(float);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(_bufferBytes);
            try
            {
                int maxElems = Math.Max(1, buffer.Length / elemSize);
                int i = 0;
                int count = input.Length;
                while (i < count)
                {
                    int elements = Math.Min(count - i, maxElems);
                    Span<byte> span = buffer.AsSpan(0, elements * elemSize);
                    for (int j = 0; j < elements; j++)
                    {
                        DoubleDataValue dv = input[i + j];
                        float value = dv.Type == DataValueType.Exists ? (float)dv.UnsafeValue : MapTo(dv.Type);
                        int bits = BitConverter.SingleToInt32Bits(value);
                        BinaryPrimitives.WriteInt32LittleEndian(span.Slice(j * elemSize, elemSize), bits);
                    }
                    output.Write(buffer, 0, elements * elemSize);
                    i += elements;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Reads a single 32-bit little-endian encoded float value into a <see cref="DoubleDataValue"/>.
        /// </summary>
        /// <param name="input4Bytes">A span containing at least 4 bytes (little-endian) representing a float.</param>
        /// <returns>The decoded <see cref="DoubleDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DoubleDataValue ReadOne(ReadOnlySpan<byte> input4Bytes)
        {
            int bits = BinaryPrimitives.ReadInt32LittleEndian(input4Bytes);
            float value = BitConverter.Int32BitsToSingle(bits);
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DoubleDataValue(value, DataValueType.Exists)
                : new DoubleDataValue(0, type);
        }

        /// <summary>
        /// Reads a single 32-bit little-endian encoded float value into a <see cref="DecimalDataValue"/>.
        /// </summary>
        /// <param name="input4Bytes">A span containing at least 4 bytes (little-endian) representing a float.</param>
        /// <returns>The decoded <see cref="DecimalDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DecimalDataValue ReadOneAsDecimal(ReadOnlySpan<byte> input4Bytes)
        {
            int bits = BinaryPrimitives.ReadInt32LittleEndian(input4Bytes);
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
            const int elemSize = sizeof(float);
            int count = Math.Min(input.Length / elemSize, output.Length);
            for (int i = 0; i < count; i++)
            {
                output[i] = ReadOne(input.Slice(i * elemSize, elemSize));
            }
        }

        /// <summary>
        /// Reads 32-bit little-endian encoded float values from input bytes into a span of <see cref="DecimalDataValue"/>.
        /// </summary>
        /// <param name="input">Source bytes to decode.</param>
        /// <param name="output">Destination span for decoded values.</param>
        public void Read(ReadOnlySpan<byte> input, Span<DecimalDataValue> output)
        {
            const int elemSize = sizeof(float);
            int count = Math.Min(input.Length / elemSize, output.Length);
            for (int i = 0; i < count; i++)
            {
                output[i] = ReadOneAsDecimal(input.Slice(i * elemSize, elemSize));
            }
        }

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

        private static DataValueType MapFrom(float value)
        {
            int bits = BitConverter.SingleToInt32Bits(value);
            if (bits >= SentinelBitsStart && bits <= SentinelBitsStart + 6)
            {
                int offset = bits - SentinelBitsStart;
                return offset switch
                {
                    0 => DataValueType.Missing,
                    1 => DataValueType.CanNotRepresent,
                    2 => DataValueType.Confidential,
                    3 => DataValueType.NotAcquired,
                    4 => DataValueType.NotAsked,
                    5 => DataValueType.Empty,
                    6 => DataValueType.Nill,
                    _ => DataValueType.Exists
                };
            }
            return DataValueType.Exists;
        }
    }
}