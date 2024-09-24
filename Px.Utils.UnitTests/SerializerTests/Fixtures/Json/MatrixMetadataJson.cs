namespace Px.Utils.UnitTests.SerializerTests.Fixtures.Json
{
    internal static class MatrixMetadataJson
    {
        public static string SimpleMeta =
@"{
  ""defaultLanguage"": ""foo"",
  ""availableLanguages"": [
    ""foo"",
    ""bar""
  ],
  ""dimensions"": [
    {
      ""type"": ""Nominal"",
      ""code"": ""dimension_code_1"",
      ""name"": {
        ""foo"": ""dimension_name_1_foo"",
        ""bar"": ""dimension_name_1_bar""
      },
      ""values"": [
        {
          ""code"": ""value_code_1_1"",
          ""name"": {
            ""foo"": ""value_name_1_1_foo"",
            ""bar"": ""value_name_1_1_bar""
          },
          ""additionalProperties"": {},
          ""isVirtual"": false
        }
      ],
      ""additionalProperties"": {}
    },
    {
      ""type"": ""Content"",
      ""code"": ""dimension_code_2"",
      ""name"": {
        ""foo"": ""dimension_name_2_foo"",
        ""bar"": ""dimension_name_2_bar""
      },
      ""values"": [
        {
          ""unit"": {
            ""foo"": ""unit_name_2_1_foo"",
            ""bar"": ""unit_name_2_1_bar""
          },
          ""lastUpdated"": ""0001-01-01T00:00:00"",
          ""precision"": 1,
          ""code"": ""value_code_2_1"",
          ""name"": {
            ""foo"": ""value_name_2_1_foo"",
            ""bar"": ""value_name_2_1_bar""
          },
          ""additionalProperties"": {},
          ""isVirtual"": false
        }
      ],
      ""additionalProperties"": {}
    },
    {
      ""type"": ""Time"",
      ""code"": ""dimension_code_3"",
      ""name"": {
        ""foo"": ""dimension_name_3_foo"",
        ""bar"": ""dimension_name_3_bar""
      },
      ""interval"": ""Year"",
      ""values"": [
        {
          ""code"": ""value_code_3_1"",
          ""name"": {
            ""foo"": ""value_name_3_1_foo"",
            ""bar"": ""value_name_3_1_bar""
          },
          ""additionalProperties"": {},
          ""isVirtual"": false
        }
      ],
      ""additionalProperties"": {}
    }
  ],
  ""additionalProperties"": {
    ""property_1"": {
      ""type"": ""Text"",
      ""value"": ""property_value_1""
    },
    ""property_2"": {
      ""type"": ""MultilanguageText"",
      ""value"": {
        ""foo"": ""property_value_foo"",
        ""bar"": ""property_value_bar""
      }
    }
  }
}";

        public static string SimpleMetaWithoutWhitespace =
            @"{""DefaultLanguage"":""foo"",""AvailableLanguages"":[""foo"",""bar""],""Dimensions"":[{""Type"":""Nominal"",""Code"":""dimension_code_1"",""Name"":{""foo"":""dimension_name_1_foo"",""bar"":""dimension_name_1_bar""},""Values"":[{""Code"":""value_code_1_1"",""Name"":{""foo"":""value_name_1_1_foo"",""bar"":""value_name_1_1_bar""},""AdditionalProperties"":{},""IsVirtual"":false}],""AdditionalProperties"":{}},{""Type"":""Content"",""Code"":""dimension_code_2"",""Name"":{""foo"":""dimension_name_2_foo"",""bar"":""dimension_name_2_bar""},""Values"":[{""Unit"":{""foo"":""unit_name_2_1_foo"",""bar"":""unit_name_2_1_bar""},""LastUpdated"":""0001-01-01T00:00:00"",""Precision"":1,""Code"":""value_code_2_1"",""Name"":{""foo"":""value_name_2_1_foo"",""bar"":""value_name_2_1_bar""},""AdditionalProperties"":{},""IsVirtual"":false}],""AdditionalProperties"":{}},{""Type"":""Time"",""Code"":""dimension_code_3"",""Name"":{""foo"":""dimension_name_3_foo"",""bar"":""dimension_name_3_bar""},""Interval"":""Year"",""Values"":[{""Code"":""value_code_3_1"",""Name"":{""foo"":""value_name_3_1_foo"",""bar"":""value_name_3_1_bar""},""AdditionalProperties"":{},""IsVirtual"":false}],""AdditionalProperties"":{}}],""AdditionalProperties"":{""property_1"":{""Type"":""Text"",""Value"":""property_value_1""},""property_2"":{""Type"":""MultilanguageText"",""Value"":{""foo"":""property_value_foo"",""bar"":""property_value_bar""}}}}";
    }
}
