using PxUtils.Models.Data.DataValue;

namespace PxUtils.PxFile.Data
{
    public interface IPxFileStreamDataReader
    {
        /// <summary>
        /// Reads a specified set of data values into a buffer as <see cref="double"/>s.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read.</param>
        /// <param name="cols">An array of column indices specifying which columns to read.</param>
        /// <param name="missingValueEncodings"> An array of <see cref="double"/> values that represent missing data.</param>
        public void ReadUnsafeDoubles(double[] buffer, int offset, int[] rows, int[] cols, double[] missingValueEncodings);

        /// <summary>
        /// Reads a specified set of data values into a buffer as <see cref="DoubleDataValue"/> instances.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read.</param>
        /// <param name="cols">An array of column indices specifying which columns to read.</param>
        public void ReadDoubleDataValues(DoubleDataValue[] buffer, int offset, int[] rows, int[] cols);

        /// <summary>
        /// Reads a specified set of data values into a buffer as <see cref="DecimalDataValue"/> instances.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read.</param>
        /// <param name="cols">An array of column indices specifying which columns to read.</param>
        public void ReadDecimalDataValues(DecimalDataValue[] buffer, int offset, int[] rows, int[] cols);

        /// <summary>
        /// Asynchronously reads a specified set of data values into a buffer as <see cref="double"/>s.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read.</param>
        /// <param name="cols">An array of column indices specifying which columns to read.</param>
        /// <param name="missingValueEncodings"> An array of <see cref="double"/> values that represent missing data.</param>
        public Task ReadUnsafeDoublesAsync(double[] buffer, int offset, int[] rows, int[] cols, double[] missingValueEncodings);

        /// <summary>
        /// Asynchronously reads a specified set of data values into a buffer as <see cref="double"/>s.
        /// This method can be cancelled using a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read.</param>
        /// <param name="cols">An array of column indices specifying which columns to read.</param>
        /// <param name="missingValueEncodings"> An array of double values that represent missing data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        public Task ReadUnsafeDoublesAsync(double[] buffer, int offset, int[] rows, int[] cols, double[] missingValueEncodings, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously reads a specified set of data values into a buffer as <see cref="DoubleDataValue"/> instances.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read.</param>
        /// <param name="cols">An array of column indices specifying which columns to read.</param>
        public Task ReadDoubleDataValuesAsync(DoubleDataValue[] buffer, int offset, int[] rows, int[] cols);

        /// <summary>
        /// Asynchronously reads a specified set of data values into a buffer as <see cref="DoubleDataValue"/> instances.
        /// This method can be cancelled using a CancellationToken.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read from the data section.</param>
        /// <param name="cols">An array of column indices specifying which columns to read from the data section.</param>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        public Task ReadDoubleDataValuesAsync(DoubleDataValue[] buffer, int offset, int[] rows, int[] cols, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously reads a specified set of data values into a buffer as <see cref="DecimalDataValue"/> instances.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read.</param>
        /// <param name="cols">An array of column indices specifying which columns to read.</param>
        public Task ReadDecimalDataValuesAsync(DecimalDataValue[] buffer, int offset, int[] rows, int[] cols);

        /// <summary>
        /// Asynchronously reads a specified set of data values into a buffer as <see cref="DecimalDataValue"/> instances.
        /// This method can be cancelled using a CancellationToken.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="rows">An array of row indices specifying which rows to read.</param>
        /// <param name="cols">An array of column indices specifying which columns to read.</param>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        public Task ReadDecimalDataValuesAsync(DecimalDataValue[] buffer, int offset, int[] rows, int[] cols, CancellationToken cancellationToken);
    }
}
