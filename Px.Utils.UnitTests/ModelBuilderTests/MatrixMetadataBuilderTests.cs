using ModelBuilderTests.Fixtures;
using Px.Utils.Language;
using Px.Utils.ModelBuilders;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Models.Metadata.MetaProperties;
using Px.Utils.PxFile;
using Px.Utils.UnitTests.ModelBuilderTests.Fixtures;
using System.Globalization;
using Px.Utils.PxFile;
using Px.Utils.UnitTests.ModelBuilderTests.Fixtures;

namespace ModelBuilderTests
{
    [TestClass]
    public class MatrixMetadataBuilderTests
    {
        private MatrixMetadata Actual_3Lang { get; } = new MatrixMetadataBuilder().Build(PxFileMetaEntries_Robust_3_Languages.Entries);
        private MatrixMetadata Actual_1Lang { get; } = new MatrixMetadataBuilder().Build(PxFileMetaEntries_Robust_1_Language.Entries);
        private MatrixMetadata Actual_Recommended_3Lang { get; } = new MatrixMetadataBuilder().Build(PxFileMetaEntries_Recommended_3_Langs.Entries);
        private MatrixMetadata Actual_1Lang_With_Table_Level_Units_And_Precision { get; } = 
            new MatrixMetadataBuilder().Build(PxFileMetaEntries_Robust_1_Language_With_Table_Level_Units_And_Precision.Entries);

        [TestMethod]
        public void IEnumerableBuildTest()
        {
            MatrixMetadataBuilder builder = new();
            MatrixMetadata meta = builder.Build(PxFileMetaEntries_Robust_3_Languages.Entries);
            Assert.IsNotNull(meta);
        }
        
        [TestMethod]
        public void DictionaryBuildTest()
        {
            MatrixMetadataBuilder builder = new();
            Dictionary<string, string> inputDict = new(PxFileMetaEntries_Robust_3_Languages.Entries);
            MatrixMetadata meta = builder.Build(inputDict);
            Assert.IsNotNull(meta);
        }

        [TestMethod]
        public async Task IAsyncEnumerableBuildTest()
        {
            MatrixMetadataBuilder builder = new();
            IAsyncEnumerable<KeyValuePair<string, string>> asyncEntries = PxFileMetaEntries_Robust_3_Languages.Entries.ToAsyncEnumerable();
            MatrixMetadata meta = await builder.BuildAsync(asyncEntries);
            Assert.IsNotNull(meta);
        }

        [TestMethod]
        public void MultiLangTableLevelMetaLanguageTests()
        {
            Assert.AreEqual("fi", Actual_3Lang.DefaultLanguage);
            CollectionAssert.AreEqual(new List<string> { "fi", "sv", "en" }, Actual_3Lang.AvailableLanguages.ToList());
        }

        [TestMethod]
        public void SingleLangTableLevelMetaLanguageTests()
        {
            Assert.AreEqual("fi", Actual_1Lang.DefaultLanguage);
            CollectionAssert.AreEqual(new List<string> { "fi" }, Actual_1Lang.AvailableLanguages.ToList());
        }

        [DataTestMethod]
        [DataRow("ANSI", "CHARSET")]
        [DataRow("2013", "AXIS-VERSION")]
        [DataRow("iso-8859-15", "CODEPAGE")]
        [DataRow("20200121 09:00", "CREATION-DATE")]
        [DataRow("20240131 08:00", "NEXT-UPDATE")]
        public void MultiLangTableLevelAdditionalNotTranslatedStringParametersTest(string expected, string keyWord)
        {
            if(Actual_3Lang.AdditionalProperties[keyWord] is StringProperty asp)
            {
                Assert.AreEqual(expected, asp.Value);
            }
            else
            {
                Assert.Fail("Property is not a StringProperty");
            }
        }

        [DataTestMethod]
        [DataRow(true, "OFFICIAL-STATISTICS")]
        public void MultiLangTableLevelAdditionalBoolParametersTest(bool expected, string keyWord)
        {
            if(Actual_3Lang.AdditionalProperties[keyWord] is BooleanProperty abp)
            {
                Assert.AreEqual(expected, abp.Value);
            }
            else
            {
                Assert.Fail("Property is not a BooleanProperty");
            }
        }

        [DataTestMethod]
        [DataRow("ANSI", "CHARSET")]
        [DataRow("2013", "AXIS-VERSION")]
        [DataRow("iso-8859-15", "CODEPAGE")]
        [DataRow("20200121 09:00", "CREATION-DATE")]
        [DataRow("20240131 08:00", "NEXT-UPDATE")]
        public void SingleLangTableLevelAdditionalNotTranslatedParametersTest(string expected, string keyWord)
        {
            if(Actual_1Lang.AdditionalProperties[keyWord] is StringProperty asp)
            {
                Assert.AreEqual(expected, asp.Value);
            }
            else
            {
                Assert.Fail("Property is not a StringProperty");
            }
        }

        [DataTestMethod]
        [DataRow(true, "OFFICIAL-STATISTICS")]
        public void SingleLangTableLevelAdditionalNotTranslatedParametersTest(bool expected, string keyWord)
        {
            if(Actual_1Lang.AdditionalProperties[keyWord] is BooleanProperty abp)
            {
                Assert.AreEqual(expected, abp.Value);
            }
            else
            {
                Assert.Fail("Property is not a BooleanProperty");
            }
        }

        [DataTestMethod]
        [DataRow("abcd", "abcd", "abcd", "SUBJECT-AREA")]
        [DataRow("test_description_fi", "test_description_sv", "test_description_en", "DESCRIPTION")]
        [DataRow("test_note_fi", "test_note_sv", "test_note_en", "NOTE")]
        public void MultiLangTableLevelAdditionalTranslatedParametersTest(string fi, string sv, string en, string keyWord)
        {
            MultilanguageString expected = new([new("fi", fi), new("sv", sv), new("en", en)]);
            if (Actual_3Lang.AdditionalProperties[keyWord] is MultilanguageStringProperty msp)
            {
                Assert.AreEqual(expected, msp.Value);
            }
            else
            {
                Assert.Fail("Property is not a MultilanguageStringProperty");
            }
        }

        [DataTestMethod]
        [DataRow("abcd", "SUBJECT-AREA")]
        [DataRow("test_description_fi", "DESCRIPTION")]
        [DataRow("test_note_fi", "NOTE")]
        public void SingleLangTableLevelAdditionalTranslatedParametersTest(string input, string keyWord)
        {
            if(Actual_1Lang.AdditionalProperties[keyWord] is StringProperty msp)
            {
                Assert.AreEqual(input, msp.Value);
            }
            else
            {
                Assert.Fail("Property is not a StringProperty");
            }
        }

        [TestMethod]
        public void MultiLangVariableBuildTest()
        {
            Assert.AreEqual(4, Actual_3Lang.Dimensions.Count);

            List<string> expectedCodes = ["Vuosi", "Alue", "Talotyyppi", "Tiedot"];
            CollectionAssert.AreEqual(expectedCodes, Actual_3Lang.Dimensions.Select(d => d.Code).ToList());

            List<MultilanguageString> expectedNames = [
                new([new("fi", "Vuosi"), new("sv", "År"), new("en", "Year")]),
                new([new("fi", "Alue"), new("sv", "Område"), new("en", "Region")]),
                new([new("fi", "Talotyyppi"), new("sv", "Hustyp"), new("en", "Building type")]),
                new([new("fi", "Tiedot"), new("sv", "Uppgifter"), new("en", "Information")])
                ];
            CollectionAssert.AreEqual(expectedNames, Actual_3Lang.Dimensions.Select(d => d.Name).ToList());
        }

        [TestMethod]
        public void MultiLangVariableBuildTestFromRecommendedFixture()
        {
            Assert.AreEqual(4, Actual_Recommended_3Lang.Dimensions.Count);

            List<string> expectedCodes = ["year", "area", "type", "info"];
            CollectionAssert.AreEqual(expectedCodes, Actual_Recommended_3Lang.Dimensions.Select(d => d.Code).ToList());

            List<MultilanguageString> expectedNames = [
                new([new("fi", "Vuosi"), new("sv", "År"), new("en", "Year")]),
                new([new("fi", "Alue"), new("sv", "Område"), new("en", "Region")]),
                new([new("fi", "Talotyyppi"), new("sv", "Hustyp"), new("en", "Building type")]),
                new([new("fi", "Tiedot"), new("sv", "Uppgifter"), new("en", "Information")])
                ];
            CollectionAssert.AreEqual(expectedNames, Actual_Recommended_3Lang.Dimensions.Select(d => d.Name).ToList());
        }

        [TestMethod]
        public void SingleLangVariableBuildTest()
        {
            Assert.AreEqual(4, Actual_1Lang.Dimensions.Count);

            List<string> expectedCodes = ["Vuosi", "Alue", "Talotyyppi", "Tiedot"];
            CollectionAssert.AreEqual(expectedCodes, Actual_1Lang.Dimensions.Select(d => d.Code).ToList());

            List<MultilanguageString> expectedNames = [
                new("fi", "Vuosi"),
                new("fi", "Alue"),
                new("fi", "Talotyyppi"),
                new("fi", "Tiedot")
                ];
            CollectionAssert.AreEqual(expectedNames, Actual_1Lang.Dimensions.Select(d => d.Name).ToList());
        }

        [TestMethod]
        public void SingleLangWithTableLevelUnitsAndPrecisionBuildTest()
        {
            ContentDimension? contentDimension = (ContentDimension?)Actual_1Lang_With_Table_Level_Units_And_Precision.Dimensions.Find(d => d.Type == DimensionType.Content);
            Assert.IsNotNull(contentDimension);
            MultilanguageString[] expectedUnits = [
                new("fi", "indeksipisteluku"),
                new("fi", "%"),
                new("fi", "lukumäärä")
            ];
            for(int i = 0; i < contentDimension.Values.Count; i++)
            {
                Assert.AreEqual(expectedUnits[i], contentDimension.Values[i].Unit);
            }
            for(int i = 0; i < contentDimension.Values.Count; i++)
            {
                Assert.AreEqual(1, contentDimension.Values[i].Precision);
            }
            Assert.IsFalse(Actual_1Lang_With_Table_Level_Units_And_Precision.AdditionalProperties.ContainsKey(PxFileSyntaxConf.Default.Tokens.KeyWords.Units));
            Assert.IsFalse(Actual_1Lang_With_Table_Level_Units_And_Precision.AdditionalProperties.ContainsKey(PxFileSyntaxConf.Default.Tokens.KeyWords.Decimals));
            Assert.IsFalse(Actual_1Lang_With_Table_Level_Units_And_Precision.AdditionalProperties.ContainsKey(PxFileSyntaxConf.Default.Tokens.KeyWords.ShowDecimals));
        }

        #region Content Dimension Tests

        [TestMethod]
        public void MultiLangContentDimensionBuildTest()
        {
            Dimension? contentDimension = Actual_3Lang.Dimensions.Find(d => d.Type == DimensionType.Content);
            Assert.IsInstanceOfType<ContentDimension>(contentDimension);
            Assert.IsNotNull(contentDimension);
            Assert.AreEqual(3, contentDimension.Values.Count);
            Assert.AreEqual("Tiedot", contentDimension.Code);
            Assert.IsFalse(Actual_3Lang.AdditionalProperties.ContainsKey("CONTVARIABLE"));
        }

        [TestMethod]
        public void SingleLangContentDimensionBuildTest()
        {
            Dimension? contentDimension = (ContentDimension?)Actual_1Lang.Dimensions.Find(d => d.Type == DimensionType.Content);
            Assert.IsInstanceOfType<ContentDimension>(contentDimension);
            Assert.IsNotNull(contentDimension);
            Assert.AreEqual(3, contentDimension.Values.Count);
            Assert.AreEqual("Tiedot", contentDimension.Code);
            Assert.IsFalse(Actual_1Lang.AdditionalProperties.ContainsKey("CONTVARIABLE"));
        }

        [DataTestMethod]
        [DataRow(0, "indeksipisteluku", "indextal", "index point")]
        [DataRow(1, "%", "%", "%")]
        [DataRow(2, "lukumäärä", "antal", "number")]
        public void MultiLangUnitsTest(int index, string fi, string sv, string en)
        {
            ContentDimension? contentDimension = (ContentDimension?)Actual_3Lang.Dimensions.Find(d => d.Type == DimensionType.Content);
            MultilanguageString expected = new([new("fi", fi), new("sv", sv), new("en", en)]);
            Assert.AreEqual(expected, contentDimension?.Values[index].Unit);
            Assert.IsFalse(contentDimension?.Values[index].AdditionalProperties.ContainsKey("UNIT"));
        }

        [DataTestMethod]
        [DataRow(0, "indeksipisteluku")]
        [DataRow(1, "%")]
        [DataRow(2, "lukumäärä")]
        public void SingleLangUnitsTest(int index, string fi)
        {
            ContentDimension? contentDimension = (ContentDimension?)Actual_1Lang.Dimensions.Find(d => d.Type == DimensionType.Content);
            MultilanguageString expected = new("fi", fi);
            Assert.AreEqual(expected, contentDimension?.Values[index].Unit);
            Assert.IsFalse(contentDimension?.Values[index].AdditionalProperties.ContainsKey("UNIT"));
        }

        [DataTestMethod]
        [DataRow(0, "20230131 08:00")]
        [DataRow(1, "20230131 09:00")]
        [DataRow(2, "20230131 10:00")]
        public void MultiLangLastUpdatedTest(int index, string timeStamp)
        {
            ContentDimension? contentDimension = (ContentDimension?)Actual_3Lang.Dimensions.Find(d => d.Type == DimensionType.Content);
            DateTime expected = DateTime.ParseExact(timeStamp, "yyyyMMdd HH:mm", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, contentDimension?.Values[index].LastUpdated);
            Assert.IsFalse(contentDimension?.Values[index].AdditionalProperties.ContainsKey("LAST-UPDATED"));
        }

        [DataTestMethod]
        [DataRow(0, "20230131 08:00")]
        [DataRow(1, "20230131 09:00")]
        [DataRow(2, "20230131 10:00")]
        public void SingleLangLastUpdatedTest(int index, string timeStamp)
        {
            ContentDimension? contentDimension = (ContentDimension?)Actual_1Lang.Dimensions.Find(d => d.Type == DimensionType.Content);
            DateTime expected = DateTime.ParseExact(timeStamp, "yyyyMMdd HH:mm", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, contentDimension?.Values[index].LastUpdated);
            Assert.IsFalse(contentDimension?.Values[index].AdditionalProperties.ContainsKey("LAST-UPDATED"));
        }

        [DataTestMethod]
        [DataRow(0, 1)]
        [DataRow(1, 1)]
        [DataRow(2, 0)]
        public void MultiLangPrecisionTest(int index, int expected)
        {
            ContentDimension? contentDimension = (ContentDimension?)Actual_3Lang.Dimensions.Find(d => d.Type == DimensionType.Content);
            Assert.AreEqual(expected, contentDimension?.Values[index].Precision);
            Assert.IsFalse(contentDimension?.Values[index].AdditionalProperties.ContainsKey("PRECISION"));
        }

        [DataTestMethod]
        [DataRow(0, 1)]
        [DataRow(1, 1)]
        [DataRow(2, 0)]
        public void SingleLangPrecisionTest(int index, int expected)
        {
            ContentDimension? contentDimension = (ContentDimension?)Actual_1Lang.Dimensions.Find(d => d.Type == DimensionType.Content);
            Assert.AreEqual(expected, contentDimension?.Values[index].Precision);
            Assert.IsFalse(contentDimension?.Values[index].AdditionalProperties.ContainsKey("PRECISION"));
        }

        [TestMethod]
        public void MultiLangContentDimensionBuildTestFromRecommendedFixture()
        {
            Dimension? contentDimension = Actual_Recommended_3Lang.Dimensions.Find(d => d.Type == DimensionType.Content);
            Assert.IsInstanceOfType<ContentDimension>(contentDimension);
            Assert.IsNotNull(contentDimension);
            Assert.AreEqual(3, contentDimension.Values.Count);
            Assert.AreEqual("info", contentDimension.Code);
            Assert.IsFalse(Actual_3Lang.AdditionalProperties.ContainsKey("CONTVARIABLE"));
        }

        [DataTestMethod]
        [DataRow(0, "indeksipisteluku", "indextal", "index point")]
        [DataRow(1, "%", "%", "%")]
        [DataRow(2, "lukumäärä", "antal", "number")]
        public void MultiLangUnitsTestFromRecommendedFixture(int index, string fi, string sv, string en)
        {
            ContentDimension? contentDimension = (ContentDimension?)Actual_Recommended_3Lang.Dimensions.Find(d => d.Type == DimensionType.Content);
            MultilanguageString expected = new([new("fi", fi), new("sv", sv), new("en", en)]);
            Assert.AreEqual(expected, contentDimension?.Values[index].Unit);
            Assert.IsFalse(contentDimension?.Values[index].AdditionalProperties.ContainsKey("UNIT"));
        }

        [DataTestMethod]
        [DataRow(0, "20230131 08:00")]
        [DataRow(1, "20230131 09:00")]
        [DataRow(2, "20230131 10:00")]
        public void MultiLangLastUpdatedTestFromRecommendedFixture(int index, string timeStamp)
        {
            ContentDimension? contentDimension = (ContentDimension?)Actual_Recommended_3Lang.Dimensions.Find(d => d.Type == DimensionType.Content);
            DateTime expected = DateTime.ParseExact(timeStamp, "yyyyMMdd HH:mm", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, contentDimension?.Values[index].LastUpdated);
            Assert.IsFalse(contentDimension?.Values[index].AdditionalProperties.ContainsKey("LAST-UPDATED"));
        }

        [DataTestMethod]
        [DataRow(0, 1)]
        [DataRow(1, 1)]
        [DataRow(2, 0)]
        public void MultiLangPrecisionTestFromRecommendedFixture(int index, int expected)
        {
            ContentDimension? contentDimension = (ContentDimension?)Actual_Recommended_3Lang.Dimensions.Find(d => d.Type == DimensionType.Content);
            Assert.AreEqual(expected, contentDimension?.Values[index].Precision);
            Assert.IsFalse(contentDimension?.Values[index].AdditionalProperties.ContainsKey("PRECISION"));
        }

        #endregion

        #region Time Dimension Tests

        [TestMethod]
        public void MultiLangTimeDimensionBuildTest()
        {
            TimeDimension? timeDimension = (TimeDimension?)Actual_3Lang.Dimensions.Find(d => d.Type == DimensionType.Time);
            Assert.IsNotNull(timeDimension);
            Assert.AreEqual("Vuosi", timeDimension.Code);
            Assert.AreEqual(TimeDimensionInterval.Year, timeDimension.Interval);
            Assert.AreEqual(8, timeDimension.Values.Count);
            Assert.IsTrue(timeDimension.AdditionalProperties.ContainsKey("TIMEVAL"));
            Assert.IsFalse(Actual_3Lang.AdditionalProperties.ContainsKey("TIMEVAL"));
        }

        [TestMethod]
        public void SingleLangTimeDimensionBuildTest()
        {
            TimeDimension? timeDimension = (TimeDimension?)Actual_1Lang.Dimensions.Find(d => d.Type == DimensionType.Time);
            Assert.IsNotNull(timeDimension);
            Assert.AreEqual("Vuosi", timeDimension.Code);
            Assert.AreEqual(TimeDimensionInterval.Year, timeDimension.Interval);
            Assert.AreEqual(8, timeDimension.Values.Count);
            Assert.IsTrue(timeDimension.AdditionalProperties.ContainsKey("TIMEVAL"));
            Assert.IsFalse(Actual_3Lang.AdditionalProperties.ContainsKey("TIMEVAL"));
        }

        #endregion

        [TestMethod]
        public void MultiLangDefaultDimensionValueTest()
        {
            Dimension? building_type_dim = Actual_3Lang.Dimensions.Find(d => d.Code == "Talotyyppi");
            Assert.IsNotNull(building_type_dim);
            if(building_type_dim.AdditionalProperties["ELIMINATION"] is MultilanguageStringProperty msp)
            {
                MultilanguageString expected = new([new("fi", "Talotyypit yhteensä"), new("sv", "Hustyp totalt"), new("en", "Building types total")]);
                Assert.AreEqual(expected, msp.Value);
            }
            else
            {
                Assert.Fail("Property is not a MultilanguageStringProperty");
            }
        }

        [TestMethod]
        public void SingleLangDefaultDimensionValueTest()
        {
            Dimension? building_type_dim = Actual_1Lang.Dimensions.Find(d => d.Code == "Talotyyppi");
            Assert.IsNotNull(building_type_dim);
            if(building_type_dim.AdditionalProperties["ELIMINATION"] is StringProperty msp)
            {
                Assert.AreEqual("Talotyypit yhteensä", msp.Value);
            }
            else
            {
                Assert.Fail("Property is not a StringProperty");
            }
        }

        [TestMethod]
        public void MultiLangDimensionValuesTest()
        {
            Dimension dim0 = Actual_3Lang.Dimensions[0];
            Assert.AreEqual(8, dim0.Values.Count);
            List<string> expected0 = ["2015", "2016", "2017", "2018", "2019", "2020", "2021", "2022"];
            CollectionAssert.AreEqual(expected0, dim0.Values.Codes.ToList());

            Dimension dim1 = Actual_3Lang.Dimensions[1];
            Assert.AreEqual(7, dim1.Values.Count);
            List<string> expected1 = ["ksu", "pks", "msu", "091", "049", "092", "853"];
            CollectionAssert.AreEqual(expected1, dim1.Values.Codes.ToList());

            Dimension dim2 = Actual_3Lang.Dimensions[2];
            Assert.AreEqual(3, dim2.Values.Count);
            List<string> expected2 = ["0", "1", "3"];
            CollectionAssert.AreEqual(expected2, dim2.Values.Codes.ToList());

            Dimension dim3 = Actual_3Lang.Dimensions[3];
            Assert.AreEqual(3, dim3.Values.Count);
            List<string> expected3 = ["ketjutettu_lv", "vmuutos_lv", "lkm_julk_uudet"];
            CollectionAssert.AreEqual(expected3, dim3.Values.Codes.ToList());
        }

        [TestMethod]
        public void SingleLangDimensionValuesTest()
        {
            Dimension dim0 = Actual_1Lang.Dimensions[0];
            Assert.AreEqual(8, dim0.Values.Count);
            List<string> expected0 = ["2015", "2016", "2017", "2018", "2019", "2020", "2021", "2022"];
            CollectionAssert.AreEqual(expected0, dim0.Values.Codes.ToList());

            Dimension dim1 = Actual_1Lang.Dimensions[1];
            Assert.AreEqual(7, dim1.Values.Count);
            List<string> expected1 = ["ksu", "pks", "msu", "091", "049", "092", "853"];
            CollectionAssert.AreEqual(expected1, dim1.Values.Codes.ToList());

            Dimension dim2 = Actual_1Lang.Dimensions[2];
            Assert.AreEqual(3, dim2.Values.Count);
            List<string> expected2 = ["0", "1", "3"];
            CollectionAssert.AreEqual(expected2, dim2.Values.Codes.ToList());

            Dimension dim3 = Actual_1Lang.Dimensions[3];
            Assert.AreEqual(3, dim3.Values.Count);
            List<string> expected3 = ["ketjutettu_lv", "vmuutos_lv", "lkm_julk_uudet"];
            CollectionAssert.AreEqual(expected3, dim3.Values.Codes.ToList());
        }

        [TestMethod]
        public void MultiLangMapTestFromRecommendedFixture()
        {
            Dimension? area_dim = Actual_Recommended_3Lang.Dimensions.Find(d => d.Code == "area");
            Assert.IsNotNull(area_dim);
            Assert.AreEqual(DimensionType.Geographical, area_dim.Type);
        }

        [TestMethod]
        public void SingleLangMapTest()
        {
            Dimension? area_dim = Actual_1Lang.Dimensions.Find(d => d.Code == "Alue");
            Assert.IsNotNull(area_dim);
            Assert.AreEqual(DimensionType.Geographical, area_dim.Type);
        }

        [TestMethod]
        public void MultilanguageTableWithCustomMetaPropertiesAndPropetyTypes()
        {
            PxFileSyntaxConf syntaxConf = PxFileSyntaxConf.Default;
            Dictionary<string, MetaPropertyType> customTypeDictionary = new()
            {
                { "TEXTPROPERTY", MetaPropertyType.Text },
                { "MULTILANGUAGETEXTPROPERTY", MetaPropertyType.MultilanguageText },
                { "NUMBERPROPERTY", MetaPropertyType.Numeric },
                { "BOOLEANPROPERTY", MetaPropertyType.Boolean },
                { "BOOLEANTEXTPROPERTY", MetaPropertyType.Text },
                { "TEXTARRAYPROPERTY", MetaPropertyType.TextArray },
                { "MULTILANGUAGETEXTARRAYPROPERTY", MetaPropertyType.MultilanguageTextArray },
                { "SINGLEITEMTEXTARRAYPROPERTY", MetaPropertyType.TextArray },
                { "SINGLEITEMMULTILANGUAGETEXTARRAYPROPERTY", MetaPropertyType.MultilanguageTextArray },
            };
            syntaxConf.Content.EntryTypes.PropertyTypeDefinitions = customTypeDictionary;
            MatrixMetadata actual = new MatrixMetadataBuilder(syntaxConf).Build(PxFileMetaEntries_Robust_3_Languages_With_Custom_Properties.Entries);
            Assert.IsNotNull(actual);
            Assert.AreEqual(MetaPropertyType.Text, actual.AdditionalProperties["TEXTPROPERTY"].Type);
            Assert.AreEqual(MetaPropertyType.MultilanguageText, actual.AdditionalProperties["MULTILANGUAGETEXTPROPERTY"].Type);
            Assert.AreEqual(MetaPropertyType.Numeric, actual.AdditionalProperties["NUMBERPROPERTY"].Type);
            Assert.AreEqual(MetaPropertyType.Boolean, actual.AdditionalProperties["BOOLEANPROPERTY"].Type);
            Assert.AreEqual(MetaPropertyType.Text, actual.AdditionalProperties["BOOLEANTEXTPROPERTY"].Type);
            Assert.AreEqual(MetaPropertyType.TextArray, actual.AdditionalProperties["TEXTARRAYPROPERTY"].Type);
            Assert.AreEqual(MetaPropertyType.MultilanguageTextArray, actual.AdditionalProperties["MULTILANGUAGETEXTARRAYPROPERTY"].Type);
            Assert.AreEqual(MetaPropertyType.TextArray, actual.AdditionalProperties["SINGLEITEMTEXTARRAYPROPERTY"].Type);
            Assert.AreEqual(MetaPropertyType.MultilanguageTextArray, actual.AdditionalProperties["SINGLEITEMMULTILANGUAGETEXTARRAYPROPERTY"].Type);
        }
    }
}
