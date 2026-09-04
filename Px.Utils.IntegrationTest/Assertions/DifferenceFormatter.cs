using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Metadata;
using Px.Utils.IntegrationTest.ExpectedResults;
using System.Globalization;

namespace Px.Utils.IntegrationTest.Assertions;

internal static class DifferenceFormatter
{
    public static string FormatCoordinate(IMatrixMap map, int index)
    {
        List<string> coordinates = [];
        int remaining = index;
        for (int dimensionIndex = map.DimensionMaps.Count - 1; dimensionIndex >= 0; dimensionIndex--)
        {
            IDimensionMap dimension = map.DimensionMaps[dimensionIndex];
            int valueIndex = remaining % dimension.ValueCodes.Count;
            remaining /= dimension.ValueCodes.Count;
            coordinates.Insert(0, $"{dimension.Code}={dimension.ValueCodes[valueIndex]}");
        }

        return $"[{string.Join(", ", coordinates)}]";
    }

    public static string FormatValue(DataValueExpectation value)
    {
        return value.Value.HasValue
            ? $"{value.Type}({value.Value.Value.ToString("R", CultureInfo.InvariantCulture)})"
            : value.Type;
    }

    public static string FormatValue(DoubleDataValue value)
    {
        return value.Type == Px.Utils.Models.Data.DataValueType.Exists
            ? $"{value.Type}({value.UnsafeValue.ToString("R", CultureInfo.InvariantCulture)})"
            : value.Type.ToString();
    }
}
