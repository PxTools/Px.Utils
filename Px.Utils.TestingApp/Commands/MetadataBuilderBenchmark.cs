
using Px.Utils.ModelBuilders;
using Px.Utils.PxFile.Metadata;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    internal sealed class MetadataBuilderBenchmark : Benchmark
    {
        internal override string Help => "Tests the performance of the metadata building.";

        internal override string Description => "Tests the performance of the metadata building.";

        private List<KeyValuePair<string, string>> _metaEntries;

        internal MetadataBuilderBenchmark()
        {
            BenchmarkFunctions = [BuildMetadata];
            BenchmarkFunctionsAsync = [];
            _metaEntries = [];
        }

        protected override void OneTimeBenchmarkSetup()
        {
            base.OneTimeBenchmarkSetup();
            FileStream fileStream = new(TestFilePath, FileMode.Open, FileAccess.Read);
            PxFileMetadataReader reader = new();
            Encoding encoding = reader.GetEncoding(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            _metaEntries = reader.ReadMetadata(fileStream, encoding).ToList();
        }

        private void BuildMetadata()
        {
            var builder = new MatrixMetadataBuilder();
            IEnumerable<KeyValuePair<string, string>> copyMeta = [.. _metaEntries];
            builder.Build(copyMeta);
        }
    }
}
