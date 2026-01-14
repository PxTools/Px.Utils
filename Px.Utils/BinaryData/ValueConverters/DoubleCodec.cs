using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Codec for reading and writing 64-bit floating point values with sentinel-based <see cref="DataValueType"/> mapping.
    /// Uses the sequence of smallest positive subnormal bit patterns as sentinels and preserves the exact bit pattern when writing.
    /// </summary>
    /// <param name="bufferBytes">The size of the internal write buffer in bytes. The minimum effective size is <c>sizeof(double)</c>.</param>
    public sealed class DoubleCodec(int bufferBytes = 64 * 1024) : IBinaryValueCodec
    {
        /// <summary>
        /// Gets the number of bytes per encoded value for this codec.
        /// </summary>
        public static int ByteCount => sizeof(double);

        // Use contiguous smallest positive subnormals by bit pattern for double
        private const long MissingSentinelBits = 0x0000000000000001L;
        private const long CanNotRepresentSentinelBits = MissingSentinelBits + 1L;
        private const long ConfidentialSentinelBits = MissingSentinelBits + 2L;
        private const long NotAcquiredSentinelBits = MissingSentinelBits + 3L;
        private const long NotAskedSentinelBits = MissingSentinelBits + 4L;
        private const long EmptySentinelBits = MissingSentinelBits + 5L;
        private const long NillSentinelBits = MissingSentinelBits + 6L;

        private static readonly double Missing = BitConverter.Int64BitsToDouble(MissingSentinelBits);
        private static readonly double CanNotRepresent = BitConverter.Int64BitsToDouble(CanNotRepresentSentinelBits);
        private static readonly double Confidential = BitConverter.Int64BitsToDouble(ConfidentialSentinelBits);
        private static readonly double NotAcquired = BitConverter.Int64BitsToDouble(NotAcquiredSentinelBits);
        private static readonly double NotAsked = BitConverter.Int64BitsToDouble(NotAskedSentinelBits);
        private static readonly double Empty = BitConverter.Int64BitsToDouble(EmptySentinelBits);
        private static readonly double Nill = BitConverter.Int64BitsToDouble(NillSentinelBits);

        private readonly int _bufferBytes = Math.Max(sizeof(double), bufferBytes);

        private static double MapTo(DataValueType type)
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

        private static DataValueType MapFrom(double value)
        {
            long bits = BitConverter.DoubleToInt64Bits(value);
            if (bits >= MissingSentinelBits && bits <= NillSentinelBits)
            {
                return bits switch
                {
                    MissingSentinelBits => DataValueType.Missing,
                    CanNotRepresentSentinelBits => DataValueType.CanNotRepresent,
                    ConfidentialSentinelBits => DataValueType.Confidential,
                    NotAcquiredSentinelBits => DataValueType.NotAcquired,
                    NotAskedSentinelBits => DataValueType.NotAsked,
                    EmptySentinelBits => DataValueType.Empty,
                    NillSentinelBits => DataValueType.Nill,
                    _ => DataValueType.Exists
                };
            }
            return DataValueType.Exists;
        }

        /// <summary>
        /// Reads a single double-based data value from an 8-byte little-endian buffer.
        /// </summary>
        /// <param name="bytes">A span containing at least 8 bytes (little-endian) representing a double.</param>
        /// <returns>The decoded <see cref="DoubleDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DoubleDataValue ReadOne(ReadOnlySpan<byte> bytes)
        {
            long bits = BinaryPrimitives.ReadInt64LittleEndian(bytes);
            double value = BitConverter.Int64BitsToDouble(bits);
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DoubleDataValue(value, DataValueType.Exists)
                : new DoubleDataValue(0, type);
        }

        /// <summary>
        /// Reads a single double-based data value from an 8-byte little-endian buffer and converts to decimal.
        /// </summary>
        /// <param name="bytes">A span containing at least 8 bytes (little-endian) representing a double.</param>
        /// <returns>The decoded <see cref="DecimalDataValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DecimalDataValue ReadOneAsDecimal(ReadOnlySpan<byte> bytes)
        {
            long bits = BinaryPrimitives.ReadInt64LittleEndian(bytes);
            double value = BitConverter.Int64BitsToDouble(bits);
            DataValueType type = MapFrom(value);
            return type == DataValueType.Exists
                ? new DecimalDataValue((decimal)value, DataValueType.Exists)
                : new DecimalDataValue(0, type);
        }

        /// <summary>
        /// Writes a span of <see cref="DoubleDataValue"/> entries to the output stream using 64-bit little-endian encoding of double.
        /// Values with non-<see cref="DataValueType.Exists"/> types are mapped to reserved sentinel bit patterns.
        /// </summary>
        /// <param name="input">Input values to encode.</param>
        /// <param name="output">Destination stream to write to.</param>
        public void Write(ReadOnlySpan<DoubleDataValue> input, Stream output)
        {
            ArgumentNullException.ThrowIfNull(output);

            const int elemSize = sizeof(double);
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
                        double value = dv.Type == DataValueType.Exists ? dv.UnsafeValue : MapTo(dv.Type);
                        // Convert double to its bit representation to preserve subnormal values exactly
                        long bits = BitConverter.DoubleToInt64Bits(value);
                        BinaryPrimitives.WriteInt64LittleEndian(span.Slice(j * elemSize, elemSize), bits);
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
        /// Reads 64-bit little-endian encoded double values from input bytes into a span of <see cref="DoubleDataValue"/>.
        /// </summary>
        /// <param name="input">Source bytes to decode.</param>
        /// <param name="output">Destination span for decoded values.</param>
        public void Read(ReadOnlySpan<byte> input, Span<DoubleDataValue> output)
        {
            const int elemSize = sizeof(double);
            int count = Math.Min(input.Length / elemSize, output.Length);
            for (int i = 0; i < count; i++)
            {
                output[i] = ReadOne(input.Slice(i * elemSize, elemSize));
            }
        }

        /// <summary>
        /// Reads 64-bit little-endian encoded double values from input bytes into a span of <see cref="DecimalDataValue"/>.
        /// </summary>
        /// <param name="input">Source bytes to decode.</param>
        /// <param name="output">Destination span for decoded values.</param>
        public void Read(ReadOnlySpan<byte> input, Span<DecimalDataValue> output)
        {
            const int elemSize = sizeof(double);
            int count = Math.Min(input.Length / elemSize, output.Length);
            for (int i = 0; i < count; i++)
            {
                output[i] = ReadOneAsDecimal(input.Slice(i * elemSize, elemSize));
            }
        }
    }
}
