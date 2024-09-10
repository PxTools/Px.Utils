using Px.Utils.PxFile.Metadata;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    internal sealed class MetadataReaderBenchmark : FileBenchmark
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
            PxFileMetadataReader reader = new();
            Encoding encoding = reader.GetEncoding(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            _ = reader.ReadMetadata(fileStream, encoding).ToList();
        }

        private async Task BuildMetadataAsync()
        {
            using FileStream fileStream = new(TestFilePath, FileMode.Open, FileAccess.Read);
            PxFileMetadataReader reader = new();
            Encoding encoding = await reader.GetEncodingAsync(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            _ = await reader.ReadMetadataAsync(fileStream, encoding).ToListAsync();
        }
    }
}
