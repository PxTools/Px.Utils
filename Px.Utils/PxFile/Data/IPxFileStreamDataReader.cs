using Px.Utils.Models.Data.DataValue;

namespace Px.Utils.PxFile.Data
{
    public interface IPxFileStreamDataReader
    {
        /// <summary>
        /// Reads a specified set of data values into a buffer as <see cref="double"/>s.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
        /// <param name="missingValueEncodings"> An array of <see cref="double"/> values that represent missing data.</param>
        public void ReadUnsafeDoubles(double[] buffer, int offset, DataIndexer indexer, double[] missingValueEncodings);

        /// <summary>
        /// Reads a specified set of data values into a buffer as <see cref="DoubleDataValue"/> instances.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
        public void ReadDoubleDataValues(DoubleDataValue[] buffer, int offset, DataIndexer indexer);

        /// <summary>
        /// Reads a specified set of data values into a buffer as <see cref="DecimalDataValue"/> instances.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
        public void ReadDecimalDataValues(DecimalDataValue[] buffer, int offset, DataIndexer indexer);

        /// <summary>
        /// Asynchronously reads a specified set of data values into a buffer as <see cref="double"/>s.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
        /// <param name="missingValueEncodings"> An array of <see cref="double"/> values that represent missing data.</param>
        public Task ReadUnsafeDoublesAsync(double[] buffer, int offset, DataIndexer indexer, double[] missingValueEncodings);

        /// <summary>
        /// Asynchronously reads a specified set of data values into a buffer as <see cref="double"/>s.
        /// This method can be cancelled using a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
        /// <param name="missingValueEncodings"> An array of double values that represent missing data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        public Task ReadUnsafeDoublesAsync(double[] buffer, int offset, DataIndexer indexer, double[] missingValueEncodings, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously reads a specified set of data values into a buffer as <see cref="DoubleDataValue"/> instances.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
        public Task ReadDoubleDataValuesAsync(DoubleDataValue[] buffer, int offset, DataIndexer indexer);

        /// <summary>
        /// Asynchronously reads a specified set of data values into a buffer as <see cref="DoubleDataValue"/> instances.
        /// This method can be cancelled using a CancellationToken.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        public Task ReadDoubleDataValuesAsync(DoubleDataValue[] buffer, int offset, DataIndexer indexer, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously reads a specified set of data values into a buffer as <see cref="DecimalDataValue"/> instances.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
        public Task ReadDecimalDataValuesAsync(DecimalDataValue[] buffer, int offset, DataIndexer indexer);

        /// <summary>
        /// Asynchronously reads a specified set of data values into a buffer as <see cref="DecimalDataValue"/> instances.
        /// This method can be cancelled using a CancellationToken.
        /// </summary>
        /// <param name="buffer">The buffer to store the read values.</param>
        /// <param name="offset">The starting index in the buffer to begin storing the read values.</param>
        /// <param name="indexer">Provides the indexes where the data will be read.</param>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        public Task ReadDecimalDataValuesAsync(DecimalDataValue[] buffer, int offset, DataIndexer indexer, CancellationToken cancellationToken);
    }
}
