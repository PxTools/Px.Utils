using System.Buffers;
using System.Runtime.CompilerServices;
using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Codec for reading and writing 24-bit unsigned integer values with sentinel-based <see cref="DataValueType"/> mapping.
    /// </summary>
    public sealed class UInt24Codec(int bufferBytes = 64 * 1024) : IBinaryValueCodec
    {
        public static int ByteCount => sizeof(uint);

        internal const uint SentinelStart = 16777209u; // 0x00FFFFF9
        private const uint Missing = SentinelStart;
        private const uint CanNotRepresent = SentinelStart + 1;
        private const uint Confidential = SentinelStart + 2;
        private const uint NotAcquired = SentinelStart + 3;
        private const uint NotAsked = SentinelStart + 4;
        private const uint Empty = SentinelStart + 5;
        private const uint Nill = SentinelStart + 6; // 0x00FFFFFF

        private readonly int _bufferBytes = Math.Max(3, bufferBytes);

        /// <summary>
        /// Writes a span of <see cref="DoubleDataValue"/> entries to the output stream using 24-bit little-endian encoding.
        /// </summary>
        /// <param name="input">Input values to encode.</param>
        /// <param name="output">Destination stream to write to.</param>
        public void Write(ReadOnlySpan<DoubleDataValue> input, Stream output)
        {
            ArgumentNullException.ThrowIfNull(output);

            const int elemSize = 3;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(_bufferBytes);
            try
            {
                int maxElems = Math.Max(1, buffer.Length / elemSize);
                int i = 0;
                int count = input.Length;
                while (i < count)
                {
                    int elements = Math.Min(count - i, maxElems);
                    int totalBytes = elements * elemSize;
                    Span<byte> span = buffer.AsSpan(0, totalBytes);

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

                        unchecked
                        {
                            span[j * elemSize + 0] = (byte)(value & 0xFF);
                            span[j * elemSize + 1] = (byte)((value >> 8) & 0xFF);
                            span[j * elemSize + 2] = (byte)((value >> 16) & 0xFF);
                        }
                    }

                    output.Write(buffer, 0, totalBytes);
                    i += elements;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Reads a single 24-bit little-endian encoded unsigned value into a <see cref="DoubleDataValue"/>.
        /// </summary>
        /// <param name="input3Bytes">A span containing at least 3 bytes (little-endian) representing an unsigned 24-bit value.</param>
        /// <returns>The decoded <see cref="DoubleDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DoubleDataValue ReadOne(ReadOnlySpan<byte> input3Bytes)
        {
            uint b0 = input3Bytes[0];
            uint b1 = (uint)input3Bytes[1] << 8;
            uint b2 = (uint)input3Bytes[2] << 16;
            uint value = b0 | b1 | b2;
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DoubleDataValue(value, DataValueType.Exists)
                : new DoubleDataValue(0, type);
        }

        /// <summary>
        /// Reads a single 24-bit little-endian encoded unsigned value into a <see cref="DecimalDataValue"/>.
        /// </summary>
        /// <param name="input3Bytes">A span containing at least 3 bytes (little-endian) representing an unsigned 24-bit value.</param>
        /// <returns>The decoded <see cref="DecimalDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DecimalDataValue ReadOneAsDecimal(ReadOnlySpan<byte> input3Bytes)
        {
            uint b0 = input3Bytes[0];
            uint b1 = (uint)input3Bytes[1] << 8;
            uint b2 = (uint)input3Bytes[2] << 16;
            uint value = b0 | b1 | b2;
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
            const int elemSize = 3;
            int count = Math.Min(input.Length / elemSize, output.Length);
            for (int i = 0; i < count; i++)
            {
                output[i] = ReadOne(input.Slice(i * elemSize, elemSize));
            }
        }

        /// <summary>
        /// Reads 24-bit little-endian encoded values from input bytes into a span of <see cref="DecimalDataValue"/>.
        /// </summary>
        /// <param name="input">Source bytes to decode.</param>
        /// <param name="output">Destination span for decoded values.</param>
        public void Read(ReadOnlySpan<byte> input, Span<DecimalDataValue> output)
        {
            const int elemSize = 3;
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
