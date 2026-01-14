using System.Buffers;
using System.Runtime.CompilerServices;
using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Codec for reading and writing 24-bit signed integer values with sentinel-based <see cref="DataValueType"/> mapping.
    /// </summary>
    public sealed class Int24Codec(int bufferBytes = 64 * 1024) : IBinaryValueCodec
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
