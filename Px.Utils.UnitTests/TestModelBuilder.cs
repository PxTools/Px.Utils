using PxUtils.Language;
using PxUtils.Models.Metadata;
using PxUtils.Models.Metadata.Dimensions;
using PxUtils.Models.Metadata.Enums;

namespace Px.Utils.UnitTests
{
    internal static class TestModelBuilder
    {
        internal static MatrixMetadata BuildTestMetadata(int[] dimensionSizes)
        {
            List<Dimension> dimensions = [BuildTestContentDimension(0, dimensionSizes[0])];
            if (dimensionSizes.Length > 1) dimensions.Add(BuildTestTimeDimension(1, dimensionSizes[1]));
            if (dimensionSizes.Length > 2) dimensions.AddRange(dimensionSizes.Skip(2).Select((size, i) => BuildTestDimension(i + 2, size)));
            return new MatrixMetadata("en", ["en"], dimensions, []);
        }

        internal static ContentDimension BuildTestContentDimension(int ordinal, int numOfValues)
        {
            string code = $"var{ordinal}";
            MultilanguageString name = new("en", $"name_en_var{ordinal}");
            Dictionary<string, MetaProperty> additionalProperties = [];
            List<ContentDimensionValue> values = [];
            for (int i = 0; i < numOfValues; i++)
            {
                values.Add(BuildTestContentDimensionValue(code, i));
            }
            return new ContentDimension(code, name, additionalProperties, values);
        }

        internal static TimeDimension BuildTestTimeDimension(int ordinal, int numOfValues)
        {
            string code = $"var{ordinal}";
            MultilanguageString name = new("en", $"name_en_var{ordinal}");
            Dictionary<string, MetaProperty> additionalProperties = [];
            List<DimensionValue> values = [];
            for (int i = 0; i < numOfValues; i++)
            {
                values.Add(BuildTestDimensionValue(code, i));
            }
            return new TimeDimension(code, name, additionalProperties, values, TimeDimensionInterval.Year);
        }

        internal static Dimension BuildTestDimension(int ordinal, int numOfValues)
        {
            string code = $"var{ordinal}";
            MultilanguageString name = new("en", $"name_en_var{ordinal}");
            Dictionary<string, MetaProperty> additionalProperties = [];
            List<DimensionValue> values = [];
            for (int i = 0; i < numOfValues; i++)
            {
                values.Add(BuildTestDimensionValue(code, i));
            }
            return new Dimension(code, name, additionalProperties, values, DimensionType.Other);
        }

        internal static ContentDimensionValue BuildTestContentDimensionValue(string parentCode, int ordinal)
        {
            DimensionValue baseValue = BuildTestDimensionValue(parentCode, ordinal);
            MultilanguageString unit = new("en", $"unit_en_{parentCode}-val{ordinal}");
            DateTime lastUpdated = new(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            int precision = 1;
            return new ContentDimensionValue(baseValue, unit, lastUpdated, precision);
        }

        internal static DimensionValue BuildTestDimensionValue(string parentCode, int ordinal)
        {
            string code = $"{parentCode}_val{ordinal}";
            MultilanguageString name = new("en", $"name_en_{parentCode}_val{ordinal}");
            return new DimensionValue(code, name);
        }
    }
}
