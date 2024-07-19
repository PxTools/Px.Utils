using Px.Utils.Validation.DatabaseValidation;

namespace Px.Utils.TestingApp.Commands
{
    internal class DatabaseValidationBenchmark : FileBenchmark
    {
        internal override string Help => "Validates a px file database.";

        internal override string Description => "Validates a px file database.";

        private DatabaseValidator validator;

        internal DatabaseValidationBenchmark()
        { 
            BenchmarkFunctions = [ValidationBenchmark];
            BenchmarkFunctionsAsync = [ValidationBenchmarkAsync];
        }

        protected override void OneTimeBenchmarkSetup()
        {
            base.OneTimeBenchmarkSetup();

            validator = new(TestFilePath);
        }

        protected void ValidationBenchmark()
        {
            validator.Validate();
        }

        protected async Task ValidationBenchmarkAsync()
        {
            await validator.ValidateAsync();
        }
    }
}
