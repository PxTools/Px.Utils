using PxUtils.PxFile.Metadata;
using PxUtils.Validation.SyntaxValidation;
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

        internal MetadataSyntaxValidationBenchmark()
        {             
            BenchmarkFunctions = [SyntaxValidationBenchmark];
            BenchmarkFunctionsAsync = [SyntaxValidationAsyncBenchmark];
        }

        private void SyntaxValidationBenchmark()
        {
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);

            SyntaxValidation.ValidatePxFileMetadataSyntax(stream, encoding, TestFilePath);
        }

        private async Task SyntaxValidationAsyncBenchmark()
        {
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);

            await SyntaxValidation.ValidatePxFileMetadataSyntaxAsync(stream, encoding, TestFilePath);
        }
    }
}
