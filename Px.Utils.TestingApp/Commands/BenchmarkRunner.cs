namespace Px.Utils.TestingApp.Commands
{
    internal sealed class BenchmarkRunner : Command
    {
        private readonly Dictionary<string, Benchmark> _benchmarks = [];

        internal BenchmarkRunner()
        {
            _benchmarks.Add("dataread", new DataReadBenchmark());
            _benchmarks.Add("metadata-reader", new MetadataReaderBenchmark());
            _benchmarks.Add("metadata-builder", new MetadataBuilderBenchmark());
            _benchmarks.Add("metadata-syntaxvalidation", new MetadataSyntaxValidationBenchmark());
            _benchmarks.Add("metadata-contentvalidation", new MetadataContentValidationBenchmark());
            _benchmarks.Add("data-validation", new DataValidationBenchmark());
            _benchmarks.Add("file-validation", new PxFileValidationBenchmark());
            _benchmarks.Add("computation", new ComputationBenchmark());
            _benchmarks.Add("database-validation", new DatabaseValidationBenchmark());
        }

        internal override string Help
        {
            get
            {
                Console.Clear();
                string help = "Available benchmarks:" + Environment.NewLine;
                return help + string.Join(Environment.NewLine, _benchmarks.Select(b => $"\t{b.Key}: {b.Value.Description}{Environment.NewLine}"));
            }
        }

        internal override string Description => "Run a set of benchmarks.";

        internal override void Run(bool batchMode, List<string>? inputs = null)
        {
            const string q = "Enter the benchmark name and optional parameters:";
            const string followUp = "Valid benchmark name is required, please input the benchmark name:";

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
