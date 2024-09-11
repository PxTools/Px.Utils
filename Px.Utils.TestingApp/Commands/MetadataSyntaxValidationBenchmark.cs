using Px.Utils.PxFile.Metadata;
using Px.Utils.Validation.SyntaxValidation;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    internal sealed class MetadataSyntaxValidationBenchmark : FileBenchmark
    {
        internal override string Help =>
        "Validates the syntax of the Px file metadata given amount of times." + Environment.NewLine +
        "\t-f, -file: The path to the px file to read." + Environment.NewLine +
        "\t-i, -iter: The number of iterations to run.";

        internal override string Description => "Benchmarks the metadata syntax validation of Px.Utils/Validation/SyntaxValidator.";

        private Encoding encoding = Encoding.Default;

        internal MetadataSyntaxValidationBenchmark()
        {             
            BenchmarkFunctions = [SyntaxValidationBenchmark];
            BenchmarkFunctionsAsync = [SyntaxValidationBenchmarkAsync];
        }

        protected override void OneTimeBenchmarkSetup()
        {
            base.OneTimeBenchmarkSetup();

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            PxFileMetadataReader reader = new();
            encoding = reader.GetEncoding(stream);
        }

        private void SyntaxValidationBenchmark()
        {
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            SyntaxValidator validator = new(stream, encoding, TestFilePath, leaveStreamOpen: true);
            validator.Validate();
        }

        private async Task SyntaxValidationBenchmarkAsync()
        {
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            SyntaxValidator validator = new(stream, encoding, TestFilePath, leaveStreamOpen: true);
            await validator.ValidateAsync();
        }
    }
}
