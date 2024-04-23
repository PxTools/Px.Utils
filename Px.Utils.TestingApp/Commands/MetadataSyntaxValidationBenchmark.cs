using PxUtils.PxFile.Metadata;
using PxUtils.Validation.SyntaxValidation;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    internal class MetadataSyntaxValidationBenchmark : Benchmark
    {
        internal override string Help =>
        "Validates the syntax of the Px file metadata given amount of times." + Environment.NewLine +
        "\t-f, -file: The path to the px file to read." + Environment.NewLine +
        "\t-i, -iter: The number of iterations to run.";

        internal override string Description => "Benchmarks the metadata syntax validation of Px.Utils/Validation/SyntaxValidation.";

        private Encoding encoding;

        internal MetadataSyntaxValidationBenchmark()
        {             
            BenchmarkFunctions = [SyntaxValidationBenchmark];
            BenchmarkFunctionsAsync = [SyntaxValidationAsyncBenchmark];
        }

        protected override void OneTimeBenchmarkSetup()
        {
            base.OneTimeBenchmarkSetup();

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);
        }

        private void SyntaxValidationBenchmark()
        {
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);

            SyntaxValidation.ValidatePxFileMetadataSyntax(stream, encoding, TestFilePath);
        }

        private async Task SyntaxValidationAsyncBenchmark()
        {
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);

            await SyntaxValidation.ValidatePxFileMetadataSyntaxAsync(stream, encoding, TestFilePath);
        }
    }
}
