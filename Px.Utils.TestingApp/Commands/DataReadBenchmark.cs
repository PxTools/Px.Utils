using Px.Utils.PxFile.Data;
using PxUtils.Models.Data.DataValue;
using PxUtils.PxFile.Data;
using System.Text.RegularExpressions;

namespace Px.Utils.TestingApp.Commands
{
    internal class DataReadBenchmark : Benchmark
    {
        private int[] _readRows = [];
        private int[] _readCols = [];

        private readonly string[] rowsFlags = ["-r", "-rows"];
        private readonly string[] colsFlags = ["-c", "-cols"];

        private readonly int[][] coordinates = [
                Enumerable.Range(330, 83).ToArray(),
                [1,2],
                [9, 10, 11],
                [0, 2],
                Enumerable.Range(110, 100).ToArray(),
                Enumerable.Range(1, 10).ToArray()
                ];

        private DataIndexer Indexer => new(coordinates, [414, 3, 12, 3, 215, 13]);

        internal override string Help =>
        "Reads the defined dataset from the target px file the given amount of times." + Environment.NewLine +
        "\t-f, -file: The path to the px file to read." + Environment.NewLine +
        "\t-i, -iter: The number of iterations to run." + Environment.NewLine +
        "\t-r, -rows: The rows to read from the dataset." + Environment.NewLine +
        "\t-c, -cols: The columns to read from the dataset." + Environment.NewLine +
        "The rows and columns are defined as a space-separated list of integers or ranges of integers separated by '..'.";

        internal override string Description => "Benchmarks the data reading capabilities of the PxFileStreamDataReader.";

        internal DataReadBenchmark()
        {
            BenchmarkFunctions = [RunReadDoubleDataValuesBenchmarks, RunReadDecimalDataValuesBenchmarks, RunReadUnsafeDoubleBenchmarks];
            BenchmarkFunctionsAsync = [RunReadDoubleDataValuesAsyncBenchmarks, RunReadDecimalDataValuesAsyncBenchmarks, RunReadUnsafeDoubleAsyncBenchmarks];
            ParameterFlags.AddRange([rowsFlags, colsFlags]);
        }

        private void RunReadDoubleDataValuesBenchmarks()
        {
            DoubleDataValue[] buffer = new DoubleDataValue[_readRows.Length * _readCols.Length];
           
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            using PxFileStreamDataReader reader = new(stream);
            
            reader.ReadDoubleDataValues(buffer, 0, Indexer);
        }

        private void RunReadDecimalDataValuesBenchmarks()
        {
            DecimalDataValue[] buffer = new DecimalDataValue[_readRows.Length * _readCols.Length];
            
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            using PxFileStreamDataReader reader = new(stream);
            
            reader.ReadDecimalDataValues(buffer, 0, Indexer);
        }

        private void RunReadUnsafeDoubleBenchmarks()
        {
            double[] buffer = new double[_readRows.Length * _readCols.Length];
            double[] missingValueEncodings = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0];

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            using PxFileStreamDataReader reader = new(stream);

            reader.ReadUnsafeDoubles(buffer, 0, Indexer, missingValueEncodings);
        }

        private async Task RunReadDoubleDataValuesAsyncBenchmarks()
        {
            DoubleDataValue[] buffer = new DoubleDataValue[_readRows.Length * _readCols.Length];

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            using PxFileStreamDataReader reader = new(stream);

            await reader.ReadDoubleDataValuesAsync(buffer, 0, Indexer);
        }

        private async Task RunReadDecimalDataValuesAsyncBenchmarks()
        {
            DecimalDataValue[] buffer = new DecimalDataValue[_readRows.Length * _readCols.Length];

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            using PxFileStreamDataReader reader = new(stream);

            await reader.ReadDecimalDataValuesAsync(buffer, 0, Indexer);
        }

        private async Task RunReadUnsafeDoubleAsyncBenchmarks()
        {
            double[] buffer = new double[_readRows.Length * _readCols.Length];
            double[] missingValueEncodings = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0];

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            using PxFileStreamDataReader reader = new(stream);

            await reader.ReadUnsafeDoublesAsync(buffer, 0, Indexer, missingValueEncodings);
        }

        protected override void SetRunParameters()
        {
            base.SetRunParameters();

            Dictionary<string, List<string>> parameters = GroupParameters(Inputs ?? [], ParameterFlags.SelectMany(x => x).ToList());
            if (parameters.Keys.Count == 4)
            {
                foreach (string key in parameters.Keys)
                {
                    if (rowsFlags.Contains(key) && TryParseCoordinates(parameters[key], out _readRows))
                    {
                        continue;
                    }
                    else if (colsFlags.Contains(key) && TryParseCoordinates(parameters[key], out _readCols))
                    {
                        continue;
                    }
                    else if (Array.Exists(ParameterFlags.ToArray(), flags => flags.Contains(key)))
                    {
                        continue;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid argument {key} {string.Join(' ', parameters[key])}");
                    }
                }
            }
        }

        protected override void StartInteractiveMode()
        {
            base.StartInteractiveMode();

            Console.WriteLine("Enter the rows or row ranges to read, separated by spaces");
            string rows = Console.ReadLine() ?? "";
            while (!TryParseCoordinates([.. rows.Split(" ")], out _readRows))
            {
                Console.WriteLine("Invalid rows parameter, each row number or range must be separated by a space. Ranges need to be in the following format \"12..23\"");
                rows = Console.ReadLine() ?? "";
            }

            Console.WriteLine("Enter the columns or column ranges to read, separated by spaces");
            string cols = Console.ReadLine() ?? "";
            while (!TryParseCoordinates([.. cols.Split(" ")], out _readCols))
            {
                Console.WriteLine("Invalid columns parameter, each column number or range must be separated by a space. Ranges need to be in the following format \"12..23\"");
                cols = Console.ReadLine() ?? "";
            }
        }

        private static bool TryParseCoordinates(List<string> input, out int[] indices)
        {
            string rangePattern = @"^\d{1,10}\.\.\d{1,10}$";
            List<int> builder = [];
            foreach (string part in input)
            {
                if (Regex.IsMatch(part, rangePattern, RegexOptions.None, TimeSpan.FromMilliseconds(100)))
                {
                    string[] ints = part.Split("..");
                    if (int.TryParse(ints[0], out int start) && int.TryParse(ints[1], out int end))
                    {
                        builder.AddRange(Enumerable.Range(start, end - start));
                    }
                }
                else if (int.TryParse(part, out int value))
                {
                    builder.Add(value);
                }
                else
                {
                    indices = [];
                    return false;
                }
            }

            int i = -1;
            foreach (int index in builder)
            {
                if (i >= index)
                {
                    indices = [];
                    return false;
                }
                i = index;
            }

            indices = [.. builder];
            return true;
        }
    }
}
