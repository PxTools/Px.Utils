using Px.Utils.IntegrationTest.Assertions;
using Px.Utils.IntegrationTest.ExpectedResults;
using Px.Utils.Models;
using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Operations;
using System.Diagnostics;
using System.Text.Json;

namespace Px.Utils.IntegrationTest.Scenarios;

internal sealed class CalculationScenario(string databaseRoot, string expectationsRoot)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TestResult Run()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<string> failures = [];
        CalculationExpectationDocument document = Load();
        foreach (CalculationExpectation calculation in document.Calculations)
        {
            string filePath = Path.Combine(databaseRoot, "valid-files", calculation.FileName);
            LoadedMatrix loaded = PxLoader.Load(filePath);
            DoubleDataValue[] sourceData = [.. loaded.Matrix.Data];
            MatrixMap targetMap = PxLoader.CreateMap(calculation.TargetMap);
            DimensionMap dimensionMap = new(calculation.DimensionCode, [.. calculation.SourceValueCodes]);
            Matrix<DoubleDataValue> result = Execute(loaded.Matrix, calculation, targetMap, dimensionMap);
            failures.AddRange(IntegrationAssert.CompareDimensions($"Calculation / {calculation.Name}", calculation.ResultDimensions, result.Metadata));
            MatrixMap resultMap = PxLoader.CreateMap(calculation.ResultDimensions);
            failures.AddRange(IntegrationAssert.CompareMatrix($"Calculation / {calculation.Name}", resultMap, calculation.Values, result.Data, calculation.Operation == "DivideSubsetByConstant"));
            if (!sourceData.SequenceEqual(loaded.Matrix.Data))
            {
                failures.Add($"Calculation / {calculation.Name}: source matrix was modified.");
            }
        }
        return new TestResult("Calculations", failures.Count == 0, stopwatch.Elapsed, failures);
    }

    private static Matrix<DoubleDataValue> Execute(Matrix<DoubleDataValue> matrix, CalculationExpectation calculation, MatrixMap targetMap, DimensionMap dimensionMap)
    {
        return calculation.Operation switch
        {
            "SumToNewValue" => matrix.SumToNewValue(CreateVirtualValue(matrix, calculation), dimensionMap),
            "MultiplyToNewValue" => matrix.MultiplyToNewValue(CreateVirtualValue(matrix, calculation), dimensionMap),
            "MultiplySubsetByConstant" => 
                matrix.MultiplySubsetByConstant(targetMap, new DoubleDataValue(calculation.Constant ?? throw new InvalidOperationException("Missing multiplication constant."), DataValueType.Exists)),
            "DivideSubsetByConstant" => 
                matrix.DivideSubsetByConstant(targetMap, new DoubleDataValue(calculation.Constant ?? throw new InvalidOperationException("Missing division constant."), DataValueType.Exists)),
            "ApplyRelative" => 
                matrix.ApplyRelative((value, baseValue) => value - baseValue, dimensionMap, calculation.BaseValueCode ?? throw new InvalidOperationException("Missing subtraction base value.")),
            _ => throw new InvalidOperationException($"Unsupported calculation operation {calculation.Operation}.")
        };
    }

    private static DimensionValue CreateVirtualValue(Matrix<DoubleDataValue> matrix, CalculationExpectation calculation)
    {
        string code = calculation.NewValueCode ?? throw new InvalidOperationException($"Calculation {calculation.Name} is missing a result value code.");
        return new DimensionValue(code, new(matrix.Metadata.DefaultLanguage, code), true);
    }

    private CalculationExpectationDocument Load()
    {
        string path = Path.Combine(expectationsRoot, "calculation-results.json");
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<CalculationExpectationDocument>(json, JsonOptions) ?? throw new InvalidOperationException("Could not read calculation expectation fixture.");
    }
}
