using Px.Utils.Models.Data.DataValue;
using System.Buffers;

namespace Px.Utils.BinaryData.ValueConverters
{
    /// <summary>
    /// Base class for binary value codecs with shared write logic.
    /// </summary>
    /// <param name="elementSize">The size in bytes of each encoded element.</param>
    /// <param name="bufferBytes">The buffer size in bytes.</param>
    public abstract class BinaryValueCodecBase(int elementSize, int bufferBytes)
    {
        private readonly int _elementSize = elementSize;
        private readonly int _bufferBytes = Math.Max(elementSize, bufferBytes);
        private const int MinElementsPerChunk = 1;

        /// <summary>
        /// Writes a span of <see cref="DoubleDataValue"/> values to the output stream using the codec's encoding.
        /// </summary>
        /// <param name="input">Input values to encode.</param>
        /// <param name="output">Destination stream to write to.</param>
        public void Write(ReadOnlySpan<DoubleDataValue> input, Stream output)
        {
            ArgumentNullException.ThrowIfNull(output);

            byte[] buffer = ArrayPool<byte>.Shared.Rent(_bufferBytes);
            try
            {
                int maxElems = Math.Max(MinElementsPerChunk, buffer.Length / _elementSize);
                int i = 0;
                int count = input.Length;
                while (i < count)
                {
                    int elements = Math.Min(count - i, maxElems);
                    int totalBytes = elements * _elementSize;
                    Span<byte> span = buffer.AsSpan(0, totalBytes);

                    for (int j = 0; j < elements; j++)
                    {
                        WriteEncodedValue(span, j * _elementSize, input[i + j]);
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
        /// Encodes and writes the value to the buffer at the specified offset.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="offset">The offset in the buffer.</param>
        /// <param name="value">The value to encode and write.</param>
        protected abstract void WriteEncodedValue(Span<byte> buffer, int offset, DoubleDataValue value);
    }
}
