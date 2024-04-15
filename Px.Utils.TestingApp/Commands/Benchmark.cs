using System;
using System.Diagnostics;
using System.Reflection;

namespace Px.Utils.TestingApp.Commands
{
    internal abstract class Benchmark : Command
    {
        internal class BenchmarkResult
        {
            internal readonly string Name;
            internal bool Error { get; }
            internal IReadOnlyList<double> IterationTimesMs { get; }
            internal double MinTimeMs => Math.Round(IterationTimesMs.Min(), 2);
            internal double MaxTimeMs => Math.Round(IterationTimesMs.Max(), 2);
            internal double MeanTimeMs => Math.Round(IterationTimesMs.Average(), 2);
            internal double MedianTimeMs
            {
                get
                {
                    List<double> sortedTimes = [.. IterationTimesMs.OrderBy(x => x)];
                    return Math.Round(sortedTimes[sortedTimes.Count / 2], 2);
                }
            }

            internal double StandardDeviation
            {
                get
                {
                    double sumOfSquaresOfDifferences = IterationTimesMs.Select(val => (val - MeanTimeMs) * (val - MeanTimeMs)).Sum();
                    return Math.Round(Math.Sqrt(sumOfSquaresOfDifferences / IterationTimesMs.Count), 2);
                }
            }

            internal BenchmarkResult(string name, IReadOnlyList<double> iterationTimesMs)
            {
                Name = name;
                IterationTimesMs = iterationTimesMs;
            }
        }
        internal Action[] BenchmarkFunctions { get; set; } = [];
        internal Func<Task>[] BenchmarkFunctionsAsync { get; set; } = [];

        internal string TestFilePath { get; set; } = "";
        internal int Iterations { get; set; } = 10;
        internal bool BatchMode { get; }
        internal List<string>? Inputs { get; set; } = [];
        
        private static readonly string[] fileFlags = ["-f", "-file"];
        private static readonly string[] iterFlags = ["-i", "-iter"];

        protected List<string[]> ParameterFlags { get; } = [fileFlags, iterFlags];

        internal List<BenchmarkResult> Results { get; } = [];
        private int processesCompleted = 0;

        internal void PrintResults()
        {
            int longestNameLength = Results.Max(x => x.Name.Length);
            if (longestNameLength < 4) longestNameLength = 4;
            string format = "| {0,-" + longestNameLength + "} | {1,12} | {2,12} | {3,12} | {4,12} | {5,12} |";

            Console.WriteLine($"Benchmark results for {Iterations} iterations:");
            Console.WriteLine(new string('-', longestNameLength + 79));
            Console.WriteLine(format, "Name", "Min (ms)", "Max (ms)", "Mean (ms)", "Median (ms)", "Std. dev.");

            Console.WriteLine(new string('-', longestNameLength + 79));
            foreach (BenchmarkResult result in Results)
            {
                Console.WriteLine(format, result.Name, result.MinTimeMs, result.MaxTimeMs, result.MeanTimeMs, result.MedianTimeMs, result.StandardDeviation);
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
            BenchmarkSetup();

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

            while (!Path.Exists(file) || !Path.GetFileName(file).EndsWith(".px"))
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

        protected virtual void BenchmarkSetup()
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

                for (int i = 0; i < Iterations; i++)
                {
                    stopwatch.Start();
                    function();
                    stopwatch.Stop();
                    iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Reset();
                    UpdateProgressText();
                }

                Results.Add(new BenchmarkResult(name, iterationTimes));
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
                    await task();
                    stopwatch.Stop();
                    iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Reset();
                    UpdateProgressText();
                }
                Results.Add(new BenchmarkResult(name, iterationTimes));
            }
        }

        private void UpdateProgressText()
        {
            int totalProcesses = Iterations * (BenchmarkFunctions.Length + BenchmarkFunctionsAsync.Length);
            processesCompleted++;
            if (processesCompleted > 1)
            {
                Console.CursorLeft = 0;
                Console.Write(new string(' ', Console.WindowWidth)); // Clear the entire line
                Console.CursorLeft = 0;
            }
            float progress = (float)processesCompleted / (totalProcesses);
            Console.Write($"Progress: {Math.Round(progress * 100f)}% - {processesCompleted} operations completed out of total {totalProcesses}");
            if (processesCompleted == totalProcesses)
            {
                Console.WriteLine();
            }
        }
    }
}
