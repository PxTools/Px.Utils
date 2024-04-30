using PxUtils.PxFile;
using PxUtils.PxFile.Data;
using PxUtils.Validation.DataValidation;
using System.Diagnostics;
using PxUtils.Validation;

namespace Px.Utils.TestingApp.Commands
{
    internal class DataValidationBenchmark : Benchmark
    {
        private string _testFilePath = "";

        private readonly string[] fileFlags = ["-f", "-file"];
        private readonly string[] iterFlags = ["-i", "-iter"];
        private readonly string[] expectedRowsFlags = ["-r", "-rows"];
        private readonly string[] expectedColsFlags = ["-c", "-cols"];

        private int expectedRows;
        private int expectedCols;

        internal override string Help =>
        "Reads the defined dataset from the target px file the given amount of times." + Environment.NewLine +
        "\t-f, -file: The path to the px file to read." + Environment.NewLine +
        "\t-i, -iter: The number of iterations to run.";

        internal override string Description => "Benchmarks the data reading capabilities of the PxFileStreamDataReader.";

        internal override void Run(bool batchMode, List<string>? inputs = null)
        {
            if (inputs?.Count == 1 && inputs[0] == "help")
            {
                Console.Clear();
                Console.WriteLine(Help);
                Console.WriteLine();
                inputs = [];
            }

            SetRunParameters(batchMode, inputs);

            ValidateDataTokenBenchmarks();
            ValidateDataTokenAsyncBenchmarks().Wait();
             
            // synchronous read
            ValidateDataBenchmarks();

            // async read
            ValidateDataAsyncBenchmarks().Wait();
            
        }

        private void ValidateDataBenchmarks()
        {
            Stopwatch stopwatch = new();
            List<double> iterationTimes = [];

            for (int i = 0; i < Iterations; i++)
            {

                using Stream stream = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
                string dataKeyword = "DATA";
                long start = StreamUtilities.FindKeywordPosition(stream, dataKeyword, PxFileSyntaxConf.Default);
                if (start == -1)
                {
                    throw new ArgumentException($"Could not find data keyword '{dataKeyword}'");
                }
                stream.Position = start + dataKeyword.Length + 1 + 2; // +1 to skip the '='

                stopwatch.Start();
                
                IEnumerable<ValidationFeedback> feedbacks = DataValidation.Validate(stream, expectedCols, expectedRows, 0, null);

                stopwatch.Stop();
                foreach (ValidationFeedback validationFeedback in feedbacks)
                {
                    Console.WriteLine($"Line {validationFeedback.Line}, Char {validationFeedback.Character}: " 
                                      + $"{validationFeedback.Rule} {validationFeedback.AdditionalInfo}");
                }

                iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();
            }

            Results.Add(new BenchmarkResult("ValidateData", iterationTimes));
        }
        
        private async Task ValidateDataTokenAsyncBenchmarks()
        {
            Stopwatch stopwatch = new();
            List<double> iterationTimes = [];

            for (int i = 0; i < Iterations; i++)
            {

                using Stream stream = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
                string dataKeyword = "DATA";
                long start = StreamUtilities.FindKeywordPosition(stream, dataKeyword, PxFileSyntaxConf.Default);
                if (start == -1)
                {
                    throw new ArgumentException($"Could not find data keyword '{dataKeyword}'");
                }
                stream.Position = start + dataKeyword.Length + 1 +2 ; // +1 to skip the '='

                stopwatch.Start();

                IAsyncEnumerable<Token> tokens = DataValidation.TokenizeAsync(stream, PxFileSyntaxConf.Default,null);
                
                int j = 0;
                await foreach (Token token in tokens)
                {
                    j++;
                }

                stopwatch.Stop();

                iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();
            }

            Results.Add(new BenchmarkResult("TokenizeAsync", iterationTimes));
        }
        private void ValidateDataTokenBenchmarks()
        {
            Stopwatch stopwatch = new();
            List<double> iterationTimes = [];

            for (int i = 0; i < Iterations; i++)
            {

                using Stream stream = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
                string dataKeyword = "DATA";
                long start = StreamUtilities.FindKeywordPosition(stream, dataKeyword, PxFileSyntaxConf.Default);
                if (start == -1)
                {
                    throw new ArgumentException($"Could not find data keyword '{dataKeyword}'");
                }
                stream.Position = start + dataKeyword.Length + 1 + 2; // +1 to skip the '='

                stopwatch.Start();

                IEnumerable<Token> tokens = DataValidation.Tokenize(stream, PxFileSyntaxConf.Default,null);
                
                int j = 0;
                foreach (Token token in tokens)
                {
                    j++;
                }

                stopwatch.Stop();

                iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();
            }

            Results.Add(new BenchmarkResult("Tokenize", iterationTimes));
        }
        
        
        private async Task ValidateDataAsyncBenchmarks()
        {
            Stopwatch stopwatch = new();
            List<double> iterationTimes = [];

            for (int i = 0; i < Iterations; i++)
            {

                using Stream stream = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
                string dataKeyword = "DATA";
                long start = StreamUtilities.FindKeywordPosition(stream, dataKeyword, PxFileSyntaxConf.Default);
                if (start == -1)
                {
                    throw new ArgumentException($"Could not find data keyword '{dataKeyword}'");
                }
                stream.Position = start + dataKeyword.Length + 1 + 2; // +1 to skip the '='

                stopwatch.Start();
                
                await DataValidation.ValidateAsync(stream, expectedCols, expectedRows, 0, null);

                stopwatch.Stop();
                iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Reset();
            }

            Results.Add(new BenchmarkResult("ValidateDataAsync", iterationTimes));
        }
        private void SetRunParameters(bool batchMode, List<string>? inputs)
        {
            Dictionary<string, List<string>> parameters = GroupParameters(inputs ?? [], [.. fileFlags, .. iterFlags, ..expectedRowsFlags, ..expectedColsFlags ]);
            if (parameters.Keys.Count == 4)
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
                    else if (expectedRowsFlags.Contains(key) && parameters[key].Count == 1 &&
                        int.TryParse(parameters[key][0], out int rows))
                    {
                        expectedRows = rows;
                    }
                    else if (expectedColsFlags.Contains(key) && parameters[key].Count == 1 &&
                        int.TryParse(parameters[key][0], out int cols))
                    {
                        expectedCols = cols;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid argument {key} {string.Join(' ', parameters[key])}");
                    }
                }
            }
            else if (batchMode)
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
            string iterationsStr = Console.ReadLine() ?? "";
            int iter;
            while (!int.TryParse(iterationsStr, out iter))
            {
                Console.WriteLine("Invalid number of iterations, please enter a valid integer");
                iterationsStr = Console.ReadLine() ?? "";
            }
            Iterations = iter;

            Console.WriteLine("Enter the number of rows the validator should expect the data section to have:");
            string expectedRowsStr = Console.ReadLine() ?? "";
            int rows;
            while (!int.TryParse(expectedRowsStr, out rows))
            {
                Console.WriteLine("Invalid number of iterations, please enter a valid integer");
                expectedRowsStr = Console.ReadLine() ?? "";
            }
            expectedRows = rows;

            Console.WriteLine("Enter the number of columns the validator should expect the data section to have:");
            string expextedColsStr = Console.ReadLine() ?? "";
            int cols;
            while (!int.TryParse(expextedColsStr, out cols))
            {
                Console.WriteLine("Invalid number of iterations, please enter a valid integer");
                expextedColsStr = Console.ReadLine() ?? "";
            }
            expectedCols = cols;
        }
    }
}
