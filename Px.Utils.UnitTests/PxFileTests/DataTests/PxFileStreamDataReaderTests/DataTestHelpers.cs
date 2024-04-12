namespace PxFileTests.DataTests.PxFileStreamDataReaderTests
{
    internal static class DataTestHelpers
    {
        internal static int[] BuildRanges(int start, int end, int step)
        {
            List<int> ranges = [];
            for (int i = start; i < end; i += step)
            {
                ranges.Add(i);
            }
            return [.. ranges];
        }
    }
}
