﻿
using PxUtils.ModelBuilders;
using PxUtils.PxFile.Metadata;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    internal class MetadataBuilderBenchmark : Benchmark
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

        protected override void BenchmarkSetup()
        {
            base.BenchmarkSetup();
            FileStream fileStream = new(TestFilePath, FileMode.Open, FileAccess.Read);
            Encoding encoding = PxFileMetadataReader.GetEncoding(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            _metaEntries = PxFileMetadataReader.ReadMetadata(fileStream, encoding).ToList();
        }

        protected void BuildMetadata()
        {
            var builder = new MatrixMetadataBuilder();
            IEnumerable<KeyValuePair<string, string>> copyMeta = [.. _metaEntries];
            builder.Build(copyMeta);
        }
    }
}