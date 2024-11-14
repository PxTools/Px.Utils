using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Models.Metadata.MetaProperties;

namespace Px.Utils.UnitTests.SerializerTests.Fixtures.Objects
{
    internal static class MatrixMetadataFixture
    {
        public static MatrixMetadata SimpleMeta => new(
            "foo",
            ["foo", "bar"],
            Dimensions,
            AdditionalProperties
        );

        private static readonly List<Dimension> Dimensions =
        [
            new Dimension(
                "dimension_code_1",
                new([
                    new KeyValuePair<string, string>("foo", "dimension_name_1_foo"),
                    new KeyValuePair<string, string>("bar", "dimension_name_1_bar")
                ]),
                [],
                new ValueList(
                [
                    new(
                        code: "value_code_1_1",
                        name: new([
                            new KeyValuePair<string, string>("foo", "value_name_1_1_foo"),
                            new KeyValuePair<string, string>("bar", "value_name_1_1_bar")
                            ]))
                ]),
                DimensionType.Nominal
            ),
            new ContentDimension(
                "dimension_code_2",
                new([
                    new KeyValuePair<string, string>("foo", "dimension_name_2_foo"),
                    new KeyValuePair<string, string>("bar", "dimension_name_2_bar")
                ]),
                [],
                new ContentValueList(
                [
                    new ContentDimensionValue(
                        code: "value_code_2_1",
                        name: new([
                            new KeyValuePair<string, string>("foo", "value_name_2_1_foo"),
                            new KeyValuePair<string, string>("bar", "value_name_2_1_bar")
                        ]),
                        unit: new([
                            new KeyValuePair<string, string>("foo", "unit_name_2_1_foo"),
                            new KeyValuePair<string, string>("bar", "unit_name_2_1_bar")
                        ]),
                        lastUpdated: DateTime.MinValue,
                        precision: 1
                    )
                ])
            ),
            new TimeDimension(
                "dimension_code_3",
                new([
                    new KeyValuePair<string, string>("foo", "dimension_name_3_foo"),
                    new KeyValuePair<string, string>("bar", "dimension_name_3_bar")
                ]),
                [],
                new ValueList(
                [
                    new(
                        code: "value_code_3_1",
                        name: new([
                            new KeyValuePair<string, string>("foo", "value_name_3_1_foo"),
                            new KeyValuePair<string, string>("bar", "value_name_3_1_bar")
                            ]))
                ]),
                TimeDimensionInterval.Year
            ),
        ];

        // Create test data for additional properties
        private static readonly Dictionary<string, MetaProperty> AdditionalProperties = new()
        {
            { "property_1", new StringProperty("property_value_1") },
            { "property_2", new MultilanguageStringProperty(
                new ([
                    new KeyValuePair<string, string>("foo", "property_value_foo"),
                    new KeyValuePair<string, string>("bar", "property_value_bar")
                ])) }
        };
    }
}
