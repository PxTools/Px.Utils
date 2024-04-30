using PxUtils.PxFile;
using PxUtils.PxFile.Data;
using PxUtils.Validation.DataValidation;
using PxUtils.PxFile.Metadata;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    internal class DataValidationBenchmark : Benchmark
    {
        private readonly string[] expectedRowsFlags = ["-r", "-rows"];
        private readonly string[] expectedColsFlags = ["-c", "-cols"];

        private int expectedRows;
        private int expectedCols;

        internal override string Help =>
        "Reads the defined dataset from the target px file the given amount of times." + Environment.NewLine +
        "\t-f, -file: The path to the px file to read." + Environment.NewLine +
        "\t-i, -iter: The number of iterations to run." + Environment.NewLine +
        "\t-r, -rows: How many data rows the validator should expect." + Environment.NewLine +
        "\t-c, -cols: How many data colums the validator should expect.";

        internal override string Description => "Benchmarks the data validation capabilities of the DataValidator.";

        private long start = 0;
        private const string dataKeyword = "DATA";

        private Encoding encoding;

        internal DataValidationBenchmark()
        {
            BenchmarkFunctions = [ValidateDataBenchmarks];
            // BenchmarkFunctionsAsync = [ValidateDataAsyncBenchmarks, ValidateDataTokenAsyncBenchmarks];
            ParameterFlags.Add(expectedRowsFlags);
            ParameterFlags.Add(expectedColsFlags);
            encoding = Encoding.UTF8;
        }

        protected override void OneTimeBenchmarkSetup()
        {
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            encoding = PxFileMetadataReader.GetEncoding(stream);
            start = StreamUtilities.FindKeywordPosition(stream, dataKeyword, PxFileSyntaxConf.Default);
            if (start == -1)
            {
                throw new ArgumentException($"Could not find data keyword '{dataKeyword}'");
            }
        }

        private void ValidateDataBenchmarks()
        {
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            stream.Position = start + dataKeyword.Length + 1 + 2; // +1 to skip the '='
            DataValidation.Validate(stream, expectedCols, expectedRows, 0, encoding);
        }
        
       /* private async Task ValidateDataTokenAsyncBenchmarks()
        {
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            
            stream.Position = start + dataKeyword.Length + 1 +2 ; // +1 to skip the '='

            IAsyncEnumerable<Token> tokens = DataValidation.TokenizeAsync(stream, PxFileSyntaxConf.Default,null);
                
            int j = 0;
            await foreach (Token token in tokens)
            {
                j++;
            }
        }*/
        
        private async Task ValidateDataAsyncBenchmarks()
        {
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            stream.Position = start + dataKeyword.Length + 1 + 2; // +1 to skip the '='
                
            await DataValidation.ValidateAsync(stream, expectedCols, expectedRows, 0, null);
        }

        protected override void SetRunParameters()
        {
            base.SetRunParameters();

            Dictionary<string, List<string>> parameters = GroupParameters(Inputs ?? [], ParameterFlags.SelectMany(x => x).ToList());
            foreach (string key in parameters.Keys)
            {
                if ((expectedColsFlags.Contains(key) && !int.TryParse(parameters[key][0], out expectedCols)) &&
                    (expectedRowsFlags.Contains(key) && !int.TryParse(parameters[key][0], out expectedRows)))
                {
                        throw new ArgumentException($"Invalid argument {key} {string.Join(' ', parameters[key])}");
                }
            }
        }

        protected override void StartInteractiveMode()
        {
            base.StartInteractiveMode();

            Console.WriteLine("How many data rows should the validator expect?");
            string rowsInput = Console.ReadLine() ?? "";
            int rowsValue;
            while (!int.TryParse(rowsInput, out rowsValue))
            {
                Console.WriteLine("Invalid number of rows, please enter a valid integer");
                rowsInput = Console.ReadLine() ?? "";
            }
            expectedRows = rowsValue;

            Console.WriteLine("How many data columns should the validator expect?");
            string colsInput = Console.ReadLine() ?? "";
            int colsValue;
            while (!int.TryParse(colsInput, out colsValue))
            {
                Console.WriteLine("Invalid number of columns, please enter a valid integer");
                colsInput = Console.ReadLine() ?? "";
            }   
            expectedCols = colsValue;
        }
    }
}
