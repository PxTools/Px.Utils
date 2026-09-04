using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Metadata;
using Px.Utils.IntegrationTest.ExpectedResults;
using Px.Utils.Models.Metadata.Dimensions;

namespace Px.Utils.IntegrationTest.Assertions;

internal static class IntegrationAssert
{
    private const double DivisionTolerance = 0.000000001;
    private const int MaximumDifferences = 10;

    public static List<string> CompareMatrix(string context, MatrixMap map, IReadOnlyList<DataValueExpectation> expected, IReadOnlyList<DoubleDataValue> actual, bool useDivisionTolerance = false)
    {
        List<string> differences = [];
        if (expected.Count != actual.Count)
        {
            differences.Add($"{context}: expected {expected.Count} values, actual {actual.Count} values.");
        }

        int comparableCount = Math.Min(expected.Count, actual.Count);
        for (int index = 0; index < comparableCount; index++)
        {
            DataValueExpectation expectedValue = expected[index];
            DoubleDataValue actualValue = actual[index];
            bool typeMatches = string.Equals(expectedValue.Type, actualValue.Type.ToString(), StringComparison.Ordinal);
            bool valueMatches = actualValue.Type != DataValueType.Exists || (expectedValue.Value.HasValue && ValuesMatch(expectedValue.Value.Value, actualValue.UnsafeValue, useDivisionTolerance));
            if (!typeMatches || !valueMatches)
            {
                if (differences.Count < MaximumDifferences)
                {
                    differences.Add($"" +
                        $"{context}: index {index} " +
                        $"{DifferenceFormatter.FormatCoordinate(map, index)} expected " +
                        $"{DifferenceFormatter.FormatValue(expectedValue)}, " +
                        $"actual {DifferenceFormatter.FormatValue(actualValue)}.");
                }
                else
                {
                    differences.Add("Additional differences omitted.");
                    break;
                }
            }
        }

        return differences;
    }

    public static List<string> CompareDimensions(string context, IReadOnlyList<DimensionExpectation> expected, IReadOnlyMatrixMetadata actual)
    {
        List<string> differences = [];
        if (expected.Count != actual.Dimensions.Count)
        {
            differences.Add($"{context}: expected {expected.Count} dimensions, actual {actual.Dimensions.Count} dimensions.");
            return differences;
        }

        for (int index = 0; index < expected.Count; index++)
        {
            DimensionExpectation expectedDimension = expected[index];
            IReadOnlyDimension actualDimension = actual.Dimensions[index];
            string[] actualValues = [.. actualDimension.Values.Select(value => value.Code)];
            if (!string.Equals(expectedDimension.Code, actualDimension.Code, StringComparison.Ordinal) || !expectedDimension.ValueCodes.SequenceEqual(actualValues, StringComparer.Ordinal))
            {
                differences.Add($"{context}: dimension {index} " +
                    $"expected {expectedDimension.Code}=[{string.Join(',', expectedDimension.ValueCodes)}], " +
                    $"actual {actualDimension.Code}=[{string.Join(',', actualValues)}].");
            }
        }

        return differences;
    }

    private static bool ValuesMatch(double expected, double actual, bool useDivisionTolerance)
    {
        return useDivisionTolerance ? Math.Abs(expected - actual) <= DivisionTolerance : expected.Equals(actual);
    }
}
