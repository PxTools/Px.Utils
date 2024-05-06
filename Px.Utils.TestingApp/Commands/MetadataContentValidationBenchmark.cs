using PxUtils.PxFile.Metadata;
using PxUtils.Validation.ContentValidation;
using PxUtils.Validation.SyntaxValidation;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    internal class MetadataContentValidationBenchmark : Benchmark
    {
        internal override string Help =>
        "Validates the contents of the Px file metadata given amount of times." + Environment.NewLine +
        "\t-f, -file: The path to the px file to read." + Environment.NewLine +
        "\t-i, -iter: The number of iterations to run.";

        internal override string Description => "Benchmarks the metadata content validation of Px.Utils/Validation/SyntaxValidation.";
        ContentValidator validator;
        ValidationStructuredEntry[] entries;

        internal MetadataContentValidationBenchmark()
        {             
            BenchmarkFunctions = [ValidateContentBenchmark];
            BenchmarkFunctionsAsync = [ValidateContentBenchmarkAsync];
            entries = [];
            validator = new("", Encoding.Default);
        }

        protected override void OneTimeBenchmarkSetup()
        {
            base.OneTimeBenchmarkSetup();

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
            entries = [.. SyntaxValidation.ValidatePxFileMetadataSyntax(stream, encoding, TestFilePath).Result];

            validator = new(TestFilePath, encoding);
        }

        private void ValidateContentBenchmark()
        {
            validator.Validate(entries);
        }

        private async Task ValidateContentBenchmarkAsync()
        {
            await validator.ValidateAsync(entries);
        }
    }
}
