using Px.Utils.PxFile;
using Px.Utils.PxFile.Metadata;
using Px.Utils.Validation.ContentValidation;
using Px.Utils.Validation.SyntaxValidation;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    internal sealed class MetadataContentValidationBenchmark : Benchmark
    {
        internal override string Help =>
        "Validates the contents of the Px file metadata given amount of times." + Environment.NewLine +
        "\t-f, -file: The path to the px file to read." + Environment.NewLine +
        "\t-i, -iter: The number of iterations to run.";

        internal override string Description => "Benchmarks the metadata content validation of Px.Utils/Validation/SyntaxValidator.";
        private readonly ContentValidator validator;
        private ValidationStructuredEntry[] entries;

        internal MetadataContentValidationBenchmark()
        {             
            BenchmarkFunctions = [ValidateContentBenchmark];
            entries = [];
            validator = new(TestFilePath, Encoding.Default, entries);
        }

        protected override void OneTimeBenchmarkSetup()
        {
            base.OneTimeBenchmarkSetup();

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);
            SyntaxValidator syntaxValidator = new(stream, Encoding.Default, TestFilePath);
            SyntaxValidationResult validatorResult = syntaxValidator.Validate();
            entries = [.. validatorResult.Result];
        }

        private void ValidateContentBenchmark()
        {
            validator.Validate();
        }
    }
}
