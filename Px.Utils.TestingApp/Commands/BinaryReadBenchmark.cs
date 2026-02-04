using Px.Utils.BinaryData;
using Px.Utils.BinaryData.ValueConverters;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.ExtensionMethods;
using Px.Utils.ModelBuilders;
using Px.Utils.PxFile.Metadata;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    /// <summary>
    /// Benchmarks binary data reading performance by testing all three BinaryDataReader methods:
    /// ReadByChunkAsync, ReadFromSeekableStreamAsync, and ReadFromNonSeekableStreamAsync.
    /// Generates its own test blob files during setup and cleans them up after benchmarking.
    /// </summary>
    internal sealed class BinaryReadBenchmark : FileBenchmark
    {
        internal override string Help =>
            "Benchmarks binary data reading performance by generating and testing .pxb blob files." + Environment.NewLine +
            "\t-f, -file: Path to the PX file to process (required)." + Environment.NewLine +
            "\t-d, -dims: Comma-separated list of dimension codes to split data by (default: use content dimension)." + Environment.NewLine +
            "\t-w, -window: Maximum window size in bytes (default:1MB)." + Environment.NewLine +
            "\t-m, -merge: Maximum merge gap in bytes (default:64KB)." + Environment.NewLine +
            "\t-o, -output: Directory for .pxb files (default: temp directory)." + Environment.NewLine +
            "\t-c, -cells: Approximate number of cells to read (default:1000000)." + Environment.NewLine +
            "\t-iter: Number of iterations to run (default:10).";

        internal override string Description => "Benchmarks BinaryDataReader performance with self-generated blob files.";

        private string[] _splitDimensions = [];
        private long _windowSizeBytes = 1 * 1024 * 1024; // 1MB
        private long _mergeCapBytes = 64 * 1024; // 64KB
        private string _outputDirectory = Path.GetTempPath();
        private bool _userSpecifiedOutput;
        private long _numberOfCells = 1000000L;

        private static readonly string[] dimsFlags = ["-d", "-dims"];
        private static readonly string[] windowFlags = ["-w", "-window"];
        private static readonly string[] mergeFlags = ["-m", "-merge"];
        private static readonly string[] outputFlags = ["-o", "-output"];
        private static readonly string[] cellFlags = ["-c", "-cells"];

        private readonly List<BinaryWriteBenchmark.BlobGenerationInfo> _blobFiles = [];
        private IReadOnlyMatrixMetadata? _metadata;

        internal BinaryReadBenchmark()
        {
            BenchmarkFunctions = [];
            BenchmarkFunctionsAsync = [
                RunChunkBasedReadAllBenchmarksAsync,
                RunChunkBasedReadLastBenchmarksAsync,
                RunSeekableStreamReadAllBenchmarksAsync,
                RunSeekableStreamReadLastBenchmarksAsync,
                RunNonSeekableStreamReadAllBenchmarksAsync,
                RunNonSeekableStreamReadLastBenchmarksAsync
            ];

            ParameterFlags.Add(dimsFlags);
            ParameterFlags.Add(windowFlags);
            ParameterFlags.Add(mergeFlags);
            ParameterFlags.Add(outputFlags);
            ParameterFlags.Add(cellFlags);
        }

        protected override async Task OneTimeBenchmarkSetupAsync()
        {
            await base.OneTimeBenchmarkSetupAsync();

            if (!_userSpecifiedOutput)
            {
                _outputDirectory = Path.Combine(Path.GetTempPath(), $"pxbench_{Guid.NewGuid():N}");
            }

            Directory.CreateDirectory(_outputDirectory);

            try
            {
                using FileStream fileStream = new(TestFilePath, FileMode.Open, FileAccess.Read);
                PxFileMetadataReader metadataReader = new();
                Encoding encoding = metadataReader.GetEncoding(fileStream);
                fileStream.Seek(0, SeekOrigin.Begin);

                List<KeyValuePair<string, string>> entries = [.. metadataReader.ReadMetadata(fileStream, encoding)];
                MatrixMetadataBuilder builder = new();
                _metadata = builder.Build(entries);

                Console.WriteLine("Generating test blob files...");

                List<BinaryWriteBenchmark.BlobGenerationInfo> generatedBlobs =
                    await BinaryWriteBenchmark.GenerateBlobFilesAsync(
                        TestFilePath,
                        _outputDirectory,
                        _splitDimensions,
                        CancellationToken.None);

                _blobFiles.AddRange(generatedBlobs);

                Console.WriteLine($"Generated {_blobFiles.Count} blob files for benchmarking:");
                foreach (BinaryWriteBenchmark.BlobGenerationInfo blob in _blobFiles)
                {
                    Console.WriteLine($"  {blob.FileName}: {blob.CodecType}, {blob.FileSizeBytes:N0} bytes, {blob.ValueCount:N0} values");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating blob files: {ex.Message}");
                throw;
            }
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
                else if (windowFlags.Contains(key) &&
                    Parameters[key].Count > 0 &&
                    !long.TryParse(Parameters[key][0], out _windowSizeBytes))
                {
                    throw new ArgumentException($"Invalid window size: {Parameters[key][0]}");
                }
                else if (mergeFlags.Contains(key) &&
                    Parameters[key].Count > 0 &&
                    !long.TryParse(Parameters[key][0], out _mergeCapBytes))
                {
                    throw new ArgumentException($"Invalid merge gap: {Parameters[key][0]}");
                }
                else if (outputFlags.Contains(key) && Parameters[key].Count > 0)
                {
                    _outputDirectory = Parameters[key][0];
                    _userSpecifiedOutput = true;
                }
                else if (cellFlags.Contains(key) &&
                    Parameters[key].Count > 0 &&
                    !long.TryParse(Parameters[key][0], out _numberOfCells))
                {
                    throw new ArgumentException($"Invalid number of cells: {Parameters[key][0]}");
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

            Console.WriteLine("Enter window size in bytes (default: 1048576):");
            string windowInput = Console.ReadLine() ?? "";
            if (!string.IsNullOrEmpty(windowInput) && long.TryParse(windowInput, out long window))
            {
                _windowSizeBytes = window;
            }

            Console.WriteLine("Enter merge gap in bytes (default: 65536):");
            string mergeInput = Console.ReadLine() ?? "";
            if (!string.IsNullOrEmpty(mergeInput) && long.TryParse(mergeInput, out long merge))
            {
                _mergeCapBytes = merge;
            }

            Console.WriteLine($"Enter output directory for .pxb files (default: {Path.GetTempPath()}):");
            Console.WriteLine("Note: Files will be automatically deleted if using default temp directory.");
            string outputInput = Console.ReadLine() ?? "";
            if (!string.IsNullOrEmpty(outputInput))
            {
                _outputDirectory = outputInput;
                _userSpecifiedOutput = true;
            }

            Console.WriteLine("Enter approximate number of cells to read (default: 1000000):");
            string cellsInput = Console.ReadLine() ?? "";
            if (!string.IsNullOrEmpty(cellsInput) && long.TryParse(cellsInput, out long cells))
            {
                _numberOfCells = cells;
            }
        }

        private async Task RunChunkBasedReadAllBenchmarksAsync()
        {
            await Task.WhenAll(_blobFiles.Select(async blobFile =>
            {
                await ReadBlobWithCodecAsync(blobFile, GenerateReadAllMap, (readMap, blobMap, bufferMap, bufferMemory) =>
                {
                    Func<long, long, CancellationToken, Task<Stream>> chunkProvider = (offset, length, ct) =>
                    {
                        FileStream stream = new(blobFile.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
                        stream.Seek(offset, SeekOrigin.Begin);
                        return Task.FromResult<Stream>(stream);
                    };

                    return CreateReaderAndReadByChunkAsync(blobFile.CodecType, chunkProvider, readMap, blobMap, bufferMap, bufferMemory);
                });
            }));
        }

        private async Task RunChunkBasedReadLastBenchmarksAsync()
        {   
            IMatrixMap? targetMap = DataReadBenchmark.GenerateBenchmarkTargetMap(_metadata!, _numberOfCells);
            Console.WriteLine($"Global target map generated: {targetMap.GetSize()} cells to read across all blobs.");

            await Task.WhenAll(_blobFiles.Select(async blobFile =>
            {
                IMatrixMap blobMap = blobFile.MatrixMap;
                IMatrixMap readMap = targetMap.IntersectionMap(blobFile.MatrixMap);

                // Skip blobs with empty read maps (no values to read from this blob)
                if (readMap.DimensionMaps.Count == 0 || readMap.GetSize() == 0)
                {
                    return;
                }
                
                IMatrixMap bufferMap = readMap;
                int bufferSize = (int)readMap.GetSize();
                DoubleDataValue[] buffer = new DoubleDataValue[bufferSize];
                Memory<DoubleDataValue> bufferMemory = new(buffer);

                Func<long, long, CancellationToken, Task<Stream>> chunkProvider = (offset, length, ct) =>
                {
                    FileStream stream = new(blobFile.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
                    stream.Seek(offset, SeekOrigin.Begin);
                    return Task.FromResult<Stream>(stream);
                };

                await CreateReaderAndReadByChunkAsync(blobFile.CodecType, chunkProvider, readMap, blobMap, bufferMap, bufferMemory);
            }));
            }

        private async Task RunSeekableStreamReadAllBenchmarksAsync()
        {
            await Task.WhenAll(_blobFiles.Select(async blobFile =>
            {
                await ReadBlobWithCodecAsync(blobFile, GenerateReadAllMap, async (readMap, blobMap, bufferMap, bufferMemory) =>
                {
                    using FileStream stream = new(blobFile.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
                    await CreateReaderAndReadFromStreamAsync(blobFile.CodecType, stream, readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None);
                });
            }));
        }

        private async Task RunSeekableStreamReadLastBenchmarksAsync()
        {
            IMatrixMap? targetMap = DataReadBenchmark.GenerateBenchmarkTargetMap(_metadata!, _numberOfCells);
            Console.WriteLine($"Global target map generated: {targetMap.GetSize()} cells to read across all blobs.");

            await Task.WhenAll(_blobFiles.Select(async blobFile =>
            {
                IMatrixMap blobMap = blobFile.MatrixMap;
                IMatrixMap readMap = targetMap.IntersectionMap(blobFile.MatrixMap);

                // Skip blobs with empty read maps (no values to read from this blob)
                if (readMap.DimensionMaps.Count == 0 || readMap.GetSize() == 0)
                {
                    return;
                }
                
                IMatrixMap bufferMap = readMap;
                int bufferSize = (int)readMap.GetSize();
                DoubleDataValue[] buffer = new DoubleDataValue[bufferSize];
                Memory<DoubleDataValue> bufferMemory = new(buffer);

                using FileStream stream = new(blobFile.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
                await CreateReaderAndReadFromStreamAsync(blobFile.CodecType, stream, readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None);
            }));
        }

        private async Task RunNonSeekableStreamReadAllBenchmarksAsync()
        {
            await Task.WhenAll(_blobFiles.Select(async blobFile =>
            {
                await ReadBlobWithCodecAsync(blobFile, GenerateReadAllMap, async (readMap, blobMap, bufferMap, bufferMemory) =>
                {
                    await ReadNonSeekableAsync(blobFile.FullPath, blobFile.CodecType, readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None);
                });
            }));
        }

        private async Task RunNonSeekableStreamReadLastBenchmarksAsync()
        {
            IMatrixMap? targetMap = DataReadBenchmark.GenerateBenchmarkTargetMap(_metadata!, _numberOfCells);
            Console.WriteLine($"Global target map generated: {targetMap.GetSize()} cells to read across all blobs.");

            await Task.WhenAll(_blobFiles.Select(async blobFile =>
            {
                IMatrixMap blobMap = blobFile.MatrixMap;
                IMatrixMap readMap = targetMap.IntersectionMap(blobFile.MatrixMap);
                
                // Skip blobs with empty read maps (no values to read from this blob)
                if (readMap.DimensionMaps.Count == 0 || readMap.GetSize() == 0)
                {
                    return;
                }
                
                IMatrixMap bufferMap = readMap;
                int bufferSize = (int)readMap.GetSize();
                DoubleDataValue[] buffer = new DoubleDataValue[bufferSize];
                Memory<DoubleDataValue> bufferMemory = new(buffer);

                await ReadNonSeekableAsync(blobFile.FullPath, blobFile.CodecType, readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None);
            }));
        }

        /// <summary>
        /// Generates a ReadAll matrix map - reads all values.
        /// </summary>
        private static IMatrixMap GenerateReadAllMap(BinaryWriteBenchmark.BlobGenerationInfo blobFile)
        {
            return blobFile.MatrixMap;
        }

        private static async Task ReadBlobWithCodecAsync(
            BinaryWriteBenchmark.BlobGenerationInfo blobFile,
            Func<BinaryWriteBenchmark.BlobGenerationInfo, IMatrixMap> mapGenerator,
            Func<IMatrixMap, IMatrixMap, IMatrixMap, Memory<DoubleDataValue>, Task> benchmarkAction)
        {
            IMatrixMap blobMap = blobFile.MatrixMap;
            IMatrixMap readMap = mapGenerator(blobFile);
            
            // Skip blobs with empty read maps (no values to read from this blob)
            if (readMap.DimensionMaps.Count == 0 || readMap.GetSize() == 0)
            {
                return;
            }
            
            IMatrixMap bufferMap = readMap;
            int bufferSize = (int)readMap.GetSize();
            DoubleDataValue[] buffer = new DoubleDataValue[bufferSize];
            Memory<DoubleDataValue> bufferMemory = new(buffer);
            await benchmarkAction(readMap, blobMap, bufferMap, bufferMemory);
        }

        private async Task ReadNonSeekableAsync(
            string blobPath,
            BinaryValueCodecType codecType,
            IMatrixMap readMap,
            IMatrixMap blobMap,
            IMatrixMap bufferMap,
            Memory<DoubleDataValue> bufferMemory,
            CancellationToken ct)
        {
            byte[] fileData = await File.ReadAllBytesAsync(blobPath, ct);

            using NonSeekableReadOnlyStream stream = new(fileData);
            await CreateReaderAndReadFromStreamAsync(codecType, stream, readMap, blobMap, bufferMap, bufferMemory, ct);
        }

        private Task CreateReaderAndReadByChunkAsync(
            BinaryValueCodecType codecType,
            Func<long, long, CancellationToken, Task<Stream>> provider,
            IMatrixMap readMap,
            IMatrixMap blobMap,
            IMatrixMap bufferMap,
            Memory<DoubleDataValue> bufferMemory)
        {
            return codecType switch
            {
                BinaryValueCodecType.UInt16Codec => new BinaryDataReader<UInt16Codec>(_windowSizeBytes, _mergeCapBytes)
                    .ReadByChunkAsync((offset, length, ct) => provider(offset, length, ct), readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None),
                BinaryValueCodecType.Int16Codec => new BinaryDataReader<Int16Codec>(_windowSizeBytes, _mergeCapBytes)
                    .ReadByChunkAsync((offset, length, ct) => provider(offset, length, ct), readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None),
                BinaryValueCodecType.UInt24Codec => new BinaryDataReader<UInt24Codec>(_windowSizeBytes, _mergeCapBytes)
                    .ReadByChunkAsync((offset, length, ct) => provider(offset, length, ct), readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None),
                BinaryValueCodecType.Int24Codec => new BinaryDataReader<Int24Codec>(_windowSizeBytes, _mergeCapBytes)
                    .ReadByChunkAsync((offset, length, ct) => provider(offset, length, ct), readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None),
                BinaryValueCodecType.UInt32Codec => new BinaryDataReader<UInt32Codec>(_windowSizeBytes, _mergeCapBytes)
                    .ReadByChunkAsync((offset, length, ct) => provider(offset, length, ct), readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None),
                BinaryValueCodecType.Int32Codec => new BinaryDataReader<Int32Codec>(_windowSizeBytes, _mergeCapBytes)
                    .ReadByChunkAsync((offset, length, ct) => provider(offset, length, ct), readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None),
                BinaryValueCodecType.FloatCodec => new BinaryDataReader<FloatCodec>(_windowSizeBytes, _mergeCapBytes)
                    .ReadByChunkAsync((offset, length, ct) => provider(offset, length, ct), readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None),
                BinaryValueCodecType.DoubleCodec => new BinaryDataReader<DoubleCodec>(_windowSizeBytes, _mergeCapBytes)
                    .ReadByChunkAsync((offset, length, ct) => provider(offset, length, ct), readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None),
                _ => new BinaryDataReader<DoubleCodec>(_windowSizeBytes, _mergeCapBytes)
                    .ReadByChunkAsync((offset, length, ct) => provider(offset, length, ct), readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None)
            };
        }

        private Task CreateReaderAndReadFromStreamAsync(
            BinaryValueCodecType codecType,
            Stream stream,
            IMatrixMap readMap,
            IMatrixMap blobMap,
            IMatrixMap bufferMap,
            Memory<DoubleDataValue> bufferMemory,
            CancellationToken ct)
        {
            return codecType switch
            {
                BinaryValueCodecType.UInt16Codec => new BinaryDataReader<UInt16Codec>(_windowSizeBytes, _mergeCapBytes).ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, bufferMemory, ct),
                BinaryValueCodecType.Int16Codec => new BinaryDataReader<Int16Codec>(_windowSizeBytes, _mergeCapBytes).ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, bufferMemory, ct),
                BinaryValueCodecType.UInt24Codec => new BinaryDataReader<UInt24Codec>(_windowSizeBytes, _mergeCapBytes).ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, bufferMemory, ct),
                BinaryValueCodecType.Int24Codec => new BinaryDataReader<Int24Codec>(_windowSizeBytes, _mergeCapBytes).ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, bufferMemory, ct),
                BinaryValueCodecType.UInt32Codec => new BinaryDataReader<UInt32Codec>(_windowSizeBytes, _mergeCapBytes).ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, bufferMemory, ct),
                BinaryValueCodecType.Int32Codec => new BinaryDataReader<Int32Codec>(_windowSizeBytes, _mergeCapBytes).ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, bufferMemory, ct),
                BinaryValueCodecType.FloatCodec => new BinaryDataReader<FloatCodec>(_windowSizeBytes, _mergeCapBytes).ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, bufferMemory, ct),
                BinaryValueCodecType.DoubleCodec => new BinaryDataReader<DoubleCodec>(_windowSizeBytes, _mergeCapBytes).ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, bufferMemory, ct),
                _ => new BinaryDataReader<DoubleCodec>(_windowSizeBytes, _mergeCapBytes).ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, bufferMemory, ct)
            };
        }

        private sealed class NonSeekableReadOnlyStream(ReadOnlyMemory<byte> data) : Stream
        {
            private readonly Stream _inner = new MemoryStream(data.ToArray(), writable: false);

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _inner.Read(buffer, offset, count);
            }

            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                return _inner.ReadAsync(buffer, cancellationToken);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                throw new NotSupportedException();
            }

            public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                throw new NotSupportedException();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _inner.Dispose();
                }

                base.Dispose(disposing);
            }
        }
    }
}