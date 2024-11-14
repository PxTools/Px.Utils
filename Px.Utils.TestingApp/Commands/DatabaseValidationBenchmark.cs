using Px.Utils.Validation.DatabaseValidation;

namespace Px.Utils.TestingApp.Commands
{
    internal sealed class DatabaseValidationBenchmark : Benchmark
    {
        internal override string Help => "Validates a px path database.";

        internal override string Description => "Validates a px path database.";
        private static readonly string[] directoryFlags = ["-d", "-directory"];

        private DatabaseValidator? validator;

        internal DatabaseValidationBenchmark()
        {
            ParameterFlags.Add(directoryFlags);
            BenchmarkFunctions = [ValidationBenchmark];
            BenchmarkFunctionsAsync = [ValidationBenchmarkAsync];
        }
        
        protected override void SetRunParameters()
        {
            foreach (string key in directoryFlags)
            {
                if (Parameters.TryGetValue(key, out List<string>? value) && value.Count == 1)
                {
                    TestFilePath = value[0];
                    base.SetRunParameters();
                    return;
                }
            }

            throw new ArgumentException("Directory not found.");
        }

        protected override void StartInteractiveMode()
        {
            base.StartInteractiveMode();

            Console.WriteLine("Enter the path to the PX database root to benchmark");
            string path = Console.ReadLine() ?? "";

            while (!Directory.Exists(path))
            {
                Console.WriteLine("Path provided is not valid, please enter a path to a valid directory.");
                path = Console.ReadLine() ?? "";
            }

            TestFilePath = path;
        }

        protected override void OneTimeBenchmarkSetup()
        {
            base.OneTimeBenchmarkSetup();

            validator = new(TestFilePath, new LocalFileSystem());
        }

        private void ValidationBenchmark()
        {
            if(validator is null) throw new InvalidOperationException("Validator not initialized.");
            validator.Validate();
        }

        private async Task ValidationBenchmarkAsync()
        {
            if(validator is null) throw new InvalidOperationException("Validator not initialized.");
            await validator.ValidateAsync();
        }
    }
}
