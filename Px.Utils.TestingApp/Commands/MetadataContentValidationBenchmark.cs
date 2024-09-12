using Px.Utils.Validation.ContentValidation;
using Px.Utils.Validation.SyntaxValidation;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    internal sealed class MetadataContentValidationBenchmark : FileBenchmark
    {
        internal override string Help =>
        "Validates the contents of the Px file metadata given amount of times." + Environment.NewLine +
        "\t-f, -file: The path to the px file to read." + Environment.NewLine +
        "\t-i, -iter: The number of iterations to run.";

        internal override string Description => "Benchmarks the metadata content validation of Px.Utils/Validation/SyntaxValidator.";

        private List<ValidationStructuredEntry> _entries = [];

        internal MetadataContentValidationBenchmark()
        {             
            BenchmarkFunctions = [ValidateContentBenchmark];
        }

        protected override void OneTimeBenchmarkSetup()
        {
            base.OneTimeBenchmarkSetup();

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);
            SyntaxValidator syntaxValidator = new();
            SyntaxValidationResult validatorResult = syntaxValidator.Validate(stream, Encoding.Default, TestFilePath);
            _entries = validatorResult.Result;
        }

        private void ValidateContentBenchmark()
        {
            ContentValidator validator = new(TestFilePath, Encoding.Default, [.. _entries]);
            validator.Validate();
        }
    }
}
