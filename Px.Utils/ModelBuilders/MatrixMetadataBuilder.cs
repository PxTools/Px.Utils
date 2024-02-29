using Px.Utils.Language;
using PxUtils.Language;
using PxUtils.Models.Metadata;
using PxUtils.Models.Metadata.Dimensions;
using PxUtils.Models.Metadata.Enums;
using PxUtils.PxFile;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace PxUtils.ModelBuilders
{
    public class MatrixMetadataBuilder
    {
        private readonly PxFileSyntaxConf _pxFileSyntaxConf;
        private readonly char _listSeparator;
        private readonly char _stringDelimeter;

        public MatrixMetadataBuilder(PxFileSyntaxConf? pxFileSyntaxConf = null)
        {
            _pxFileSyntaxConf = pxFileSyntaxConf ?? PxFileSyntaxConf.Default;
            _listSeparator = _pxFileSyntaxConf.Symbols.Value.ListSeparator;
            _stringDelimeter = _pxFileSyntaxConf.Symbols.Value.StringDelimeter;
        }

        public MatrixMetadata Build(IEnumerable<KeyValuePair<string, string>> metadataInput)
        {
            MetadataEntryKeyBuilder entryKeyBuilder = new();
            IEnumerable<KeyValuePair<MetadataEntryKey, string>> entryIterator =
                metadataInput.Select(kvp =>
                {
                    var entryKey = entryKeyBuilder.Parse(kvp.Key);
                    return new KeyValuePair<MetadataEntryKey, string>(entryKey, kvp.Value);
                });
            Dictionary<MetadataEntryKey, string> entries = new(entryIterator);

            PxFileLanguages langs = GetLanguages(entries);

            string stubKey = _pxFileSyntaxConf.Tokens.KeyWords.StubDimensions;
            IEnumerable<MultilanguageString> stubDimensionNames = TryGetAndRemoveMultilanguageProperty(entries, stubKey, langs, out Property? maybeStub)
                ? ValueParserUtilities.ParseListOfMultilanguageStrings(maybeStub, _listSeparator, _stringDelimeter)
                : throw new ArgumentException("Stub variable names not found in metadata");

            string headingKey = _pxFileSyntaxConf.Tokens.KeyWords.HeadingDimensions;
            IEnumerable<MultilanguageString> headingDimensionNames = TryGetAndRemoveMultilanguageProperty(entries, headingKey, langs, out Property? maybeHeading)
                ? ValueParserUtilities.ParseListOfMultilanguageStrings(maybeHeading, _listSeparator, _stringDelimeter)
                : throw new ArgumentException("Heading variable names not found in metadata");

            ContentDimension cd = TryGetContentDimension(entries, langs, out ContentDimension? maybeContent)
                ? maybeContent
                : throw new ArgumentException("Content variable not found in metadata");

            IEnumerable<IDimension> dimensions = stubDimensionNames.Concat(headingDimensionNames)
                .Select<MultilanguageString, IDimension>(name =>
                {
                    if (name == cd.Name) return cd;
                    else if(TestIfTimeAndBuild(entries, langs, name, out TimeDimension? timeDim)) return timeDim;
                    else return BuildDimension(entries, langs, name);
                });

            MatrixMetadata meta = new (langs.DefaultLanguage, langs.AvailableLanguages, dimensions.ToList(), []);
            AddAdditionalPropertiesToMatrixMetadata(meta, entries, langs);
            return meta;
        }

        #region Dimension building


        private bool TryGetContentDimension(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, [MaybeNullWhen(false)] out ContentDimension contentDimension)
        {
            string contentKey = _pxFileSyntaxConf.Tokens.KeyWords.ContentVariableIdentifier;
            if(TryGetAndRemoveMultilanguageProperty(entries, contentKey, langs, out Property? contVarName))
            {
                contentDimension = BuildContentDimension(entries, langs, contVarName.GetMultiLanguageString());
                return true;
            }

            contentDimension = null;
            return false;
        }

        private bool TestIfTimeAndBuild(
            Dictionary<MetadataEntryKey, string> entries,
            PxFileLanguages langs,
            MultilanguageString dimensionNameToTest,
            [MaybeNullWhen(false)] out TimeDimension timeDimension)
        {
            string code = GetDimensionCode(entries, langs, dimensionNameToTest);
            IEnumerable<DimensionValue> values = BuildDimensionValues(entries, langs, dimensionNameToTest);
            DimensionValue? maybeDefault = GetDefaultValue(entries, langs, values);
            string timeValIdentifierKey = _pxFileSyntaxConf.Tokens.KeyWords.TimeVal;
            string dimensionTypeKey = _pxFileSyntaxConf.Tokens.KeyWords.DimensionType;
            if(TryGetAndRemoveMultilanguageProperty(entries, timeValIdentifierKey, langs, out Property? timeVal, dimensionNameToTest))
            {
                List<string> strings = ValueParserUtilities.ParseStringList(timeVal.GetString(), _listSeparator, _stringDelimeter);
                TimeDimensionInterval interval = ValueParserUtilities.ParseTimeIntervalFromTimeVal(strings[0], _pxFileSyntaxConf);
                timeDimension = new TimeDimension(code, dimensionNameToTest, [timeVal], values.ToList(), maybeDefault, interval);
                return true;
            }
            else if (TryGetAndRemoveMultilanguageProperty(entries, dimensionTypeKey, langs, out Property? dimTypeContent, dimensionNameToTest) &&
                dimTypeContent.GetString() == _pxFileSyntaxConf.Tokens.VariableTypes.Time)
            {
                timeDimension = new TimeDimension(code, dimensionNameToTest, [], values.ToList(), maybeDefault, TimeDimensionInterval.Irregular);
                return true;
            }

            timeDimension = null;
            return false;
        }

        private Dimension BuildDimension(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimensionName)
        {
            string code = GetDimensionCode(entries, langs, dimensionName);
            IEnumerable<DimensionValue> values = BuildDimensionValues(entries, langs, dimensionName);
            DimensionValue? maybeDefault = GetDefaultValue(entries, langs, values);
            string dimensionTypeKey = _pxFileSyntaxConf.Tokens.KeyWords.DimensionType;

            DimensionType type = TryGetAndRemoveMultilanguageProperty(entries, dimensionTypeKey, langs, out Property? dimTypeContent, dimensionName)
                ? ValueParserUtilities.StringToDimensionType(dimTypeContent.GetString(), _pxFileSyntaxConf)
                : DimensionType.Unknown;

            return new Dimension(code, dimensionName, [], values.ToList(), maybeDefault, type);
        }

        private ContentDimension BuildContentDimension(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimensionName)
        {
            string code = GetDimensionCode(entries, langs, dimensionName);
            List<ContentDimensionValue> values = BuildContentDimensionValues(entries, langs, dimensionName).ToList();
            string defaultValueKey = _pxFileSyntaxConf.Tokens.KeyWords.DimensionDefaultValue;
            if (TryGetAndRemoveMultilanguageProperty(entries, defaultValueKey, langs, out Property? defaultValueName))
            {
                ContentDimensionValue? defaultValue = values.Find(v => v.Name == defaultValueName.GetMultiLanguageString());

                if (defaultValue is null)
                {
                    string defaultName = defaultValueName.GetMultiLanguageString()[langs.DefaultLanguage];
                    string eMsg = $"Default value is defined for the content variable but the variable does not contain a value with name {defaultName}";
                    throw new ArgumentException(eMsg);
                }

                return new ContentDimension(code, dimensionName, [], values, defaultValue);
            }

            return new ContentDimension(code, dimensionName, [], values);
        }

        #endregion

        #region Dimension value building

        private IEnumerable<DimensionValue> BuildDimensionValues(
            Dictionary<MetadataEntryKey, string> entries,
            PxFileLanguages langs,
            MultilanguageString dimensionName)
        {
            string valueNamesKey = _pxFileSyntaxConf.Tokens.KeyWords.VariableValues;
            if (TryGetAndRemoveMultilanguageProperty(entries, valueNamesKey, langs, out Property? valueNames, dimensionName))
            {
                return GetVariableValueIterator(entries, langs, dimensionName, valueNames.GetMultiLanguageString());
            }

            throw new ArgumentException($"Value names not found for dimension {dimensionName[langs.DefaultLanguage]}");
        }

        private IEnumerable<DimensionValue> GetVariableValueIterator(
            Dictionary<MetadataEntryKey, string> entries,
            PxFileLanguages langs,
            MultilanguageString dimensionName,
            MultilanguageString valueNamesBlob)
        {
            Dictionary<string, List<string>> valueNames = langs.AvailableLanguages
                .ToDictionary(l => l, l => ValueParserUtilities.ParseStringList(valueNamesBlob[l], _listSeparator, _stringDelimeter));

            string valueCodesKey = _pxFileSyntaxConf.Tokens.KeyWords.VariableValueCodes;
            List<string> codes = TryGetAndRemoveMultilanguageProperty(entries, valueCodesKey, langs, out Property? codeSet, dimensionName)
                ? ValueParserUtilities.ParseStringList(codeSet.GetString(), _listSeparator, _stringDelimeter)
                : valueNames[langs.DefaultLanguage];

            int numOfCodes = codes.Count;
            for (int index = 0; index < numOfCodes; index++)
            {
                IEnumerable<KeyValuePair<string, string>> langNamePairs = valueNames.Select(langToNameList =>
                new KeyValuePair<string, string>(langToNameList.Key, langToNameList.Value[index]));

                MultilanguageString name = new(langNamePairs);
                yield return new DimensionValue(codes[index], name);
            }
        }

        private IEnumerable<ContentDimensionValue> BuildContentDimensionValues(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimensionName)
        {
            foreach(DimensionValue value in BuildDimensionValues(entries, langs, dimensionName))
            {
                MultilanguageString unit = GetUnit(entries, langs, value.Name);
                DateTime lastUpdated = GetLastUpdated(entries, langs);
                yield return new ContentDimensionValue(value, unit, lastUpdated);
            }
        }

        private string GetDimensionCode(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimensionName)
        {
            string varCodeKey = _pxFileSyntaxConf.Tokens.KeyWords.VariableCode;
            return TryGetAndRemoveMultilanguageProperty(entries, varCodeKey, langs, out Property? codeSet, dimensionName)
            ? codeSet.GetString() : dimensionName[langs.DefaultLanguage];
        }

        private MultilanguageString GetUnit(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString name)
        {
            string unitKey = _pxFileSyntaxConf.Tokens.KeyWords.Units;
            if (TryGetAndRemoveMultilanguageProperty(entries, unitKey, langs, out Property? unit, name) ||
              TryGetAndRemoveMultilanguageProperty(entries, unitKey, langs, out unit))
            {
                return unit.GetMultiLanguageString();
            }

            throw new ArgumentException("Unit information not found");
        }

        private DateTime GetLastUpdated(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs)
        {
            string lastUpdatedKey = _pxFileSyntaxConf.Tokens.KeyWords.LastUpdated;
            if (TryGetAndRemoveMultilanguageProperty(entries, lastUpdatedKey, langs, out Property? lastUpdated))
            {
                string formatString = _pxFileSyntaxConf.Tokens.Time.DateTimeFormatString;
                return DateTime.ParseExact(lastUpdated.GetMultiLanguageString().UniqueValue(), formatString, CultureInfo.InvariantCulture);
            }

            throw new ArgumentException("Last update information not found");
        }

        private Dim? GetDefaultValue<Dim>(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, IEnumerable<Dim> values) where Dim : DimensionValue
        { 
            string defaultValueKey = _pxFileSyntaxConf.Tokens.KeyWords.DimensionDefaultValue;
            if (TryGetAndRemoveMultilanguageProperty(entries, defaultValueKey, langs, out Property? defaultValueName))
            {
                if(values.FirstOrDefault(v => v.Name == defaultValueName.GetMultiLanguageString()) is Dim dimensionValue)
                {
                    return dimensionValue;
                }
                else
                {
                    string defaultName = defaultValueName.GetMultiLanguageString()[langs.DefaultLanguage];
                    string eMsg = $"Default value is defined for the variable but it does not contain a value with name {defaultName}";
                    throw new ArgumentException(eMsg);
                }
            }

            return null;
        }

        #endregion

        #region Table level metadata building

        private PxFileLanguages GetLanguages(Dictionary<MetadataEntryKey, string> entries)
        {
            MetadataEntryKey defaultLangKey = new(_pxFileSyntaxConf.Tokens.KeyWords.DefaultLanguage);
            if (entries.TryGetValue(defaultLangKey, out string? defaultLang))
            {
                entries.Remove(defaultLangKey);
            }
            else
            {
                throw new ArgumentException($"Default language not found in metadata");
            }

            MetadataEntryKey availableLangsKey = new(_pxFileSyntaxConf.Tokens.KeyWords.AvailableLanguages);
            List<string> availableLangs = [defaultLang];
            if (entries.TryGetValue(availableLangsKey, out string? availableLangsString))
            {
                availableLangs = ValueParserUtilities.ParseStringList(availableLangsString, _listSeparator, _stringDelimeter);
                entries.Remove(availableLangsKey);
            }

            return new PxFileLanguages(defaultLang, availableLangs);
        }

        #endregion

        #region Property building

        private bool TryGetAndRemoveMultilanguageProperty(
            Dictionary<MetadataEntryKey, string> entries,
            string propertyName,
            PxFileLanguages langs,
            [MaybeNullWhen(false)] out Property property,
            IReadOnlyMultilanguageString? firstIdentifier = null,
            IReadOnlyMultilanguageString? secondIdentifier = null)
        {
            if (firstIdentifier is not null && firstIdentifier.Languages.Any(l => !langs.AvailableLanguages.Contains(l)))
            {
                throw new ArgumentException("Required language was not found for the variable name");
            }

            string defLang = langs.DefaultLanguage;

            // Default language
            Dictionary<MetadataEntryKey, string> read = [];
            MetadataEntryKey defaultKey = new(propertyName, null, firstIdentifier?[defLang], secondIdentifier?[defLang]);
            MetadataEntryKey defaultKeyWithLang = new(propertyName, defLang, firstIdentifier?[defLang], secondIdentifier?[defLang]);
            if (entries.TryGetValue(defaultKey, out string? defLangValue) || entries.TryGetValue(defaultKeyWithLang, out defLangValue))
            {
                read[defaultKeyWithLang] = defLangValue;
            }
            else
            {
                property = null;
                return false;
            }

            // Other languages
            foreach (string lang in langs.AvailableLanguages.Where(l => l != defLangValue))
            {
                MetadataEntryKey langKey = new(propertyName, lang, firstIdentifier?[lang], secondIdentifier?[defLang]);
                if (entries.TryGetValue(langKey, out string? langValue))
                {
                    read[langKey] = langValue;
                }
                else
                {
                    property = null;
                    return false;
                }
            }

            read.Keys.ToList().ForEach(k => entries.Remove(k));
            property = new(propertyName, new MultilanguageString(read.Select(kvp => 
                new KeyValuePair<string, string>(kvp.Key.Language ?? defLang, kvp.Value))));
            return true;
        }

        #endregion

        private void AddAdditionalPropertiesToMatrixMetadata(MatrixMetadata metadata, Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs)
        {
            while(entries.Count != 0)
            {
                KeyValuePair<MetadataEntryKey, string> kvp = entries.First();
                if (kvp.Key.FirstIdentifier is null)
                {
                    if (TryGetAndRemoveMultilanguageProperty(entries, kvp.Key.KeyWord, langs, out Property? prop))
                    {
                        metadata.AdditionalProperties.Add(prop);
                    }
                    else if (kvp.Key.Language is null)
                    {
                        metadata.AdditionalProperties.Add(new(kvp.Key.KeyWord, kvp.Value));
                        entries.Remove(kvp.Key);
                    }
                    else
                    {
                        string eMsg = $"Failed to build property for key {kvp.Key} because some of the required languages could not be found.";
                        throw new ArgumentException(eMsg);
                    }
                }
                else
                {
                    AddAdditionalPropertiesToVariables(metadata.Dimensions, entries, kvp, langs);
                }
            }   
        }

        private void AddAdditionalPropertiesToVariables(
            IEnumerable<IDimension> dimensions,
            Dictionary<MetadataEntryKey, string> entries,
            KeyValuePair<MetadataEntryKey, string> current,
            PxFileLanguages langs)
        {
            IDimension targetDimension = dimensions.Single(d =>
                        d.Name[current.Key.Language ?? langs.DefaultLanguage] == current.Key.FirstIdentifier);

            if (current.Key.SecondIdentifier is null)
            {
                if (TryGetAndRemoveMultilanguageProperty(entries, current.Key.KeyWord, langs, out Property? prop, targetDimension.Name))
                {
                    targetDimension.AdditionalProperties.Add(prop);
                }
                else if (current.Key.Language is null)
                {
                    targetDimension.AdditionalProperties.Add(new(current.Key.KeyWord, current.Value));
                    entries.Remove(current.Key);
                }
                else
                {
                    string eMsg = $"Failed to build property for key {current.Key} because some of the required languages could not be found.";
                    throw new ArgumentException(eMsg);
                }
            }
            else
            {
                DimensionValue targetValue = targetDimension.Values.Single(v =>
                    v.Name[current.Key.Language ?? langs.DefaultLanguage] == current.Key.SecondIdentifier);

                if (TryGetAndRemoveMultilanguageProperty(entries, current.Key.KeyWord, langs, out Property? prop, targetDimension.Name, targetValue.Name))
                {
                    targetValue.AdditionalProperties.Add(prop);
                }
                else if (current.Key.Language is null)
                {
                    targetValue.AdditionalProperties.Add(new(current.Key.KeyWord, current.Value));
                    entries.Remove(current.Key);
                }
                else
                {
                    string eMsg = $"Failed to build property for key {current.Key} because some of the required languages could not be found.";
                    throw new ArgumentException(eMsg);
                }
            }
        }

    }
}
