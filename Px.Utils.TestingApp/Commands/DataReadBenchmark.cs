using PxUtils.Models.Data.DataValue;
using PxUtils.PxFile.Data;
using System.Diagnostics;

namespace Px.Utils.TestingApp.Commands
{
    internal class DataReadBenchmark : Benchmark
    {
        private string _testFilePath;
        private int[] _readRows;
        private int[] _readCols;

        internal DataReadBenchmark(string testFilePath, int[] readRows, int[] readCols)
        {
            _testFilePath = testFilePath;
            _readRows = readRows;
            _readCols = readCols;
        }

        internal override void Run() {}

        private void RunReadDoubleDataValuesBenchmarks()
        {
            Stopwatch stopwatch = new();
            List<double> iterationTimes = [];
            DoubleDataValue[] buffer = new DoubleDataValue[_readRows.Length * _readCols.Length];

            for (int i = 0; i < Iterations; i++)
            {
                stopwatch.Start();

                using Stream stream = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
                using PxFileStreamDataReader reader = new(stream);

                reader.ReadDoubleDataValues(buffer, 0, _readRows, _readCols);
                stopwatch.Stop();
                iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();
            }

            Results.Add(new BenchmarkResult("ReadDoubleDataValues", iterationTimes));
        }
    }
}
