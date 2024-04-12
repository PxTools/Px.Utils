using PxUtils.PxFile.Metadata;
using PxUtils.Validation.SyntaxValidation;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    internal class MetadataSyntaxValidationBenchmark : Benchmark
    {
        private string _testFilePath = "";

        private readonly string[] fileFlags = ["-f", "-file"];
        private readonly string[] iterFlags = ["-i", "-iter"];

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
            using Stream stream = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
            Encoding encoding = PxFileMetadataReader.GetEncoding(stream);
            stream.Seek(0, SeekOrigin.Begin);

            SyntaxValidation.ValidatePxFileMetadataSyntax(stream, encoding, _testFilePath);
        }

        private async Task SyntaxValidationAsyncBenchmark()
        {
            using Stream stream = new FileStream(_testFilePath, FileMode.Open, FileAccess.Read);
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);

            await SyntaxValidation.ValidatePxFileMetadataSyntaxAsync(stream, encoding, _testFilePath);
        }

        protected override void SetRunParameters(bool batchMode, List<string>? inputs)
        {
            Dictionary<string, List<string>> parameters = GroupParameters(inputs ?? [], [.. fileFlags, .. iterFlags]);
            if(parameters.Keys.Count == 2)
            {
                foreach (string key in parameters.Keys)
                {
                    if (fileFlags.Contains(key) && parameters[key].Count == 1)
                    {
                        _testFilePath = parameters[key][0];
                    }
                    else if (iterFlags.Contains(key) && parameters[key].Count == 1 &&
                        int.TryParse(parameters[key][0], out int iterations))
                    {
                        Iterations = iterations;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid argument {key} {string.Join(' ', parameters[key])}");
                    }
                }
            }
            else if( batchMode)
            {
                throw new ArgumentException("Invalid number of parameters.");
            }
            else
            {
                StartInteractiveMode();
            }
            
        }

        private void StartInteractiveMode()
        {
            Console.WriteLine("Enter the path to the PX file to benchmark");
            string file = Console.ReadLine() ?? "";

            while (!Path.Exists(file) || !Path.GetFileName(file).EndsWith(".px"))
            {
                Console.WriteLine("File provided is not valid, please enter a path to a valid px file");
                file = Console.ReadLine() ?? "";
            }

            _testFilePath = file;

            Console.WriteLine("Enter the number of iterations to run");
            string iterations = Console.ReadLine() ?? "";
            int value;
            while (!int.TryParse(iterations, out value))
            {
                Console.WriteLine("Invalid number of iterations, please enter a valid integer");
                iterations = Console.ReadLine() ?? "";
            }

            Iterations = value;
        }
    }
}
