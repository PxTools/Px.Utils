using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Models.Metadata.Enums;

namespace Px.Utils.TestingApp.TestDataGenerator
{
    internal static class MetaGenerator
    {
        internal struct DimensionDefinition
        {
            internal DimensionType Type { get; set; }
            internal int Size { get; set; }

            internal DimensionDefinition(DimensionType type, int size)
            {
                Type = type;
                Size = size;
            }
        }

        const string LANG = "foo";

        internal static MatrixMetadata Generate(List<DimensionDefinition> dimensionDefinitions)
        {
            List<Dimension> dimensions = [];
            int ordinal = 0;

            foreach (DimensionDefinition definition in dimensionDefinitions)
            {
                if (definition.Type == DimensionType.Time)
                {
                    dimensions.Add(GenerateTimeDimension(definition.Size, ordinal));
                }
                else if (definition.Type == DimensionType.Content)
                {
                    dimensions.Add(GenerateContentDimension(definition.Size, ordinal));
                }
                else
                {
                    dimensions.Add(GenerateDimension(definition.Size, dimensions.Count, definition.Type));
                }

                ordinal++;
            }

            return new(LANG, [LANG], dimensions, []);
        }

        private static ContentDimension GenerateContentDimension(int size, int ordinal)
        {
            List<ContentDimensionValue> values = [];

            for (int i = 0; i < size; i++)
            {
                values.Add(new($"dim-{ordinal}-val-{i}", new(LANG, $"val-{i}"),
                    new(LANG, $"unit-{i}"), DateTime.Now, 3));
            }

            return new($"dim-{ordinal}", new(LANG, $"dim-{ordinal}"), [], new ContentValueList(values));
        }

        private static TimeDimension GenerateTimeDimension(int size, int ordinal)
        {
            List<DimensionValue> values = [];

            for (int i = 0; i < size; i++)
            {
                values.Add(new DimensionValue($"dim-{ordinal}-val-{i}", new(LANG, $"val-{i}")));
            }

            return new($"dim-{ordinal}", new(LANG, $"dim-{ordinal}"), [], new ValueList(values), TimeDimensionInterval.Year);
        }

        private static Dimension GenerateDimension(int size, int ordinal, DimensionType type)
        {
            List<DimensionValue> values = [];

            for (int i = 0; i < size; i++)
            {
                values.Add(new DimensionValue($"dim-{ordinal}-val-{i}", new(LANG, $"val-{i}")));
            }

            return new($"dim-{ordinal}", new(LANG, $"dim-{ordinal}"), [], new ValueList(values), type);
        }
    }
}
