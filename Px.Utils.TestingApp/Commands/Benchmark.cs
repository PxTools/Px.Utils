using System.Diagnostics;
using System.Reflection;

namespace Px.Utils.TestingApp.Commands
{
    /// <summary>
    /// Base class for benchmarking commands. Contains methods for running benchmarks synchronously and asynchronously and printing the results.
    /// </summary>
    internal abstract class Benchmark : Command
    {
        /// <summary>
        /// Represents one result of a benchmark run. Contains iteration times and metrics such as min, max, mean, median and standard deviation calculated from the iteration times.
        /// </summary>
        internal sealed class BenchmarkResult
        {
            internal readonly string Name;
            internal bool Error { get; }
            internal IReadOnlyList<double> IterationTimesMs { get; }
            internal double MinTimeMs => Math.Round(IterationTimesMs.Min(), decimalPlaces);
            internal double MaxTimeMs => Math.Round(IterationTimesMs.Max(), decimalPlaces);
            internal double MeanTimeMs => Math.Round(IterationTimesMs.Average(), decimalPlaces);
            internal double MedianTimeMs
            {
                get
                {
                    List<double> sortedTimes = [.. IterationTimesMs.OrderBy(x => x)];
                    return Math.Round(sortedTimes[sortedTimes.Count / decimalPlaces], decimalPlaces);
                }
            }
            
            internal double StandardDeviation
            {
                get
                {
                    double sumOfSquaresOfDifferences = IterationTimesMs.Select(val => (val - MeanTimeMs) * (val - MeanTimeMs)).Sum();
                    return Math.Round(Math.Sqrt(sumOfSquaresOfDifferences / IterationTimesMs.Count), decimalPlaces);
                }
            }

            private const int decimalPlaces = 2;

            internal BenchmarkResult(string name, IReadOnlyList<double> iterationTimesMs)
            {
                Name = name;
                IterationTimesMs = iterationTimesMs;
                Error = iterationTimesMs.Count == 0;
            }
        }

        /// <summary>
        /// Path to the PX file subject to benchmark.
        /// </summary>
        internal string TestFilePath { get; set; } = "";
        /// <summary>
        /// How many times to run each benchmark.
        /// </summary>
        internal int Iterations { get; set; } = 10;
        /// <summary>
        /// Whether the user provided all benchmark parameters in one go.
        /// </summary>
        internal bool BatchMode { get; }
        /// <summary>
        /// List of inputs provided by the user.
        /// </summary>
        internal List<string>? Inputs { get; set; } = [];

        /// <summary>
        /// Synchronous benchmark functions to benchmark.
        /// </summary>
        protected Action[] BenchmarkFunctions { get; set; } = [];
        /// <summary>
        /// Asynchronous benchmarks to benchmark.
        /// </summary>
        protected Func<Task>[] BenchmarkFunctionsAsync { get; set; } = [];

        private static readonly string[] fileFlags = ["-f", "-file"];
        private static readonly string[] iterFlags = ["-i", "-iter"];

        /// <summary>
        /// List of flags that can be used to provide parameters to the benchmark command.
        /// </summary>
        protected List<string[]> ParameterFlags { get; } = [fileFlags, iterFlags];

        internal List<BenchmarkResult> Results { get; } = [];
        private int processesCompleted;
        private const int minResultsLongestNameLength = 4;
        private const int resultsTableWidth = 79;

        internal void PrintResults()
        {
            int longestNameLength = Results.Max(x => x.Name.Length);
            if (longestNameLength < minResultsLongestNameLength) longestNameLength = minResultsLongestNameLength;
            string format = "| {0,-" + longestNameLength + "} | {1,12} | {2,12} | {3,12} | {4,12} | {5,12} |";

            Console.WriteLine($"Benchmark results for {Iterations} iterations:");
            Console.WriteLine(new string('-', longestNameLength + resultsTableWidth));
            Console.WriteLine(format, "Name", "Min (ms)", "Max (ms)", "Mean (ms)", "Median (ms)", "Std. dev.");

            Console.WriteLine(new string('-', longestNameLength + resultsTableWidth));
            foreach (BenchmarkResult result in Results)
            {
                if(result.Error)
                {
                    string errorFormatString = "| {0,-" + longestNameLength + "} | {1,12} " + new string(' ', 60) + "|";
                    Console.WriteLine(errorFormatString, result.Name, "Error");
                }
                else
                {
                    Console.WriteLine(format, result.Name, result.MinTimeMs, result.MaxTimeMs, result.MeanTimeMs, result.MedianTimeMs, result.StandardDeviation);
                }
            }
        }

        internal override void Run(bool batchMode, List<string>? inputs = null)
        {
            Inputs = inputs;
            if (inputs?.Count == 1 && inputs[0] == "help")
            {
                Console.Clear();
                Console.WriteLine(Help);
                Console.WriteLine();
                Inputs = [];
            }

            SetRunParameters();
            OneTimeBenchmarkSetup();

            // synchronous validation
            RunBenchmarks(BenchmarkFunctions);

            // async validation
            RunBenchmarksAsync(BenchmarkFunctionsAsync).Wait();
        }

        protected virtual void SetRunParameters()
        {
            Dictionary<string, List<string>> parameters = GroupParameters(Inputs ?? [], ParameterFlags.SelectMany(x => x).ToList());
            if (parameters.Keys.Count == ParameterFlags.Count)
            {
                foreach (string key in parameters.Keys)
                {
                    if (fileFlags.Contains(key) && parameters[key].Count == 1)
                    {
                        TestFilePath = parameters[key][0];
                    }
                    else if (iterFlags.Contains(key) && parameters[key].Count == 1 &&
                        int.TryParse(parameters[key][0], out int iterations))
                    {
                        Iterations = iterations;
                    }
                    else if (Array.Exists(ParameterFlags.ToArray(), flag => flag.Contains(key)))
                    {
                        continue;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid argument {key} {string.Join(' ', parameters[key])}");
                    }
                }
            }
            else if (BatchMode)
            {
                throw new ArgumentException("Invalid number of parameters.");
            }
            else
            {
                StartInteractiveMode();
            }
        }

        protected virtual void StartInteractiveMode()
        {
            Console.WriteLine("Enter the path to the PX file to benchmark");
            string file = Console.ReadLine() ?? "";

            while (!Path.Exists(file) || !Path.GetFileName(file).EndsWith(".px", StringComparison.Ordinal))
            {
                Console.WriteLine("File provided is not valid, please enter a path to a valid px file");
                file = Console.ReadLine() ?? "";
            }

            TestFilePath = file;

            Console.WriteLine("Enter the number of iterations to run");
            string iterations = Console.ReadLine() ?? "";
            int value;
            while (!int.TryParse(iterations, out value))
            {
                Console.WriteLine("Invalid number of iterations, please enter a valid integer");
                iterations = Console.ReadLine() ?? "";
            }

            Iterations = value;
        }

        /// <summary>
        /// Setup method for the benchmark. Called before running the benchmarks. Marked as virtual to allow for custom setup in derived classes.
        /// </summary>
        protected virtual void OneTimeBenchmarkSetup()
        {
            Results.Clear();
            processesCompleted = 0;
        }

        protected void RunBenchmarks(Action[] functions)
        {
            foreach (Action function in functions)
            {
                string name = function.GetMethodInfo().Name;
                Stopwatch stopwatch = new();
                List<double> iterationTimes = [];

                bool error = false;

                for (int i = 0; i < Iterations; i++)
                {
                    stopwatch.Start();
                    try
                    {
                        function();
                    }
                    catch
                    {
                        error = true;
                        UpdateProgress();
                        continue;
                    }
                    stopwatch.Stop();
                    iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Reset();
                    UpdateProgress();
                }

                if (error) Results.Add(new BenchmarkResult(name, []));
                else Results.Add(new BenchmarkResult(name, iterationTimes));
            }
        }

        protected async Task RunBenchmarksAsync(Func<Task>[] tasks)
        {
            foreach (Func<Task> task in tasks)
            {
                string name = task.GetMethodInfo().Name;

                Stopwatch stopwatch = new();
                List<double> iterationTimes = [];

                for (int i = 0; i < Iterations; i++)
                {
                    stopwatch.Start();
                    try
                    {
                        await task();
                    }
                    catch
                    {
                        Results.Add(new BenchmarkResult(name, []));
                        UpdateProgress(Iterations - i);
                        stopwatch.Stop();
                        stopwatch.Reset();
                        return;
                    }
                    iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Reset();
                    UpdateProgress();
                }
                Results.Add(new BenchmarkResult(name, iterationTimes));
            }
        }

        private void UpdateProgress(int increment = 1)
        {
            int totalProcesses = Iterations * (BenchmarkFunctions.Length + BenchmarkFunctionsAsync.Length);
            processesCompleted += increment;
            if (processesCompleted > 1)
            {
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
            double progress = (double)processesCompleted / totalProcesses;

            // Progress bar
            const int progressBarLength = 100;
            int progressBlocks = (int)(progress * progressBarLength);
            Console.Write(new string('█', progressBlocks));
            Console.Write(new string('░', progressBarLength - progressBlocks));
            Console.WriteLine();
            const double percentageMultiplier = 100f;
            Console.Write($"Progress: {Math.Round(progress * percentageMultiplier)}% - {processesCompleted} operations completed out of total {totalProcesses}");

            if (processesCompleted == totalProcesses)
            {
                Console.WriteLine();
            }
        }
    }
}
