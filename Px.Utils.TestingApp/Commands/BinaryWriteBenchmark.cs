using Px.Utils.BinaryData.ValueConverters;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Models.Metadata.ExtensionMethods;
using Px.Utils.ModelBuilders;
using Px.Utils.PxFile.Data;
using Px.Utils.PxFile.Metadata;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    /// <summary>
    /// Benchmarks binary data writing by reading PX files and generating binary blob files (.pxb) with parallel processing for comprehensive benchmarking.
    /// </summary>
    internal sealed class BinaryWriteBenchmark : FileBenchmark
    {
        internal override string Help =>
            "Benchmarks building binary files from Px file data." + Environment.NewLine +
            "\t-f, -file: Path to the PX file to process (required)." + Environment.NewLine +
            "\t-d, -dims: Comma-separated list of dimension codes to split data by (default: use content dimension)." + Environment.NewLine +
            "\t-o, -output: Output directory for .pxb files (default: temp directory, auto-cleanup)." + Environment.NewLine +
            "\t-i, -iter: Number of iterations to run (default: 1).";

        internal override string Description => "Processes PX files to generate binary blob files for write benchmarking.";

        private string[] _splitDimensions = [];
        private string _outputDirectory = Path.GetTempPath();
        private bool _userSpecifiedOutput;

        private static readonly string[] dimsFlags = ["-d", "-dims"];
        private static readonly string[] outputFlags = ["-o", "-output"];

        private IReadOnlyMatrixMetadata? _metadata;
        private readonly List<BlobGenerationInfo> _generatedBlobs = [];

        internal BinaryWriteBenchmark()
        {
            BenchmarkFunctions = [];
            BenchmarkFunctionsAsync = [GenerateAllBinaryBlobsAsync];
            ParameterFlags.Add(dimsFlags);
            ParameterFlags.Add(outputFlags);
        }

        protected override async Task OneTimeBenchmarkSetupAsync()
        {
            await base.OneTimeBenchmarkSetupAsync();

            using FileStream fileStream = new(TestFilePath, FileMode.Open, FileAccess.Read);
            PxFileMetadataReader metadataReader = new();
            Encoding encoding = metadataReader.GetEncoding(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            List<KeyValuePair<string, string>> entries = [.. metadataReader.ReadMetadata(fileStream, encoding)];
            MatrixMetadataBuilder builder = new();
            _metadata = builder.Build(entries);

            if (_splitDimensions.Length == 0)
            {
                ContentDimension? contentDim = _metadata.GetContentDimension();

                if (contentDim != null)
                {
                    _splitDimensions = [contentDim.Code];
                }
                else
                {
                    throw new InvalidOperationException("No content dimension found and no split dimensions specified.");
                }
            }

            foreach (string dimCode in _splitDimensions)
            {
                if (!_metadata.Dimensions.Any(d => d.Code == dimCode))
                {
                    throw new ArgumentException($"Dimension '{dimCode}' not found in metadata.");
                }
            }

            Directory.CreateDirectory(_outputDirectory);
        }

        protected override void SetRunParameters()
        {
            base.SetRunParameters();

            foreach (string key in Parameters.Keys)
            {
                if (dimsFlags.Contains(key) && Parameters[key].Count > 0)
                {
                    _splitDimensions = [.. Parameters[key][0]
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())];
                }
                else if (outputFlags.Contains(key) && Parameters[key].Count > 0)
                {
                    _outputDirectory = Parameters[key][0];
                    _userSpecifiedOutput = true;
                }
            }
        }

        protected override void StartInteractiveMode()
        {
            base.StartInteractiveMode();

            Console.WriteLine("Enter dimension codes to split data by (comma-separated, or leave empty for content dimension):");
            string dimsInput = Console.ReadLine() ?? "";
            if (!string.IsNullOrEmpty(dimsInput))
            {
                _splitDimensions = [.. dimsInput
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())];
            }

            Console.WriteLine($"Enter output directory for .pxb files (default: {Path.GetTempPath()}):");
            Console.WriteLine("Note: Files will be automatically deleted if using default temp directory.");
            string outputInput = Console.ReadLine() ?? "";
            if (!string.IsNullOrEmpty(outputInput))
            {
                _outputDirectory = outputInput;
                _userSpecifiedOutput = true;
            }
        }

        private async Task GenerateAllBinaryBlobsAsync()
        {
            if (_metadata == null)
            {
                throw new InvalidOperationException("Metadata not initialized.");
            }

            try
            {
                List<DimensionSplit> splits = GenerateDimensionSplits(_metadata, _splitDimensions);

                ParallelOptions parallelOptions = new()
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };

                await Parallel.ForEachAsync(splits, parallelOptions, async (split, ct) =>
                {
                    BlobGenerationInfo? blobInfo = await GenerateBinaryBlobForSplitAsync(split, ct);
                    if (blobInfo != null)
                    {
                        lock (_generatedBlobs)
                        {
                            _generatedBlobs.Add(blobInfo);
                        }
                    }
                });
            }
            finally
            {
                if (!_userSpecifiedOutput)
                {
                    CleanupTemporaryFiles();
                }
            }
        }

        private async Task<BlobGenerationInfo?> GenerateBinaryBlobForSplitAsync(DimensionSplit split, CancellationToken ct)
        {
            if (_metadata == null) return null;

            try
            {
                IMatrixMap collapsedMap = _metadata;
                foreach ((string dimCode, string valueCode) in split.DimensionValues)
                {
                    collapsedMap = collapsedMap.CollapseDimension(dimCode, valueCode);
                }

                int dataSize = (int)collapsedMap.GetSize();
                if (dataSize == 0) return null;
                DoubleDataValue[] dataBuffer = new DoubleDataValue[dataSize];

                using FileStream fileStream = new(TestFilePath, FileMode.Open, FileAccess.Read);
                using PxFileStreamDataReader dataReader = new(fileStream);
                await dataReader.ReadDoubleDataValuesAsync(dataBuffer, 0, collapsedMap, _metadata, ct);

                BinaryValueCodecSelector selector = new();
                selector.Process(dataBuffer);
                BinaryValueCodecType codecType = selector.GetCodecType();
                IBinaryValueCodec codec = selector.CreateCodec();

                string codecName = codecType.ToString().Replace("Codec", "");

                string fileName = GenerateBlobFileName(split, codecName);
                string fullPath = Path.Combine(_outputDirectory, fileName);
                long fileSize;

                using (FileStream outputStream = new(fullPath, FileMode.Create, FileAccess.Write))
                {
                    codec.Write(dataBuffer, outputStream);
                    await outputStream.FlushAsync(ct);
                    fileSize = outputStream.Length;
                }

                return new BlobGenerationInfo(
                    fileName,
                    fullPath,
                    dataSize,
                    fileSize,
                    codecType,
                    collapsedMap,
                    split
                 );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating blob for split {string.Join(",", split.DimensionValues.Select(dv => $"{dv.DimCode}={dv.ValueCode}"))}: {ex.Message}");
                return null;
            }
        }

        private void CleanupTemporaryFiles()
        {
            foreach (BlobGenerationInfo blob in _generatedBlobs)
            {
                try
                {
                    if (File.Exists(blob.FullPath))
                    {
                        File.Delete(blob.FullPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not delete {blob.FileName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Public method to generate blob files for use by other benchmark classes.
        /// </summary>
        internal static async Task<List<BlobGenerationInfo>> GenerateBlobFilesAsync(
            string pxFilePath,
            string outputDirectory,
            string[] splitDimensions,
            CancellationToken ct = default)
        {
            List<BlobGenerationInfo> generatedBlobs = [];

            using FileStream fileStream = new(pxFilePath, FileMode.Open, FileAccess.Read);
            PxFileMetadataReader metadataReader = new();
            Encoding encoding = metadataReader.GetEncoding(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            List<KeyValuePair<string, string>> entries = [.. metadataReader.ReadMetadata(fileStream, encoding)];
            MatrixMetadataBuilder builder = new();
            IReadOnlyMatrixMetadata metadata = builder.Build(entries);

            // Use content dimension if no dimensions specified
            string[] actualSplitDimensions = splitDimensions;
            if (actualSplitDimensions.Length == 0)
            {
                ContentDimension? contentDim = metadata.GetContentDimension();
                if (contentDim != null)
                {
                    actualSplitDimensions = [contentDim.Code];
                }
                else
                {
                    throw new InvalidOperationException("No content dimension found and no split dimensions specified.");
                }
            }

            List<DimensionSplit> splits = GenerateDimensionSplits(metadata, actualSplitDimensions);
            Directory.CreateDirectory(outputDirectory);

            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = ct
            };

            await Parallel.ForEachAsync(splits, parallelOptions, async (split, token) =>
            {
                BlobGenerationInfo? blobInfo = await GenerateSingleBlobAsync(pxFilePath, outputDirectory, metadata, split, token);
                if (blobInfo != null)
                {
                    lock (generatedBlobs)
                    {
                        generatedBlobs.Add(blobInfo);
                    }
                }
            });

            return generatedBlobs;
        }

        private static async Task<BlobGenerationInfo?> GenerateSingleBlobAsync(
            string pxFilePath,
            string outputDirectory,
            IReadOnlyMatrixMetadata metadata,
            DimensionSplit split,
            CancellationToken ct)
        {
            try
            {
                IMatrixMap collapsedMap = metadata;
                foreach ((string dimCode, string valueCode) in split.DimensionValues)
                {
                    collapsedMap = collapsedMap.CollapseDimension(dimCode, valueCode);
                }

                int dataSize = (int)collapsedMap.GetSize();
                if (dataSize == 0) return null;

                DoubleDataValue[] dataBuffer = new DoubleDataValue[dataSize];

                using FileStream fileStream = new(pxFilePath, FileMode.Open, FileAccess.Read);
                using PxFileStreamDataReader dataReader = new(fileStream);
                dataReader.ReadDoubleDataValues(dataBuffer, 0, collapsedMap, metadata);

                BinaryValueCodecSelector selector = new();
                selector.Process(dataBuffer);
                BinaryValueCodecType codecType = selector.GetCodecType();
                IBinaryValueCodec codec = selector.CreateCodec();

                string codecName = codecType.ToString().Replace("Codec", "");

                string fileName = $"{Path.GetFileNameWithoutExtension(pxFilePath)}_{string.Join("_", split.DimensionValues.Select(dv => $"{dv.DimCode}-{dv.ValueCode}"))}_{codecName}.pxb";
                string fullPath = Path.Combine(outputDirectory, fileName);

                using FileStream outputStream = new(fullPath, FileMode.Create, FileAccess.Write);
                codec.Write(dataBuffer, outputStream);
                await outputStream.FlushAsync(ct);
                long fileSize = outputStream.Length;

                return new BlobGenerationInfo(fileName, fullPath, dataSize, fileSize, codecType, collapsedMap, split);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating blob: {ex.Message}");
                return null;
            }
        }

        private static List<DimensionSplit> GenerateDimensionSplits(IReadOnlyMatrixMetadata metadata, string[] splitDimCodes)
        {
            List<DimensionSplit> splits = [];

            // Get all value combinations for the split dimensions and generate splits based on cartesian product
            List<IReadOnlyDimension> splitDims = [.. metadata.Dimensions.Where(d => splitDimCodes.Contains(d.Code))];
            IEnumerable<IEnumerable<(string DimCode, string ValueCode)>> combinations =
                CartesianProduct(splitDims.Select(dim =>
                    dim.Values.Select(val => (dim.Code, val.Code))));

            foreach (IEnumerable<(string DimCode, string ValueCode)> combination in combinations)
            {
                splits.Add(new DimensionSplit([.. combination]));
            }

            return splits;
        }

        private static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = [[]];
            return sequences.Aggregate(
            emptyProduct,
            (accumulator, sequence) =>
                from accseq in accumulator
                from item in sequence
                select accseq.Concat([item]));
        }

        private string GenerateBlobFileName(DimensionSplit split, string codecName)
        {
            string tablePrefix = Path.GetFileNameWithoutExtension(TestFilePath);
            string splitSuffix = string.Join("_", split.DimensionValues.Select(dv => $"{dv.DimCode}-{dv.ValueCode}"));

            return $"{tablePrefix}_{splitSuffix}_{codecName}.pxb";
        }

        internal sealed record DimensionSplit(List<(string DimCode, string ValueCode)> DimensionValues);

        internal sealed record BlobGenerationInfo(
            string FileName,
            string FullPath,
            int ValueCount,
            long FileSizeBytes,
            BinaryValueCodecType CodecType,
            IMatrixMap MatrixMap,
            DimensionSplit Split)
        {
        }
    }
}