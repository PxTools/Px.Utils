using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Codec for reading and writing 16-bit unsigned integer values with sentinel-based <see cref="DataValueType"/> mapping.
    /// </summary>
    public sealed class UInt16Codec(int bufferBytes = 64 * 1024) : IBinaryValueCodec
    {
        public static int ByteCount => sizeof(ushort);

        // Continuous sentinel range at the top of ushort
        internal const ushort SentinelStart = ushort.MaxValue - 6; // 65529
        private const ushort Missing = SentinelStart;
        private const ushort CanNotRepresent = SentinelStart + 1;
        private const ushort Confidential = SentinelStart + 2;
        private const ushort NotAcquired = SentinelStart + 3;
        private const ushort NotAsked = SentinelStart + 4;
        private const ushort Empty = SentinelStart + 5;
        private const ushort Nill = SentinelStart + 6; // 65535

        private const int ElementSize = sizeof(ushort);
        private readonly int _bufferBytes = Math.Max(ElementSize, bufferBytes);

        /// <summary>
        /// Writes a span of <see cref="DoubleDataValue"/> entries to the output stream using 16-bit little-endian encoding.
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
                        ushort value;
                        if (dv.Type == DataValueType.Exists)
                        {
                            double v = dv.UnsafeValue;
                            value = v < 0 ? (ushort)0 : (ushort)Math.Round(v);
                            if (value >= SentinelStart)
                            {
                                // Values colliding with sentinel range cannot be represented in this codec
                                value = MapTo(DataValueType.CanNotRepresent);
                            }
                        }
                        else
                        {
                            value = MapTo(dv.Type);
                        }
                        BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(j * ElementSize, ElementSize), value);
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
            int count = Math.Min(input.Length / ElementSize, output.Length);
            for (int i = 0; i < count; i++)
            {
                output[i] = ReadOne(input.Slice(i * ElementSize, ElementSize));
            }
        }

        /// <summary>
        /// Reads 16-bit little-endian encoded values from input bytes into a span of <see cref="DecimalDataValue"/>.
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
