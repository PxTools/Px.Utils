using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Codec for reading and writing 16-bit signed integer values with sentinel-based <see cref="DataValueType"/> mapping.
    /// </summary>
    public sealed class Int16Codec(int bufferBytes = 64 * 1024) : IBinaryValueCodec
    {
        public static int ByteCount => sizeof(short);

        internal const short SentinelStart = short.MaxValue - 6; // 32761
        private const short Missing = SentinelStart;
        private const short CanNotRepresent = SentinelStart + 1;
        private const short Confidential = SentinelStart + 2;
        private const short NotAcquired = SentinelStart + 3;
        private const short NotAsked = SentinelStart + 4;
        private const short Empty = SentinelStart + 5;
        private const short Nill = SentinelStart + 6; // 32767

        private readonly int _bufferBytes = Math.Max(sizeof(short), bufferBytes);

        /// <summary>
        /// Writes a span of <see cref="DoubleDataValue"/> entries to the output stream using 16-bit little-endian encoding.
        /// </summary>
        /// <param name="input">Input values to encode.</param>
        /// <param name="output">Destination stream to write to.</param>
        public void Write(ReadOnlySpan<DoubleDataValue> input, Stream output)
        {
            ArgumentNullException.ThrowIfNull(output);

            const int elemSize = sizeof(short);
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
                        short value;
                        if (dv.Type == DataValueType.Exists)
                        {
                            value = (short)Math.Round(dv.UnsafeValue);
                            if (value >= SentinelStart)
                            {
                                value = MapTo(DataValueType.CanNotRepresent);
                            }
                        }
                        else
                        {
                            value = MapTo(dv.Type);
                        }
                        BinaryPrimitives.WriteInt16LittleEndian(span.Slice(j * elemSize, elemSize), value);
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
        /// Reads a single 16-bit little-endian encoded value into a <see cref="DoubleDataValue"/>.
        /// </summary>
        /// <param name="bytes">A span containing at least 2 bytes (little-endian) representing an Int16.</param>
        /// <returns>The decoded <see cref="DoubleDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DoubleDataValue ReadOne(ReadOnlySpan<byte> bytes)
        {
            short value = BinaryPrimitives.ReadInt16LittleEndian(bytes);
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DoubleDataValue(value, DataValueType.Exists)
                : new DoubleDataValue(0, type);
        }

        /// <summary>
        /// Reads a single 16-bit little-endian encoded value into a <see cref="DecimalDataValue"/>.
        /// </summary>
        /// <param name="bytes">A span containing at least 2 bytes (little-endian) representing an Int16.</param>
        /// <returns>The decoded <see cref="DecimalDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DecimalDataValue ReadOneAsDecimal(ReadOnlySpan<byte> bytes)
        {
            short value = BinaryPrimitives.ReadInt16LittleEndian(bytes);
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
            const int elemSize = sizeof(short);
            int count = Math.Min(input.Length / elemSize, output.Length);
            for (int i = 0; i < count; i++)
            {
                output[i] = ReadOne(input.Slice(i * elemSize, elemSize));
            }
        }

        /// <summary>
        /// Reads 16-bit little-endian encoded values from input bytes into a span of <see cref="DecimalDataValue"/>.
        /// </summary>
        /// <param name="input">Source bytes to decode.</param>
        /// <param name="output">Destination span for decoded values.</param>
        public void Read(ReadOnlySpan<byte> input, Span<DecimalDataValue> output)
        {
            const int elemSize = sizeof(short);
            int count = Math.Min(input.Length / elemSize, output.Length);
            for (int i = 0; i < count; i++)
            {
                output[i] = ReadOneAsDecimal(input.Slice(i * elemSize, elemSize));
            }
        }

        private static short MapTo(DataValueType type)
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

        private static DataValueType MapFrom(short value)
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
