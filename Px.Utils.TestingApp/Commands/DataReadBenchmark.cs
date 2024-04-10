using PxUtils.Models.Data.DataValue;
using PxUtils.PxFile.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Px.Utils.TestingApp.Commands
{
    internal class DataReadBenchmark : Benchmark
    {
        private string _testFilePath = "";
        private int[] _readRows = [];
        private int[] _readCols = [];

        private readonly string[] fileFlags = ["-f", "-file"];
        private readonly string[] iterFlags = ["-i", "-iter"];
        private readonly string[] rowsFlags = ["-r", "-rows"];
        private readonly string[] colsFlags = ["-c", "-cols"];

        internal override string Help =>
        "Reads the defined dataset from the target px file the given amount of times." + Environment.NewLine +
        "\t-f, -file: The path to the px file to read." + Environment.NewLine +
        "\t-i, -iter: The number of iterations to run." + Environment.NewLine +
        "\t-r, -rows: The rows to read from the dataset." + Environment.NewLine +
        "\t-c, -cols: The columns to read from the dataset." + Environment.NewLine +
        "The rows and columns are defined as a space-separated list of integers or ranges of integers separated by '..'.";

        internal override string Description => "Benchmarks the data reading capabilities of the PxFileStreamDataReader.";

        internal override void Run(bool batchMode, List<string>? inputs = null)
        {
            if(inputs?.Count == 1 && inputs[0] == "help")
            {
                Console.Clear();
                Console.WriteLine(Help);
                Console.WriteLine();
                inputs = [];
            }

            SetRunParameters(batchMode, inputs);

            // synchronous read
            RunReadDoubleDataValuesBenchmarks();
            RunReadDecimalDataValuesBenchmarks();
            RunReadUnsafeDoubleBenchmarks();

            // async read
            RunReadDoubleDataValuesAsyncBenchmarks().Wait();
            RunReadDecimalDataValuesAsyncBenchmarks().Wait();
            RunReadUnsafeDoubleAsyncBenchmarks().Wait();
        }

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

        private void RunReadDecimalDataValuesBenchmarks()
        {
            Stopwatch stopwatch = new();
            List<double> iterationTimes = [];
            DecimalDataValue[] buffer = new DecimalDataValue[_readRows.Length * _readCols.Length];

            for (int i = 0; i < Iterations; i++)
            {
                stopwatch.Start();

                using Stream stream = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
                using PxFileStreamDataReader reader = new(stream);

                reader.ReadDecimalDataValues(buffer, 0, _readRows, _readCols);
                stopwatch.Stop();
                iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();
            }

            Results.Add(new BenchmarkResult("ReadDecimalDataValues", iterationTimes));
        }

        private void RunReadUnsafeDoubleBenchmarks()
        {
            Stopwatch stopwatch = new();
            List<double> iterationTimes = [];
            double[] buffer = new double[_readRows.Length * _readCols.Length];
            double[] missingValueEncodings = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0];

            for (int i = 0; i < Iterations; i++)
            {
                stopwatch.Start();

                using Stream stream = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
                using PxFileStreamDataReader reader = new(stream);

                reader.ReadUnsafeDoubles(buffer, 0, _readRows, _readCols, missingValueEncodings);
                stopwatch.Stop();
                iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();
            }

            Results.Add(new BenchmarkResult("ReadUnsafeDoubleValues", iterationTimes));
        }

        private async Task RunReadDoubleDataValuesAsyncBenchmarks()
        {
            Stopwatch stopwatch = new();
            List<double> iterationTimes = [];
            DoubleDataValue[] buffer = new DoubleDataValue[_readRows.Length * _readCols.Length];

            for (int i = 0; i < Iterations; i++)
            {
                stopwatch.Start();

                using Stream stream = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
                using PxFileStreamDataReader reader = new(stream);

                await reader.ReadDoubleDataValuesAsync(buffer, 0, _readRows, _readCols);
                stopwatch.Stop();
                iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();
            }

            Results.Add(new BenchmarkResult("ReadDoubleDataValuesAsync", iterationTimes));
        }

        private async Task RunReadDecimalDataValuesAsyncBenchmarks()
        {
            Stopwatch stopwatch = new();
            List<double> iterationTimes = [];
            DecimalDataValue[] buffer = new DecimalDataValue[_readRows.Length * _readCols.Length];

            for (int i = 0; i < Iterations; i++)
            {
                stopwatch.Start();

                using Stream stream = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
                using PxFileStreamDataReader reader = new(stream);

                await reader.ReadDecimalDataValuesAsync(buffer, 0, _readRows, _readCols);
                stopwatch.Stop();
                iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();
            }

            Results.Add(new BenchmarkResult("ReadDecimalDataValuesAsync", iterationTimes));
        }

        private async Task RunReadUnsafeDoubleAsyncBenchmarks()
        {
            Stopwatch stopwatch = new();
            List<double> iterationTimes = [];
            double[] buffer = new double[_readRows.Length * _readCols.Length];
            double[] missingValueEncodings = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0];

            for (int i = 0; i < Iterations; i++)
            {
                stopwatch.Start();

                using Stream stream = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
                using PxFileStreamDataReader reader = new(stream);

                await reader.ReadUnsafeDoublesAsync(buffer, 0, _readRows, _readCols, missingValueEncodings);
                stopwatch.Stop();
                iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();
            }

            Results.Add(new BenchmarkResult("ReadUnsafeDoubleValuesAsync", iterationTimes));
        }

        private void SetRunParameters(bool batchMode, List<string>? inputs)
        {
            Dictionary<string, List<string>> parameters = GroupParameters(inputs ?? [], [.. fileFlags, .. iterFlags, .. rowsFlags, .. colsFlags]);
            if(parameters.Keys.Count == 4)
            {
                foreach (string key in parameters.Keys)
                {
                    if (fileFlags.Contains(key) && parameters[key].Count == 1)
                    {
                        _testFilePath = parameters[key][0];
                    }
                    else if (iterFlags.Contains(key) && parameters[key].Count == 1 &&
                        int.TryParse(parameters[key][0], out int iterations))
                    {
                        Iterations = iterations;
                    }
                    else if (rowsFlags.Contains(key) && TryParseCoordinates(parameters[key], out _readRows))
                    {
                        continue;
                    }
                    else if (colsFlags.Contains(key) && TryParseCoordinates(parameters[key], out _readCols))
                    {
                        continue;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid argument {key} {string.Join(' ', parameters[key])}");
                    }
                }
            }
            else if( batchMode)
            {
                throw new ArgumentException("Invalid number of parameters.");
            }
            else
            {
                StartInteractiveMode();
            }
            
        }

        private void StartInteractiveMode()
        {
            Console.WriteLine("Enter the path to the PX file to benchmark");
            string file = Console.ReadLine() ?? "";

            while (!Path.Exists(file) || !Path.GetFileName(file).EndsWith(".px"))
            {
                Console.WriteLine("File provided is not valid, please enter a path to a valid px file");
                file = Console.ReadLine() ?? "";
            }

            _testFilePath = file;

            Console.WriteLine("Enter the number of iterations to run");
            string iterations = Console.ReadLine() ?? "";
            int value;
            while (!int.TryParse(iterations, out value))
            {
                Console.WriteLine("Invalid number of iterations, please enter a valid integer");
                iterations = Console.ReadLine() ?? "";
            }
            Iterations = value;

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
                if (Regex.IsMatch(part, rangePattern))
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
