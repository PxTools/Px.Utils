using PxUtils.PxFile.Metadata;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    internal sealed class MetadataReaderBenchmark : Benchmark
    {
        internal override string Help => "Tests the performance of the metadata reading.";

        internal override string Description => "Tests the performance of the metadata reading.";

        internal MetadataReaderBenchmark()
        {
            BenchmarkFunctions = [BuildMetadata];
            BenchmarkFunctionsAsync = [BuildMetadataAsync];
        }

        private void BuildMetadata()
        {
            using FileStream fileStream = new(TestFilePath, FileMode.Open, FileAccess.Read);
            Encoding encoding = PxFileMetadataReader.GetEncoding(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            _ = PxFileMetadataReader.ReadMetadata(fileStream, encoding).ToList();
        }

        private async Task BuildMetadataAsync()
        {
            using FileStream fileStream = new(TestFilePath, FileMode.Open, FileAccess.Read);
            Encoding encoding = PxFileMetadataReader.GetEncoding(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            _ = await PxFileMetadataReader.ReadMetadataAsync(fileStream, encoding).ToListAsync();
        }
    }
}
