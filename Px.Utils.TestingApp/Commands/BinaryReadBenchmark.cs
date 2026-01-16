using Px.Utils.BinaryData;
using Px.Utils.BinaryData.ValueConverters;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Metadata;
using System.IO.Pipes;

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
            "\t-w, -window: Maximum window size in bytes (default: 1MB)." + Environment.NewLine +
            "\t-m, -merge: Maximum merge gap in bytes (default: 64KB)." + Environment.NewLine +
            "\t-o, -output: Directory for .pxb files (default: temp directory, auto-cleanup)." + Environment.NewLine +
            "\t-iter: Number of iterations to run (default: 10).";

        internal override string Description => "Benchmarks BinaryDataReader performance with self-generated blob files.";

        private string[] _splitDimensions = [];
        private long _windowSizeBytes = 1 * 1024 * 1024; // 1MB
        private long _mergeCapBytes = 64 * 1024; // 64KB
        private string _outputDirectory = Path.GetTempPath();
        private bool _userSpecifiedOutput;

        private static readonly string[] dimsFlags = ["-d", "-dims"];
        private static readonly string[] windowFlags = ["-w", "-window"];
        private static readonly string[] mergeFlags = ["-m", "-merge"];
        private static readonly string[] outputFlags = ["-o", "-output"];

        private readonly List<BinaryWriteBenchmark.BlobGenerationInfo> _blobFiles = [];
        private string _tempBlobDirectory = "";

        internal BinaryReadBenchmark()
        {
            BenchmarkFunctions = [];
            BenchmarkFunctionsAsync = [
                RunChunkBasedReadBenchmarksAsync,
                RunSeekableStreamReadBenchmarksAsync,
                RunNonSeekableStreamReadBenchmarksAsync
            ];

            ParameterFlags.Add(dimsFlags);
            ParameterFlags.Add(windowFlags);
            ParameterFlags.Add(mergeFlags);
            ParameterFlags.Add(outputFlags);
        }

        protected override async Task OneTimeBenchmarkSetupAsync()
        {
            await base.OneTimeBenchmarkSetupAsync();

            if (!_userSpecifiedOutput)
            {
                _tempBlobDirectory = Path.Combine(Path.GetTempPath(), $"pxbench_{Guid.NewGuid():N}");
                _outputDirectory = _tempBlobDirectory;
            }

            Directory.CreateDirectory(_outputDirectory);

            if (_userSpecifiedOutput)
            {
                Console.WriteLine("Files will be preserved in user-specified directory.");
            }
            else
            {
                Console.WriteLine("Temporary files will be deleted after benchmarking.");
            }

            try
            {
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
                    Console.WriteLine($"  {blob.FileName}: {blob.CodecName}, {blob.FileSizeBytes:N0} bytes, {blob.ValueCount:N0} values");
                }

                if (_blobFiles.Count == 0)
                {
                    throw new InvalidOperationException("No blob files were generated for benchmarking.");
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
                else if (windowFlags.Contains(key) && Parameters[key].Count > 0)
                {
                    if (!long.TryParse(Parameters[key][0], out _windowSizeBytes))
                    {
                        throw new ArgumentException($"Invalid window size: {Parameters[key][0]}");
                    }
                }
                else if (mergeFlags.Contains(key) && Parameters[key].Count > 0)
                {
                    if (!long.TryParse(Parameters[key][0], out _mergeCapBytes))
                    {
                        throw new ArgumentException($"Invalid merge gap: {Parameters[key][0]}");
                    }
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
        }

        private async Task RunChunkBasedReadBenchmarksAsync()
        {
            if (_blobFiles.Count == 0)
            {
                throw new InvalidOperationException("No blob files available for benchmarking. Setup may have failed.");
            }

            await Task.WhenAll(_blobFiles.Select(async blobFile =>
            {
                await ReadBlobWithCodecAsync(blobFile, async (readMap, blobMap, bufferMap, bufferMemory) =>
                {
                    async Task<Stream> ChunkProviderAsync(long offset, long length, CancellationToken ct)
                    {
                        FileStream stream = new(blobFile.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
                        stream.Seek(offset, SeekOrigin.Begin);
                        return stream;
                    }

                    BinaryDataReader<DoubleCodec> reader = new(_windowSizeBytes, _mergeCapBytes);
                    await reader.ReadByChunkAsync(ChunkProviderAsync, readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None);
                });
            }));

            CleanupTemporaryFiles();
        }

        private async Task RunSeekableStreamReadBenchmarksAsync()
        {
            if (_blobFiles.Count == 0)
            {
                throw new InvalidOperationException("No blob files available for benchmarking. Setup may have failed.");
            }

            await Task.WhenAll(_blobFiles.Select(async blobFile =>
            {
                await ReadBlobWithCodecAsync(blobFile, async (readMap, blobMap, bufferMap, bufferMemory) =>
                {
                    using FileStream stream = new(blobFile.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
                    BinaryDataReader<DoubleCodec> reader = new(_windowSizeBytes, _mergeCapBytes);
                    await reader.ReadFromStreamAsync(stream, readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None);
                });
            }));

            CleanupTemporaryFiles();
        }

        private async Task RunNonSeekableStreamReadBenchmarksAsync()
        {
            if (_blobFiles.Count == 0)
            {
                throw new InvalidOperationException("No blob files available for benchmarking. Setup may have failed.");
            }

            await Task.WhenAll(_blobFiles.Select(async blobFile =>
            {
                await ReadBlobWithCodecAsync(blobFile, async (readMap, blobMap, bufferMap, bufferMemory) =>
                {
                    byte[] fileData = await File.ReadAllBytesAsync(blobFile.FullPath);

                    using AnonymousPipeServerStream pipeServer = new(PipeDirection.Out);
                    using AnonymousPipeClientStream pipeClient = new(PipeDirection.In, pipeServer.GetClientHandleAsString());

                    Task writeTask = Task.Run(async () =>
                    {
                        await pipeServer.WriteAsync(fileData, CancellationToken.None);
                        pipeServer.Close();
                    });

                    BinaryDataReader<DoubleCodec> reader = new(_windowSizeBytes, _mergeCapBytes);
                    await reader.ReadFromStreamAsync(pipeClient, readMap, blobMap, bufferMap, bufferMemory, CancellationToken.None);

                    await writeTask;
                });
            }));

            CleanupTemporaryFiles();
        }

        private static async Task ReadBlobWithCodecAsync(
            BinaryWriteBenchmark.BlobGenerationInfo blobFile,
            Func<IMatrixMap, IMatrixMap, IMatrixMap, Memory<DoubleDataValue>, Task> benchmarkAction)
        {
            IMatrixMap blobMap = blobFile.MatrixMap;
            IMatrixMap readMap = blobMap;
            IMatrixMap bufferMap = blobMap;

            DoubleDataValue[] buffer = new DoubleDataValue[blobFile.ValueCount];
            Memory<DoubleDataValue> bufferMemory = new(buffer);

            await benchmarkAction(readMap, blobMap, bufferMap, bufferMemory);
        }

        private void CleanupTemporaryFiles()
        {
            if (!_userSpecifiedOutput && !string.IsNullOrEmpty(_tempBlobDirectory))
            {
                try
                {
                    if (Directory.Exists(_tempBlobDirectory))
                    {
                        Directory.Delete(_tempBlobDirectory, true);
                    }
                }
                catch
                {
                    Console.WriteLine("Warning: Failed to delete temporary blob files.");
                }
            }
        }
    }
}