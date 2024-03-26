namespace Px.Utils.TestingApp.Commands
{
    internal abstract class Benchmarks
    {
        internal class BenchmarkResult
        {
            internal readonly string Name;
            internal bool Error { get; }
            internal IReadOnlyList<double> IterationTimesMs { get; }
            internal double AverageTimeMs => IterationTimesMs.Average();
            internal double MinTimeMs => IterationTimesMs.Min();
            internal double MaxTimeMs => IterationTimesMs.Max();
            internal double MedianTimeMs
            {
                get
                {
                    List<double> sortedTimes = [.. IterationTimesMs.OrderBy(x => x)];
                    return sortedTimes[sortedTimes.Count / 2];
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

        internal abstract void Run();

        internal void PrintResults()
        {
            Console.WriteLine($"Benchmark results for {Iterations} iterations:");
            Console.WriteLine("| Name\t\t| Min ms.\t| Max ms. \t| Avarage ms. \t| Median ms. \t|");
            Console.WriteLine("|---------------|---------------|---------------|---------------|---------------|");
        }
    }
}
