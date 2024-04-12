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

        internal int Iterations { get; set; } = 10;

        internal List<BenchmarkResult> Results { get; } = [];
        internal int processesCompleted = 0;

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
            if (inputs?.Count == 1 && inputs[0] == "help")
            {
                Console.Clear();
                Console.WriteLine(Help);
                Console.WriteLine();
                inputs = [];
            }

            SetRunParameters(batchMode, inputs);

            Results.Clear();
            processesCompleted = 0;

            // synchronous validation
            RunBenchmarks(BenchmarkFunctions);

            // async validation
            RunBenchmarksAsync(BenchmarkFunctionsAsync).Wait();
        }

        protected abstract void SetRunParameters(bool batchMode, List<string>? inputs);

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
