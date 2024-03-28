namespace Px.Utils.TestingApp.Commands
{
    internal class BenchmarkRunner : Command
    {
        Dictionary<string, Benchmark> _benchmarks = [];

        internal BenchmarkRunner()
        {
            _benchmarks.Add("dataread", new DataReadBenchmark());
        }

        internal override string Help => string.Join(Environment.NewLine, _benchmarks.Select(b => $"{b.Key}:{Environment.NewLine}{b.Value.Help}{Environment.NewLine}"));

        internal override string Description => "Run a set of benchmarks.";

        internal override void Run(bool batchMode, List<string>? inputs = null)
        {
            string q = "Enter the benchmark name and optional parameters:";
            string followUp = "Valid benchmark name is required, please input the benchmark name:";

            if (inputs is null || inputs.Count == 0)
            {
                inputs = TestAppConsole.AskQuestion(q, true, followUp);
            }

            if (_benchmarks.TryGetValue(inputs[0], out Benchmark? benchmark))
            {
                benchmark.Run(batchMode, inputs.Skip(1).ToList());
                benchmark.PrintResults();
            }
            else if (inputs.Count == 1 && inputs[0] == "help")
            {
                Console.WriteLine(Help);
                inputs = TestAppConsole.AskQuestion(q, true, followUp);
                Run(batchMode, inputs);
            }
            else
            {
                Console.WriteLine($"Unknown benchmark {inputs[0]}");
            }
        }
    }
}
