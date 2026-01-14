using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Data;
using System.Buffers.Binary;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Codec for reading and writing 32-bit signed integer values with sentinel-based <see cref="DataValueType"/> mapping.
    /// </summary>
    public sealed class Int32Codec(int bufferBytes = 64 * 1024) : IBinaryValueCodec
    {
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
        private readonly int _bufferBytes = Math.Max(ElementSize, bufferBytes);

        /// <summary>
        /// Writes a span of <see cref="DoubleDataValue"/> entries to the output stream using 32-bit little-endian encoding.
        /// </summary>
        /// <param name="input">Input values to encode.</param>
        /// <param name="output">Destination stream to write to.</param>
        public void Write(ReadOnlySpan<DoubleDataValue> input, Stream output)
        {
            ArgumentNullException.ThrowIfNull(output);

            byte[] buffer = ArrayPool<byte>.Shared.Rent(_bufferBytes);
            try
            {
                int maxElems = Math.Max(1, buffer.Length / ElementSize);
                int i = 0;
                int count = input.Length;
                while (i < count)
                {
                    int elements = Math.Min(count - i, maxElems);
                    Span<byte> span = buffer.AsSpan(0, elements * ElementSize);
                    for (int j = 0; j < elements; j++)
                    {
                        DoubleDataValue dv = input[i + j];
                        int value;
                        if (dv.Type == DataValueType.Exists)
                        {
                            value = (int)Math.Round(dv.UnsafeValue);
                            if (value >= SentinelStart)
                            {
                                value = MapTo(DataValueType.CanNotRepresent);
                            }
                        }
                        else
                        {
                            value = MapTo(dv.Type);
                        }
                        BinaryPrimitives.WriteInt32LittleEndian(span.Slice(j * ElementSize, ElementSize), value);
                    }
                    output.Write(buffer, 0, elements * ElementSize);
                    i += elements;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

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