using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Codec for reading and writing 32-bit unsigned integer values with sentinel-based <see cref="DataValueType"/> mapping.
    /// </summary>
    public sealed class UInt32Codec(int bufferBytes = 64 * 1024) : IBinaryValueCodec
    {
        public static int ByteCount => sizeof(uint);

        public const uint SentinelStart = uint.MaxValue - 6u;
        private const uint Missing = SentinelStart;
        private const uint CanNotRepresent = SentinelStart + 1u;
        private const uint Confidential = SentinelStart + 2u;
        private const uint NotAcquired = SentinelStart + 3u;
        private const uint NotAsked = SentinelStart + 4u;
        private const uint Empty = SentinelStart + 5u;
        private const uint Nill = SentinelStart + 6u; // uint.MaxValue

        private readonly int _bufferBytes = Math.Max(sizeof(uint), bufferBytes);


        /// <summary>
        /// Writes a span of <see cref="DoubleDataValue"/> entries to the output stream using 32-bit little-endian encoding.
        /// </summary>
        /// <param name="input">Input values to encode.</param>
        /// <param name="output">Destination stream to write to.</param>
        public void Write(ReadOnlySpan<DoubleDataValue> input, Stream output)
        {
            ArgumentNullException.ThrowIfNull(output);

            const int elemSize = sizeof(uint);
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
                        uint value;
                        if (dv.Type == DataValueType.Exists)
                        {
                            double v = dv.UnsafeValue;
                            value = v < 0 ? 0u : (uint)Math.Round(v);
                            if (value >= SentinelStart)
                            {
                                value = MapTo(DataValueType.CanNotRepresent);
                            }
                        }
                        else
                        {
                            value = MapTo(dv.Type);
                        }
                        BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(j * elemSize, elemSize), value);
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
        /// Reads a single 32-bit little-endian encoded value into a <see cref="DoubleDataValue"/>.
        /// </summary>
        /// <param name="bytes">A span containing at least 4 bytes (little-endian) representing a UInt32.</param>
        /// <returns>The decoded <see cref="DoubleDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DoubleDataValue ReadOne(ReadOnlySpan<byte> bytes)
        {
            uint value = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DoubleDataValue(value, DataValueType.Exists)
                : new DoubleDataValue(0, type);
        }

        /// <summary>
        /// Reads a single 32-bit little-endian encoded value into a <see cref="DecimalDataValue"/>.
        /// </summary>
        /// <param name="bytes">A span containing at least 4 bytes (little-endian) representing a UInt32.</param>
        /// <returns>The decoded <see cref="DecimalDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DecimalDataValue ReadOneAsDecimal(ReadOnlySpan<byte> bytes)
        {
            uint value = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
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
            const int elemSize = sizeof(uint);
            int count = Math.Min(input.Length / elemSize, output.Length);
            for (int i = 0; i < count; i++)
            {
                output[i] = ReadOne(input.Slice(i * elemSize, elemSize));
            }
        }

        /// <summary>
        /// Reads 32-bit little-endian encoded values from input bytes into a span of <see cref="DecimalDataValue"/>.
        /// </summary>
        /// <param name="input">Source bytes to decode.</param>
        /// <param name="output">Destination span for decoded values.</param>
        public void Read(ReadOnlySpan<byte> input, Span<DecimalDataValue> output)
        {
            const int elemSize = sizeof(uint);
            int count = Math.Min(input.Length / elemSize, output.Length);
            for (int i = 0; i < count; i++)
            {
                output[i] = ReadOneAsDecimal(input.Slice(i * elemSize, elemSize));
            }
        }

        private static uint MapTo(DataValueType type)
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

        private static DataValueType MapFrom(uint value)
        {
            if (value >= SentinelStart)
            {
                uint offset = value - SentinelStart;
                return offset switch
                {
                    0u => DataValueType.Missing,
                    1u => DataValueType.CanNotRepresent,
                    2u => DataValueType.Confidential,
                    3u => DataValueType.NotAcquired,
                    4u => DataValueType.NotAsked,
                    5u => DataValueType.Empty,
                    6u => DataValueType.Nill,
                    _ => DataValueType.Exists
                };
            }
            return DataValueType.Exists;
        }
    }
}
