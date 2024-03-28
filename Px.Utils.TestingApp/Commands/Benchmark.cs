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

        internal int Iterations { get; set; } = 10;

        internal List<BenchmarkResult> Results { get; } = [];

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
    }
}
