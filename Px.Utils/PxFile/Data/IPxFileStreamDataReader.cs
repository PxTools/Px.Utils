using PxUtils.Models.Data.DataValue;

namespace PxUtils.PxFile.Data
{
    public interface IPxFileStreamDataReader : IDisposable
    {
        public void ReadUnsafeDoubles(double[] buffer, int offset, int[] rows, int[] cols, double[] missingValueEncodings);
        public void ReadDoubleDataValues(DoubleDataValue[] buffer, int offset, int[] rows, int[] cols);
        public void ReadDecimalDataValues(DecimalDataValue[] buffer, int offset, int[] rows, int[] cols);
        public Task ReadUnsafeDoublesAsync(double[] buffer, int offset, int[] rows, int[] cols, double[] missingValueEncodings);
        public Task ReadUnsafeDoublesAsync(double[] buffer, int offset, int[] rows, int[] cols, double[] missingValueEncodings, CancellationToken cancellationToken);
        public Task ReadDoubleDataValuesAsync(DoubleDataValue[] buffer, int offset, int[] rows, int[] cols);
        public Task ReadDoubleDataValuesAsync(DoubleDataValue[] buffer, int offset, int[] rows, int[] cols, CancellationToken cancellationToken);
        public Task ReadDecimalDataValuesAsync(DecimalDataValue[] buffer, int offset, int[] rows, int[] cols);
        public Task ReadDecimalDataValuesAsync(DecimalDataValue[] buffer, int offset, int[] rows, int[] cols, CancellationToken cancellationToken);
    }
}
