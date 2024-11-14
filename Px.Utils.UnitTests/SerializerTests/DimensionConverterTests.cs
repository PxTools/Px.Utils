using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Models.Metadata.MetaProperties;
using Px.Utils.UnitTests.SerializerTests.Fixtures.Json;
using System.Text.Json;

namespace Px.Utils.UnitTests.SerializerTests
{
    [TestClass]
    public class DimensionConverterTests
    {
        #region Nominal Dimension Tests (same as all other dimension types except time and content)

        [TestMethod]
        public void NominalDimensionSerializationTest()
        {
            Dictionary<string, MetaProperty> additionalProperties = new()
            {
                { "key1", new BooleanProperty(true) }
            };

            List<DimensionValue> dimensions =
            [
                new DimensionValue("valueCode1", new("lang", "valueName1")),
                new DimensionValue("valueCode2", new("lang", "valueName2"))
            ];

            Dimension nominalDim = new("dimCode", new("lang", "dimName"), additionalProperties, dimensions, DimensionType.Nominal);

            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            JsonSerializerOptions options = jsonSerializerOptions;
            string json = JsonSerializer.Serialize(nominalDim, options);
            
            Assert.AreEqual(DimensionJson.NominalDimensionJson, json);
        }

        [TestMethod]
        public void NominalDimensionDeserializationTest()
        {
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            JsonSerializerOptions options = jsonSerializerOptions;
            Dimension? nominalDim = JsonSerializer.Deserialize<Dimension>(DimensionJson.NominalDimensionJson, options);

            Assert.IsNotNull(nominalDim);
            Assert.AreEqual("dimCode", nominalDim.Code);
            Assert.AreEqual("dimName", nominalDim.Name["lang"]);
            Assert.AreEqual(DimensionType.Nominal, nominalDim.Type);
            Assert.AreEqual(1, nominalDim.AdditionalProperties.Count);
            Assert.IsTrue(((BooleanProperty)nominalDim.AdditionalProperties["key1"]).Value);
            Assert.AreEqual(2, nominalDim.Values.Count);
            Assert.AreEqual("valueCode1", nominalDim.Values[0].Code);
            Assert.AreEqual("valueName1", nominalDim.Values[0].Name["lang"]);
            Assert.AreEqual("valueCode2", nominalDim.Values[1].Code);
            Assert.AreEqual("valueName2", nominalDim.Values[1].Name["lang"]);
        }

        [TestMethod]
        public void NominalDimensionSerializeAndDeserializeTest()
        {
            Dictionary<string, MetaProperty> additionalProperties = new()
            {
                { "key1", new BooleanProperty(true) }
            };

            List<DimensionValue> dimensions =
            [
                new DimensionValue("valueCode1", new("lang", "valueName1")),
                new DimensionValue("valueCode2", new("lang", "valueName2"))
            ];

            Dimension nominalDim = new("dimCode", new("lang", "dimName"), additionalProperties, dimensions, DimensionType.Nominal);

            string json = JsonSerializer.Serialize(nominalDim);
            Dimension? deserializedNominalDim = JsonSerializer.Deserialize<Dimension>(json);

            Assert.IsNotNull(deserializedNominalDim);
            Assert.AreEqual(nominalDim.Code, deserializedNominalDim.Code);
            Assert.AreEqual(nominalDim.Name["lang"], deserializedNominalDim.Name["lang"]);
            Assert.AreEqual(nominalDim.Type, deserializedNominalDim.Type);
            Assert.AreEqual(nominalDim.AdditionalProperties.Count, deserializedNominalDim.AdditionalProperties.Count);
            Assert.IsTrue(((BooleanProperty)deserializedNominalDim.AdditionalProperties["key1"]).Value);
            Assert.AreEqual(nominalDim.Values.Count, deserializedNominalDim.Values.Count);
            Assert.AreEqual(nominalDim.Values[0].Code, deserializedNominalDim.Values[0].Code);
            Assert.AreEqual(nominalDim.Values[0].Name["lang"], deserializedNominalDim.Values[0].Name["lang"]);
            Assert.AreEqual(nominalDim.Values[1].Code, deserializedNominalDim.Values[1].Code);
            Assert.AreEqual(nominalDim.Values[1].Name["lang"], deserializedNominalDim.Values[1].Name["lang"]);
        }

        [TestMethod]
        public void NominalDimensionInterfaceAndClassSerializationResultShouldBeTheSame()
        {
            Dictionary<string, MetaProperty> additionalProperties = new()
            {
                { "key1", new BooleanProperty(true) }
            };

            List<DimensionValue> dimensions =
            [
                new DimensionValue("valueCode1", new("lang", "valueName1")),
                new DimensionValue("valueCode2", new("lang", "valueName2"))
            ];

            Dimension nominalDim = new("dimCode", new("lang", "dimName"), additionalProperties, dimensions, DimensionType.Nominal);
            IReadOnlyDimension viaInterface = nominalDim;

            string fromClass = JsonSerializer.Serialize(nominalDim);
            string fromInterface = JsonSerializer.Serialize(viaInterface);

            Assert.AreEqual(fromClass, fromInterface);
        }

        #endregion

        #region Content Dimension Tests

        [TestMethod]
        public void ContentDimensionSerializationTest()
        {
            Dictionary<string, MetaProperty> additionalProperties = new()
            {
                { "key1", new BooleanProperty(true) }
            };

            List<ContentDimensionValue> contentValues =
            [
                new("valueCode1", new("lang", "valueName1"), new("lang", "valueUnit1"), DateTime.MinValue, 1),
                new("valueCode2", new("lang", "valueName2"), new("lang", "valueUnit2"), DateTime.MinValue, 0)
            ];

            ContentDimension contentDim = new("dimCode", new("lang", "dimName"), additionalProperties, contentValues);

            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            JsonSerializerOptions options = jsonSerializerOptions;
            string json = JsonSerializer.Serialize(contentDim, options);

            Assert.AreEqual(DimensionJson.ContentDimensionJson, json);
        }

        [TestMethod]
        public void ContentDimensionDeserializationTest()
        {
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            JsonSerializerOptions options = jsonSerializerOptions;
            ContentDimension? contentDim = JsonSerializer.Deserialize<ContentDimension>(DimensionJson.ContentDimensionJson, options);

            Assert.IsNotNull(contentDim);
            Assert.AreEqual("dimCode", contentDim.Code);
            Assert.AreEqual("dimName", contentDim.Name["lang"]);
            Assert.AreEqual(DimensionType.Content, contentDim.Type);
            Assert.AreEqual(1, contentDim.AdditionalProperties.Count);
            Assert.IsTrue(((BooleanProperty)contentDim.AdditionalProperties["key1"]).Value);
            Assert.AreEqual(2, contentDim.Values.Count);
            Assert.AreEqual("valueCode1", contentDim.Values[0].Code);
            Assert.AreEqual("valueName1", contentDim.Values[0].Name["lang"]);
            Assert.AreEqual("valueUnit1", contentDim.Values[0].Unit["lang"]);
            Assert.AreEqual(DateTime.MinValue, contentDim.Values[0].LastUpdated);
            Assert.AreEqual(1, contentDim.Values[0].Precision);
            Assert.AreEqual("valueCode2", contentDim.Values[1].Code);
            Assert.AreEqual("valueName2", contentDim.Values[1].Name["lang"]);
            Assert.AreEqual("valueUnit2", contentDim.Values[1].Unit["lang"]);
            Assert.AreEqual(DateTime.MinValue, contentDim.Values[1].LastUpdated);
            Assert.AreEqual(0, contentDim.Values[1].Precision);
        }

        [TestMethod]
        public void ContentDimensionSerializationAndDeserializationTest()
        {
            Dictionary<string, MetaProperty> additionalProperties = new()
            {
                { "key1", new BooleanProperty(true) }
            };

            List<ContentDimensionValue> contentValues =
            [
                new("valueCode1", new("lang", "valueName1"), new("lang", "valueUnit1"), DateTime.MinValue, 1),
                new("valueCode2", new("lang", "valueName2"), new("lang", "valueUnit2"), DateTime.MinValue, 0)
            ];

            ContentDimension contentDim = new("dimCode", new("lang", "dimName"), additionalProperties, contentValues);

            string json = JsonSerializer.Serialize(contentDim);
            ContentDimension? deserializedContentDim = JsonSerializer.Deserialize<ContentDimension>(json);

            Assert.IsNotNull(deserializedContentDim);
            Assert.AreEqual(contentDim.Code, deserializedContentDim.Code);
            Assert.AreEqual(contentDim.Name["lang"], deserializedContentDim.Name["lang"]);
            Assert.AreEqual(contentDim.Type, deserializedContentDim.Type);
            Assert.AreEqual(contentDim.AdditionalProperties.Count, deserializedContentDim.AdditionalProperties.Count);
            Assert.IsTrue(((BooleanProperty)deserializedContentDim.AdditionalProperties["key1"]).Value);
            Assert.AreEqual(contentDim.Values.Count, deserializedContentDim.Values.Count);
            Assert.AreEqual(contentDim.Values[0].Code, deserializedContentDim.Values[0].Code);
            Assert.AreEqual(contentDim.Values[0].Name["lang"], deserializedContentDim.Values[0].Name["lang"]);
            Assert.AreEqual(contentDim.Values[0].Unit["lang"], deserializedContentDim.Values[0].Unit["lang"]);
            Assert.AreEqual(contentDim.Values[0].LastUpdated, deserializedContentDim.Values[0].LastUpdated);
            Assert.AreEqual(contentDim.Values[0].Precision, deserializedContentDim.Values[0].Precision);
            Assert.AreEqual(contentDim.Values[1].Code, deserializedContentDim.Values[1].Code);
            Assert.AreEqual(contentDim.Values[1].Name["lang"], deserializedContentDim.Values[1].Name["lang"]);
            Assert.AreEqual(contentDim.Values[1].Unit["lang"], deserializedContentDim.Values[1].Unit["lang"]);
            Assert.AreEqual(contentDim.Values[1].LastUpdated, deserializedContentDim.Values[1].LastUpdated);
            Assert.AreEqual(contentDim.Values[1].Precision, deserializedContentDim.Values[1].Precision);
        }

        [TestMethod]
        public void ContentDimensionInterfaceAndClassSerializationResultShouldBeTheSame()
        {
            Dictionary<string, MetaProperty> additionalProperties = new()
            {
                { "key1", new BooleanProperty(true) }
            };

            List<ContentDimensionValue> contentValues =
            [
                new("valueCode1", new("lang", "valueName1"), new("lang", "valueUnit1"), DateTime.MinValue, 1),
                new("valueCode2", new("lang", "valueName2"), new("lang", "valueUnit2"), DateTime.MinValue, 0)
            ];

            ContentDimension contentDim = new("dimCode", new("lang", "dimName"), additionalProperties, contentValues);
            IReadOnlyDimension viaInterface = contentDim;

            string fromClass = JsonSerializer.Serialize(contentDim);
            string fromInterface = JsonSerializer.Serialize(viaInterface);

            Assert.AreEqual(fromClass, fromInterface);
        }

        #endregion

        #region Time Dimension

        [TestMethod]
        public void TimeDimensionSerializationTest()
        {
            Dictionary<string, MetaProperty> additionalProperties = new()
            {
                { "key1", new BooleanProperty(true) }
            };

            List<DimensionValue> timeValues =
            [
                new DimensionValue("valueCode1", new("lang", "valueName1")),
                new DimensionValue("valueCode2", new("lang", "valueName2"))
            ];

            TimeDimensionInterval interval = TimeDimensionInterval.Year;

            TimeDimension timeDim = new("dimCode", new("lang", "dimName"), additionalProperties, timeValues, interval);

            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            JsonSerializerOptions options = jsonSerializerOptions;
            string json = JsonSerializer.Serialize(timeDim, options);

            Assert.AreEqual(DimensionJson.TimeDimensionJson, json);
        }

        [TestMethod]
        public void TimeDimensionDeserializationTest()
        {
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            JsonSerializerOptions options = jsonSerializerOptions;
            TimeDimension? timeDim = JsonSerializer.Deserialize<TimeDimension>(DimensionJson.TimeDimensionJson, options);

            Assert.IsNotNull(timeDim);
            Assert.AreEqual("dimCode", timeDim.Code);
            Assert.AreEqual("dimName", timeDim.Name["lang"]);
            Assert.AreEqual(DimensionType.Time, timeDim.Type);
            Assert.AreEqual(1, timeDim.AdditionalProperties.Count);
            Assert.IsTrue(((BooleanProperty)timeDim.AdditionalProperties["key1"]).Value);
            Assert.AreEqual(2, timeDim.Values.Count);
            Assert.AreEqual("valueCode1", timeDim.Values[0].Code);
            Assert.AreEqual("valueName1", timeDim.Values[0].Name["lang"]);
            Assert.AreEqual("valueCode2", timeDim.Values[1].Code);
            Assert.AreEqual("valueName2", timeDim.Values[1].Name["lang"]);
            Assert.AreEqual(TimeDimensionInterval.Year, timeDim.Interval);
        }

        [TestMethod]
        public void TimedimensionSerializationAndDeserializationTest()
        {
            Dictionary<string, MetaProperty> additionalProperties = new()
            {
                { "key1", new BooleanProperty(true) }
            };

            List<DimensionValue> timeValues =
            [
                new DimensionValue("valueCode1", new("lang", "valueName1")),
                new DimensionValue("valueCode2", new("lang", "valueName2"))
            ];

            TimeDimensionInterval interval = TimeDimensionInterval.Year;

            TimeDimension timeDim = new("dimCode", new("lang", "dimName"), additionalProperties, timeValues, interval);

            string json = JsonSerializer.Serialize(timeDim);
            TimeDimension? deserializedTimeDim = JsonSerializer.Deserialize<TimeDimension>(json);

            Assert.IsNotNull(deserializedTimeDim);
            Assert.AreEqual(timeDim.Code, deserializedTimeDim.Code);
            Assert.AreEqual(timeDim.Name["lang"], deserializedTimeDim.Name["lang"]);
            Assert.AreEqual(timeDim.Type, deserializedTimeDim.Type);
            Assert.AreEqual(timeDim.AdditionalProperties.Count, deserializedTimeDim.AdditionalProperties.Count);
            Assert.IsTrue(((BooleanProperty)deserializedTimeDim.AdditionalProperties["key1"]).Value);
            Assert.AreEqual(timeDim.Values.Count, deserializedTimeDim.Values.Count);
            Assert.AreEqual(timeDim.Values[0].Code, deserializedTimeDim.Values[0].Code);
            Assert.AreEqual(timeDim.Values[0].Name["lang"], deserializedTimeDim.Values[0].Name["lang"]);
            Assert.AreEqual(timeDim.Values[1].Code, deserializedTimeDim.Values[1].Code);
            Assert.AreEqual(timeDim.Values[1].Name["lang"], deserializedTimeDim.Values[1].Name["lang"]);
            Assert.AreEqual(timeDim.Interval, deserializedTimeDim.Interval);
        }

        [TestMethod]
        public void TimeDimensionInterfaceAndClassSerializationResultShouldBeTheSame()
        {
            Dictionary<string, MetaProperty> additionalProperties = new()
            {
                { "key1", new BooleanProperty(true) }
            };

            List<DimensionValue> timeValues =
            [
                new DimensionValue("valueCode1", new("lang", "valueName1")),
                new DimensionValue("valueCode2", new("lang", "valueName2"))
            ];

            TimeDimensionInterval interval = TimeDimensionInterval.Year;

            TimeDimension timeDim = new("dimCode", new("lang", "dimName"), additionalProperties, timeValues, interval);
            IReadOnlyDimension viaInterface = timeDim;

            string fromClass = JsonSerializer.Serialize(timeDim);
            string fromInterface = JsonSerializer.Serialize(viaInterface);

            Assert.AreEqual(fromClass, fromInterface);
        }

        #endregion
    }
}
