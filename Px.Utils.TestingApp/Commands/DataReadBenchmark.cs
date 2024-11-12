using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.ExtensionMethods;
using Px.Utils.PxFile.Data;
using Px.Utils.ModelBuilders;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.PxFile.Metadata;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    internal sealed class DataReadBenchmark : FileBenchmark
    {
        private IReadOnlyMatrixMetadata? MetaData { get; set; }

        internal override string Help =>
        "Reads the defined dataset from the target px file the given amount of times." + Environment.NewLine +
        "\t-f, -file: The path to the px file to read." + Environment.NewLine +
        "\t-c, -cells: The approximate number of cells to read." + Environment.NewLine +
        "\t-i, -iter: The number of iterations to run." + Environment.NewLine +
        "The rows and columns are defined as a space-separated list of integers or ranges of integers separated by '..'.";

        internal override string Description => "Benchmarks the data reading capabilities of the PxFileStreamDataReader.";

        private IMatrixMap? Target { get; set; }

        private int _numberOfCells = 1000000;

        private static readonly string[] cellFlags = ["-c", "-cells"];

        private const string metadataNotFoundMessage = "Metadata not found.";

        internal DataReadBenchmark()
        {
            BenchmarkFunctions = [RunReadDoubleDataValuesBenchmarks, RunReadDecimalDataValuesBenchmarks, RunReadUnsafeDoubleBenchmarks];
            BenchmarkFunctionsAsync = [RunReadDoubleDataValuesBenchmarksAsync, RunReadDecimalDataValuesBenchmarksAsync, RunReadUnsafeDoubleBenchmarksAsync];
            ParameterFlags.Add(cellFlags);
        }

        protected override void OneTimeBenchmarkSetup()
        {
            base.OneTimeBenchmarkSetup();

            using FileStream fileStream = new(TestFilePath, FileMode.Open, FileAccess.Read);
            PxFileMetadataReader reader = new();
            Encoding encoding = reader.GetEncoding(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            List<KeyValuePair<string, string>> entries = reader.ReadMetadata(fileStream, encoding).ToList();
            MatrixMetadataBuilder builder = new();
            MetaData = builder.Build(entries);
        }

        private void RunReadDoubleDataValuesBenchmarks()
        {
            if(MetaData is null) throw new InvalidOperationException(metadataNotFoundMessage);
            Target = GenerateBenchmarkTargetMap(MetaData, _numberOfCells);

            DoubleDataValue[] buffer = new DoubleDataValue[Target.GetSize()];

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            using PxFileStreamDataReader reader = new(stream);

            reader.ReadDoubleDataValues(buffer, 0, Target, MetaData);
        }

        private void RunReadDecimalDataValuesBenchmarks()
        {
            if (MetaData is null) throw new InvalidOperationException(metadataNotFoundMessage);
            Target = GenerateBenchmarkTargetMap(MetaData, _numberOfCells);

            DecimalDataValue[] buffer = new DecimalDataValue[Target.GetSize()];

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            using PxFileStreamDataReader reader = new(stream);

            reader.ReadDecimalDataValues(buffer, 0, Target, MetaData);
        }

        private void RunReadUnsafeDoubleBenchmarks()
        {
            if (MetaData is null) throw new InvalidOperationException(metadataNotFoundMessage);
            Target = GenerateBenchmarkTargetMap(MetaData, _numberOfCells);

            double[] buffer = new double[Target.GetSize()];
            double[] missingValueEncodings = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0];

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            using PxFileStreamDataReader reader = new(stream);

            reader.ReadUnsafeDoubles(buffer, 0, Target, MetaData, missingValueEncodings);
        }

        private async Task RunReadDoubleDataValuesBenchmarksAsync()
        {
            if (MetaData is null) throw new InvalidOperationException(metadataNotFoundMessage);
            Target = GenerateBenchmarkTargetMap(MetaData, _numberOfCells);

            DoubleDataValue[] buffer = new DoubleDataValue[Target.GetSize()];

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            using PxFileStreamDataReader reader = new(stream);

            await reader.ReadDoubleDataValuesAsync(buffer, 0, Target, MetaData);
        }

        private async Task RunReadDecimalDataValuesBenchmarksAsync()
        {
            if (MetaData is null) throw new InvalidOperationException(metadataNotFoundMessage);
            Target = GenerateBenchmarkTargetMap(MetaData, _numberOfCells);

            DecimalDataValue[] buffer = new DecimalDataValue[Target.GetSize()];

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            using PxFileStreamDataReader reader = new(stream);

            await reader.ReadDecimalDataValuesAsync(buffer, 0, Target, MetaData);
        }

        private async Task RunReadUnsafeDoubleBenchmarksAsync()
        {
            if (MetaData is null) throw new InvalidOperationException(metadataNotFoundMessage);
            Target = GenerateBenchmarkTargetMap(MetaData, _numberOfCells);

            double[] buffer = new double[Target.GetSize()];
            double[] missingValueEncodings = [0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0];

            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            using PxFileStreamDataReader reader = new(stream);

            await reader.ReadUnsafeDoublesAsync(buffer, 0, Target, MetaData, missingValueEncodings);
        }

        protected override void SetRunParameters()
        {
            base.SetRunParameters();

            foreach (string key in Parameters.Keys)
            {
                if (cellFlags.Contains(key) && !int.TryParse(Parameters[key][0], out _numberOfCells))
                {
                    throw new ArgumentException($"Invalid argument {key} {string.Join(' ', Parameters[key])}");
                }
            }
        }

        private static IMatrixMap GenerateBenchmarkTargetMap(IMatrixMap complete, int targetSize)
        {
            int size = complete.GetSize();
            if (size < targetSize) return complete;

            List<IDimensionMap> sortedDimensions = [.. complete.DimensionMaps];
            while (size > targetSize)
            {
                sortedDimensions = [.. sortedDimensions.OrderByDescending(x => x.ValueCodes.Count)];
                var valCodes = sortedDimensions[0].ValueCodes;
                sortedDimensions[0] = new DimensionMap(sortedDimensions[0].Code, valCodes.Skip(1).ToList());
                size = sortedDimensions.Aggregate(1, (acc, x) => acc * x.ValueCodes.Count);
            }

            List<IDimensionMap> dimList = complete.DimensionMaps
                .Select(dim => dim.Code)
                .Select(dimCode => new DimensionMap(
                    dimCode,
                    [.. sortedDimensions.First(dim => dim.Code == dimCode).ValueCodes]))
                .Cast<IDimensionMap>()
                .ToList();

            return new MatrixMap(dimList);
        }
    }
}
