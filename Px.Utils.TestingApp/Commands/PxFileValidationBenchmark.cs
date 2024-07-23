using Px.Utils.PxFile.Metadata;
using Px.Utils.Validation;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    internal class PxFileValidationBenchmark : FileBenchmark
    {
        internal override string Help =>
        "Runs through the whole px file validation process (metadata syntax- and contents-, data-) for the given file.";

        internal override string Description => "Benchmarks the px file validation capabilities of the PxFileValidator.";

        private Encoding encoding;

        internal PxFileValidationBenchmark()
        {
            BenchmarkFunctions = [ValidatePxFileBenchmarks];
            BenchmarkFunctionsAsync = [ValidatePxFileBenchmarksAsync];
            encoding = Encoding.UTF8;
        }

        protected override void OneTimeBenchmarkSetup()
        {
            base.OneTimeBenchmarkSetup();

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            PxFileMetadataReader reader = new();
            try
            {
                encoding = reader.GetEncoding(stream);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while reading the encoding of the file: {e.Message}");
                encoding = Encoding.Default;
            }
        }

        private void ValidatePxFileBenchmarks()
        {
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            PxFileValidator validator = new(stream, TestFilePath, encoding);
            validator.Validate();
        }

        private async Task ValidatePxFileBenchmarksAsync()
        {
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            PxFileValidator validator = new(stream, TestFilePath, encoding);
            await validator.ValidateAsync();
        }
    }
}
