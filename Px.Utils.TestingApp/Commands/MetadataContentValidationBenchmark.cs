namespace Px.Utils.TestingApp.Commands
{
    internal class MetadataContentValidationBenchmark : Benchmark
    {
        internal override string Help =>
        "Validates the contents of the Px file metadata given amount of times." + Environment.NewLine +
        "\t-f, -file: The path to the px file to read." + Environment.NewLine +
        "\t-i, -iter: The number of iterations to run.";

        internal override string Description => "Benchmarks the metadata content validation of Px.Utils/Validation/SyntaxValidation.";

        internal MetadataContentValidationBenchmark()
        {             
            BenchmarkFunctions = [ValidateContentBenchmark];
            BenchmarkFunctionsAsync = [ValidateContentAsyncBenchmark];
        }

        private void ValidateContentBenchmark()
        {
        }

        private async Task ValidateContentAsyncBenchmark()
        {
        }
    }
}
