﻿using Px.Utils.PxFile;
using Px.Utils.PxFile.Data;
using Px.Utils.Validation.DataValidation;
using Px.Utils.PxFile.Metadata;
using System.Text;

namespace Px.Utils.TestingApp.Commands
{
    internal sealed class DataValidationBenchmark : FileBenchmark
    {
        private readonly string[] expectedRowsFlags = ["-r", "-rows"];
        private readonly string[] expectedColsFlags = ["-c", "-cols"];

        private int expectedRows;
        private int expectedCols;

        internal override string Help =>
        "Validates the px file data." + Environment.NewLine +
        "\t-r, -rows: How many data rows the validator should expect." + Environment.NewLine +
        "\t-c, -cols: How many data colums the validator should expect.";

        internal override string Description => "Benchmarks the data validation capabilities of the DataValidator.";

        private long start;
        private const string dataKeyword = "DATA";

        private Encoding encoding;

        private const int readStartOffset = 3;

        internal DataValidationBenchmark()
        {
            BenchmarkFunctions = [ValidateDataBenchmarks];
            BenchmarkFunctionsAsync = [ValidateDataBenchmarksAsync];
            ParameterFlags.Add(expectedRowsFlags);
            ParameterFlags.Add(expectedColsFlags);
            encoding = Encoding.UTF8;
        }

        protected override void OneTimeBenchmarkSetup()
        {
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            PxFileMetadataReader reader = new();
            encoding = reader.GetEncoding(stream);
            start = StreamUtilities.FindKeywordPosition(stream, dataKeyword, PxFileConfiguration.Default);
            if (start == -1)
            {
                throw new ArgumentException($"Could not find data keyword '{dataKeyword}'");
            }
        }

        private void ValidateDataBenchmarks()
        {
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            stream.Position = start + dataKeyword.Length + readStartOffset; // skip the '=' and linechange
            DataValidator validator = new(expectedCols, expectedRows, 0);
            validator.Validate(stream, TestFilePath, encoding);
        }
        
        private async Task ValidateDataBenchmarksAsync()
        {
            using Stream stream = new FileStream(TestFilePath, FileMode.Open, FileAccess.Read);
            stream.Position = start + dataKeyword.Length + readStartOffset; // skip the '=' and linechange
            DataValidator validator = new(expectedCols, expectedRows, 0);

            await validator.ValidateAsync(stream, TestFilePath, encoding);
        }

        protected override void SetRunParameters()
        {
            base.SetRunParameters();

            foreach (string key in Parameters.Keys)
            {
                if (expectedColsFlags.Contains(key) && !int.TryParse(Parameters[key][0], out expectedCols))
                {
                    throw new ArgumentException($"Invalid argument {key} {string.Join(' ', Parameters[key])}");
                }
                if (expectedRowsFlags.Contains(key) && !int.TryParse(Parameters[key][0], out expectedRows))
                {
                    throw new ArgumentException($"Invalid argument {key} {string.Join(' ', Parameters[key])}");
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
