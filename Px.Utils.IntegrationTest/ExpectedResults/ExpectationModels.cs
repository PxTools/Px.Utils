namespace Px.Utils.IntegrationTest.ExpectedResults;

internal sealed class ValidationExpectationFile
{
    public required string FileName { get; init; }

    public required string Classification { get; init; }

    public required int WarningCount { get; init; }

    public required int ErrorCount { get; init; }

    public required List<ValidationExpectationEntry> Entries { get; init; }
}

internal sealed class ValidationExpectationEntry
{
    public required string Level { get; init; }

    public required string Rule { get; init; }

    public required int Count { get; init; }

    public int? Row { get; init; }

    public int? Character { get; init; }

    public string? AdditionalInformation { get; init; }
}

internal sealed class ValidationExpectationDocument
{
    public required List<ValidationExpectationFile> Files { get; init; }

    public required List<ValidationExpectationFile> DatabaseEntries { get; init; }
}

internal sealed class QueryExpectationDocument
{
    public required List<QueryExpectation> Queries { get; init; }
}

internal sealed class QueryExpectation
{
    public required string Name { get; init; }

    public required string FileName { get; init; }

    public required List<DimensionExpectation> Dimensions { get; init; }

    public required List<DataValueExpectation> Values { get; init; }
}

internal sealed class DimensionExpectation
{
    public required string Code { get; init; }

    public required List<string> ValueCodes { get; init; }
}

internal sealed class DataValueExpectation
{
    public required string Type { get; init; }

    public double? Value { get; init; }
}

internal sealed class CalculationExpectationDocument
{
    public required List<CalculationExpectation> Calculations { get; init; }
}

internal sealed class CalculationExpectation
{
    public required string Name { get; init; }

    public required string Operation { get; init; }

    public required string FileName { get; init; }

    public required List<DimensionExpectation> SourceMap { get; init; }

    public required List<DimensionExpectation> TargetMap { get; init; }

    public required string DimensionCode { get; init; }

    public required List<string> SourceValueCodes { get; init; }

    public string? NewValueCode { get; init; }

    public double? Constant { get; init; }

    public string? BaseValueCode { get; init; }

    public required List<DimensionExpectation> ResultDimensions { get; init; }

    public required List<DataValueExpectation> Values { get; init; }
}
