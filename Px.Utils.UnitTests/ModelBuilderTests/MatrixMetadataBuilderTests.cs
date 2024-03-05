using Px.Utils.UnitTests.ModelBuilderTests.Fixtures;
using PxUtils.Language;
using PxUtils.ModelBuilders;
using PxUtils.Models.Metadata;
using PxUtils.Models.Metadata.Dimensions;
using PxUtils.Models.Metadata.Enums;
using System.Globalization;

namespace ModelBuilderTests
{
    [TestClass]
    public class MatrixMetadataBuilderTests
    {
        private MatrixMetadata Actual { get; } = new MatrixMetadataBuilder().Build(PxFileMetaEntries_Robust_3_Languages.Entries);

        [TestMethod]
        public void TableLevelMetaLanguageTests()
        {
            Assert.AreEqual("fi", Actual.DefaultLanguage);
            CollectionAssert.AreEqual(new List<string> { "fi", "sv", "en" }, Actual.AvailableLanguages.ToList());
        }
        
        [DataTestMethod]
        [DataRow("ANSI", "CHARSET")]
        [DataRow("2013", "AXIS-VERSION")]
        [DataRow("iso-8859-15", "CODEPAGE")]
        [DataRow("20200121 09:00", "CREATION-DATE")]
        [DataRow("20240131 08:00", "NEXT-UPDATE")]
        [DataRow("YES", "OFFICIAL-STATISTICS")]
        public void TableLevelAdditionalNotTranslatedParametersTest(string expected, string keyWord)
        {
            Assert.AreEqual(expected, Actual.AdditionalProperties[keyWord].GetString());
        }

        [DataTestMethod]
        [DataRow("abcd", "abcd", "abcd", "SUBJECT-AREA")]
        [DataRow("test_description_fi", "test_description_sv", "test_description_en", "DESCRIPTION")]
        [DataRow("test_note_fi", "test_note_sv", "test_note_en", "NOTE")]
        public void TableLevelAdditionalTranslatedParametersTest(string fi, string sv, string en, string keyWord)
        {
            MultilanguageString expected = new([new("fi", fi), new("sv", sv), new("en", en)]);
            Assert.AreEqual(expected, Actual.AdditionalProperties[keyWord].GetMultiLanguageString());
        }

        [TestMethod]
        public void VariableBuildTest()
        {
            Assert.AreEqual(4, Actual.Dimensions.Count);

            List<string> expectedCodes = ["Vuosi", "Alue", "Talotyyppi", "Tiedot"];
            CollectionAssert.AreEqual(expectedCodes, Actual.Dimensions.Select(d => d.Code).ToList());

            List<MultilanguageString> expectedNames = [
                new([new("fi", "Vuosi"), new("sv", "År"), new("en", "Year")]),
                new([new("fi", "Alue"), new("sv", "Område"), new("en", "Region")]),
                new([new("fi", "Talotyyppi"), new("sv", "Hustyp"), new("en", "Building type")]),
                new([new("fi", "Tiedot"), new("sv", "Uppgifter"), new("en", "Information")])
                ];
            CollectionAssert.AreEqual(expectedNames, Actual.Dimensions.Select(d => d.Name).ToList());
        }

        [TestMethod]
        public void ContentDimensionBuildTest()
        {
            ContentDimension? contentDimension = (ContentDimension?)Actual.Dimensions.Find(d => d.Type == DimensionType.Content);
            Assert.IsNotNull(contentDimension);
            Assert.AreEqual(3, contentDimension.Values.Count);
            Assert.AreEqual("Tiedot", contentDimension.Code);
            Assert.IsFalse(Actual.AdditionalProperties.ContainsKey("CONTVARIABLE"));
        }

        [DataTestMethod]
        [DataRow(0, "indeksipisteluku", "indextal", "index point")]
        [DataRow(1, "%", "%", "%")]
        [DataRow(2, "lukumäärä", "antal", "number")]
        public void UnitsTest(int index, string fi, string sv, string en)
        {
            ContentDimension? contentDimension = (ContentDimension?)Actual.Dimensions.Find(d => d.Type == DimensionType.Content);
            MultilanguageString expected = new([new("fi", fi), new("sv", sv), new("en", en)]);
            Assert.AreEqual(expected, contentDimension?.Values[index].Unit);
            Assert.IsFalse(contentDimension?.Values[index].AdditionalProperties.ContainsKey("UNIT"));
        }

        [DataTestMethod]
        [DataRow(0, "20230131 08:00")]
        [DataRow(1, "20230131 09:00")]
        [DataRow(2, "20230131 10:00")]
        public void LastUpdatedTest(int index, string timeStamp)
        {
            ContentDimension? contentDimension = (ContentDimension?)Actual.Dimensions.Find(d => d.Type == DimensionType.Content);
            DateTime expected = DateTime.ParseExact(timeStamp, "yyyyMMdd HH:mm", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, contentDimension?.Values[index].LastUpdated);
            Assert.IsFalse(contentDimension?.Values[index].AdditionalProperties.ContainsKey("LAST-UPDATED"));
        }

        [TestMethod]
        public void TimeDimensionBuildTest()
        {
            TimeDimension? timeDimension = (TimeDimension?)Actual.Dimensions.Find(d => d.Type == DimensionType.Time);
            Assert.IsNotNull(timeDimension);
            Assert.AreEqual("Vuosi", timeDimension.Code);
            Assert.AreEqual(TimeDimensionInterval.Year, timeDimension.Interval);
            Assert.AreEqual(8, timeDimension.Values.Count);
            Assert.IsTrue(timeDimension.AdditionalProperties.ContainsKey("TIMEVAL"));
            Assert.IsFalse(Actual.AdditionalProperties.ContainsKey("TIMEVAL"));
        }

        [TestMethod]
        public void DefaultDimensionValueTest()
        {
            IDimension? building_type_dim = Actual.Dimensions.Find(d => d.Code == "Talotyyppi");
            Assert.IsNotNull(building_type_dim);
            DimensionValue? defaultValue = building_type_dim.DefaultValue;
            Assert.IsNotNull(defaultValue);
            Assert.AreEqual("0", defaultValue.Code);
            MultilanguageString expected = new([new("fi", "Talotyypit yhteensä"), new("sv", "Hustyp totalt"), new("en", "Building types total")]);
            Assert.AreEqual(expected, defaultValue.Name);
        }

        [TestMethod]
        public void DimensionValuesTest()
        {
            IDimension dim0 = Actual.Dimensions[0];
            Assert.AreEqual(8, dim0.Values.Count);
            List<string> expected0 = ["2015", "2016", "2017", "2018", "2019", "2020", "2021", "2022"];
            CollectionAssert.AreEqual(expected0, dim0.Values.Select(v => v.Code).ToList());

            IDimension dim1 = Actual.Dimensions[1];
            Assert.AreEqual(7, dim1.Values.Count);
            List<string> expected1 = ["ksu", "pks", "msu", "091", "049", "092", "853"];
            CollectionAssert.AreEqual(expected1, dim1.Values.Select(v => v.Code).ToList());

            IDimension dim2 = Actual.Dimensions[2];
            Assert.AreEqual(3, dim2.Values.Count);
            List<string> expected2 = ["0", "1", "3"];
            CollectionAssert.AreEqual(expected2, dim2.Values.Select(v => v.Code).ToList());

            IDimension dim3 = Actual.Dimensions[3];
            Assert.AreEqual(3, dim3.Values.Count);
            List<string> expected3 = ["ketjutettu_lv", "vmuutos_lv", "lkm_julk_uudet"];
            CollectionAssert.AreEqual(expected3, dim3.Values.Select(v => v.Code).ToList());
        }
    }
}
