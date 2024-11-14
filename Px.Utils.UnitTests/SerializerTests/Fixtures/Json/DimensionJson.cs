namespace Px.Utils.UnitTests.SerializerTests.Fixtures.Json
{
    internal static class DimensionJson
    {
        public static string NominalDimensionJson = @"{
  ""type"": ""Nominal"",
  ""code"": ""dimCode"",
  ""name"": {
    ""lang"": ""dimName""
  },
  ""values"": [
    {
      ""code"": ""valueCode1"",
      ""name"": {
        ""lang"": ""valueName1""
      },
      ""additionalProperties"": {},
      ""isVirtual"": false
    },
    {
      ""code"": ""valueCode2"",
      ""name"": {
        ""lang"": ""valueName2""
      },
      ""additionalProperties"": {},
      ""isVirtual"": false
    }
  ],
  ""additionalProperties"": {
    ""key1"": {
      ""type"": ""Boolean"",
      ""value"": true
    }
  }
}";

        public static string ContentDimensionJson = @"{
  ""type"": ""Content"",
  ""code"": ""dimCode"",
  ""name"": {
    ""lang"": ""dimName""
  },
  ""values"": [
    {
      ""unit"": {
        ""lang"": ""valueUnit1""
      },
      ""lastUpdated"": ""0001-01-01T00:00:00"",
      ""precision"": 1,
      ""code"": ""valueCode1"",
      ""name"": {
        ""lang"": ""valueName1""
      },
      ""additionalProperties"": {},
      ""isVirtual"": false
    },
    {
      ""unit"": {
        ""lang"": ""valueUnit2""
      },
      ""lastUpdated"": ""0001-01-01T00:00:00"",
      ""precision"": 0,
      ""code"": ""valueCode2"",
      ""name"": {
        ""lang"": ""valueName2""
      },
      ""additionalProperties"": {},
      ""isVirtual"": false
    }
  ],
  ""additionalProperties"": {
    ""key1"": {
      ""type"": ""Boolean"",
      ""value"": true
    }
  }
}";

        public static string TimeDimensionJson = @"{
  ""type"": ""Time"",
  ""code"": ""dimCode"",
  ""name"": {
    ""lang"": ""dimName""
  },
  ""interval"": ""Year"",
  ""values"": [
    {
      ""code"": ""valueCode1"",
      ""name"": {
        ""lang"": ""valueName1""
      },
      ""additionalProperties"": {},
      ""isVirtual"": false
    },
    {
      ""code"": ""valueCode2"",
      ""name"": {
        ""lang"": ""valueName2""
      },
      ""additionalProperties"": {},
      ""isVirtual"": false
    }
  ],
  ""additionalProperties"": {
    ""key1"": {
      ""type"": ""Boolean"",
      ""value"": true
    }
  }
}";
    }
}
