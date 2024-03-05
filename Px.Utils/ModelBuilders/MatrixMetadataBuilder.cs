﻿using Px.Utils.Language;
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
                MultilanguageString name = contVarName.GetMultiLanguageString().CopyAndEditAll(s => s.Trim(_stringDelimeter));
                contentDimension = BuildContentDimension(entries, langs, name);
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
            string timeValIdentifierKey = _pxFileSyntaxConf.Tokens.KeyWords.TimeVal;
            string dimensionTypeKey = _pxFileSyntaxConf.Tokens.KeyWords.DimensionType;
            if(TryGetAndRemoveMultilanguageProperty(entries, timeValIdentifierKey, langs, out Property? timeVal, dimensionNameToTest))
            {
                string code = GetDimensionCode(entries, langs, dimensionNameToTest);
                DimensionValue[] values = BuildDimensionValues(entries, langs, dimensionNameToTest);
                DimensionValue? maybeDefault = GetDefaultValue(entries, langs, dimensionNameToTest, values);
                TimeDimensionInterval interval = ValueParserUtilities.ParseTimeIntervalFromTimeVal(timeVal.GetString(), _pxFileSyntaxConf);
                Dictionary<string, Property> additionalProperties = new() { { timeVal.KeyWord, timeVal } };
                timeDimension = new TimeDimension(code, dimensionNameToTest, additionalProperties, [.. values], maybeDefault, interval);
                return true;
            }
            else if (TryGetAndRemoveMultilanguageProperty(entries, dimensionTypeKey, langs, out Property? dimTypeContent, dimensionNameToTest) &&
                dimTypeContent.GetString() == _pxFileSyntaxConf.Tokens.VariableTypes.Time)
            {
                string code = GetDimensionCode(entries, langs, dimensionNameToTest);
                DimensionValue[] values = BuildDimensionValues(entries, langs, dimensionNameToTest);
                DimensionValue? maybeDefault = GetDefaultValue(entries, langs, dimensionNameToTest, values);
                timeDimension = new TimeDimension(code, dimensionNameToTest, [], [.. values], maybeDefault, TimeDimensionInterval.Irregular);
                return true;
            }

            timeDimension = null;
            return false;
        }

        private Dimension BuildDimension(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimensionName)
        {
            string code = GetDimensionCode(entries, langs, dimensionName);
            DimensionValue[] values = BuildDimensionValues(entries, langs, dimensionName);
            DimensionValue? maybeDefault = GetDefaultValue(entries, langs, dimensionName, values);
            string dimensionTypeKey = _pxFileSyntaxConf.Tokens.KeyWords.DimensionType;

            DimensionType type = TryGetAndRemoveMultilanguageProperty(entries, dimensionTypeKey, langs, out Property? dimTypeContent, dimensionName)
                ? ValueParserUtilities.StringToDimensionType(dimTypeContent.GetString(), _pxFileSyntaxConf)
                : DimensionType.Unknown;

            return new Dimension(code, dimensionName, [], [.. values], maybeDefault, type);
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

        private static void AddAdditionalPropertiesToDimensions(
            IEnumerable<IDimension> dimensions,
            Dictionary<MetadataEntryKey, string> entries,
            KeyValuePair<MetadataEntryKey, string> current,
            PxFileLanguages pxLangs)
        {
            string firstIdentifier = current.Key.FirstIdentifier ??
                throw new ArgumentException($"First identifier is required in the key {current.Key.KeyWord} for setting variable properties");

            IDimension? targetDimension = dimensions.FirstOrDefault(d =>
                        d.Name[current.Key.Language ?? pxLangs.DefaultLanguage] == firstIdentifier);

            if (current.Key.SecondIdentifier is null)
            {
                if (targetDimension is not null) AddAdditionalPropertyToDimension(entries, current, targetDimension, pxLangs);
                else if (HasContentDimensionWithValue(dimensions, firstIdentifier, out DimensionValue? value))
                {
                    AddPropertyToContentDimensionValue(entries, current, value, pxLangs);
                }
                else
                {
                    string eMsg = $"Failed to build property for key {current.Key} because the variable with name {firstIdentifier} was not found.";
                    throw new ArgumentException(eMsg);
                }
            }
            else
            {
                DimensionValue? value = targetDimension?.Values.FirstOrDefault(v => v.Name[current.Key.Language ?? pxLangs.DefaultLanguage] == current.Key.SecondIdentifier) ??
                    throw new ArgumentException($"Failed to build property for key {current.Key} because the value with name {current.Key.SecondIdentifier} was not found.");
                AddPropertyToDimensionValue(entries, current, targetDimension.Name, value, pxLangs);
            }
        }

        private static void AddAdditionalPropertyToDimension(
            Dictionary<MetadataEntryKey, string> entries,
            KeyValuePair<MetadataEntryKey, string> current,
            IDimension targetDimension,
            PxFileLanguages pxLangs)
        {
            if (TryGetAndRemoveMultilanguageProperty(entries, current.Key.KeyWord, pxLangs, out Property? prop, targetDimension.Name))
            {
                targetDimension.AdditionalProperties[prop.KeyWord] = prop;
            }
            else if (current.Key.Language is null)
            {
                string keyWord = current.Key.KeyWord;
                targetDimension.AdditionalProperties[keyWord] = new(keyWord, current.Value);
                entries.Remove(current.Key);
            }
            else
            {
                string eMsg = $"Failed to build property for key {current.Key} because some of the required languages could not be found.";
                throw new ArgumentException(eMsg);
            }
        }

        #endregion

        #region Dimension value building

        private DimensionValue[] BuildDimensionValues(
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

        private DimensionValue[] GetVariableValueIterator(
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
            DimensionValue[] values = new DimensionValue[numOfCodes];

            for (int index = 0; index < numOfCodes; index++)
            {
                List<KeyValuePair<string, string>> langNamePairs = [];
                foreach (var langToNameList in valueNames)
                {
                    try
                    {
                        langNamePairs.Add(new KeyValuePair<string, string>(langToNameList.Key, langToNameList.Value[index]));
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        string eMsg = $"Number of value names and value codes do not match for dimension {dimensionName[langs.DefaultLanguage]}";
                        throw new ArgumentException(eMsg);
                    }
                }

                MultilanguageString name = new(langNamePairs);
                values[index] = new DimensionValue(codes[index], name);
            }

            return values;
        }

        private IEnumerable<ContentDimensionValue> BuildContentDimensionValues(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimensionName)
        {
            foreach(DimensionValue value in BuildDimensionValues(entries, langs, dimensionName))
            {
                MultilanguageString unit = GetUnit(entries, langs, value.Name);
                DateTime lastUpdated = GetLastUpdated(entries, langs, value.Name);
                yield return new ContentDimensionValue(value, unit, lastUpdated);
            }
        }

        private static void AddPropertyToDimensionValue(
            Dictionary<MetadataEntryKey, string> entries,
            KeyValuePair<MetadataEntryKey, string> current,
            MultilanguageString targetDimensionName,
            DimensionValue targetValue,
            PxFileLanguages pxLangs)
        {
            if (TryGetAndRemoveMultilanguageProperty(entries, current.Key.KeyWord, pxLangs, out Property? prop, targetDimensionName, targetValue.Name))
            {
                targetValue.AdditionalProperties[prop.KeyWord] = prop;
            }
            else if (current.Key.Language is null)
            {
                string keyWord = current.Key.KeyWord;
                targetValue.AdditionalProperties[keyWord] = new(current.Key.KeyWord, current.Value);
                entries.Remove(current.Key);
            }
            else
            {
                string eMsg = $"Failed to build property for key {current.Key} because some of the required languages could not be found.";
                throw new ArgumentException(eMsg);
            }
        }

        private static void AddPropertyToContentDimensionValue(
            Dictionary<MetadataEntryKey, string> entries,
            KeyValuePair<MetadataEntryKey, string> current,
            DimensionValue targetValue,
            PxFileLanguages pxLangs)
        {
            if (TryGetAndRemoveMultilanguageProperty(entries, current.Key.KeyWord, pxLangs, out Property? prop, targetValue.Name))
            {
                targetValue.AdditionalProperties[prop.KeyWord] = prop;
            }
            else if (current.Key.Language is null)
            {
                string keyWord = current.Key.KeyWord;
                targetValue.AdditionalProperties[keyWord] = new(current.Key.KeyWord, current.Value);
                entries.Remove(current.Key);
            }
            else
            {
                string eMsg = $"Failed to build property for content key {current.Key} because some of the required languages could not be found.";
                throw new ArgumentException(eMsg);
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

        private DateTime GetLastUpdated(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString valueName)
        {
            string lastUpdatedKey = _pxFileSyntaxConf.Tokens.KeyWords.LastUpdated;
            if (TryGetAndRemoveMultilanguageProperty(entries, lastUpdatedKey, langs, out Property? lastUpdated, valueName))
            {
                string formatString = _pxFileSyntaxConf.Tokens.Time.DateTimeFormatString;
                return DateTime.ParseExact(lastUpdated.GetMultiLanguageString().UniqueValue(), formatString, CultureInfo.InvariantCulture);
            }

            throw new ArgumentException("Last update information not found");
        }

        private Dim? GetDefaultValue<Dim>(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimensionName, Dim[] values) where Dim : DimensionValue
        { 
            string defaultValueKey = _pxFileSyntaxConf.Tokens.KeyWords.DimensionDefaultValue;
            if (TryGetAndRemoveMultilanguageProperty(entries, defaultValueKey, langs, out Property? defaultValueName, dimensionName))
            {
                if (Array.Find(values, v => v.Name == defaultValueName.GetMultiLanguageString()) is Dim dimensionValue)
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

        private static void AddAdditionalPropertiesToMatrixMetadata(MatrixMetadata metadata, Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs)
        {
            while (entries.Count != 0)
            {
                KeyValuePair<MetadataEntryKey, string> kvp = entries.First();
                if (kvp.Key.FirstIdentifier is null)
                {
                    string keyWord = kvp.Key.KeyWord;
                    if (TryGetAndRemoveMultilanguageProperty(entries, keyWord, langs, out Property? prop))
                    {
                        metadata.AdditionalProperties[keyWord] = prop;
                    }
                    else if (kvp.Key.Language is null)
                    {
                        metadata.AdditionalProperties[keyWord] = new(kvp.Key.KeyWord, kvp.Value);
                        entries.Remove(kvp.Key);
                    }
                    else
                    {
                        string eMsg = $"Failed to build property for key {kvp.Key.KeyWord} because some of the required languages could not be found.";
                        throw new ArgumentException(eMsg);
                    }
                }
                else
                {
                    AddAdditionalPropertiesToDimensions(metadata.Dimensions, entries, kvp, langs);
                }
            }
        }

        private static bool HasContentDimensionWithValue(IEnumerable<IDimension> dimensions, string name, [MaybeNullWhen(false)] out DimensionValue value)
        {
            IDimension? contentDimension = dimensions.FirstOrDefault(d => d.Type == DimensionType.Content);
            if (contentDimension is not null)
            {
                value = contentDimension.Values.FirstOrDefault(v => v.Name.Languages.Any(l => v.Name[l] == name));
                return value is not null;
            }
            else
            {
                value = null;
                return false;
            }
        }

        #endregion

        #region Property building

        private static bool TryGetAndRemoveMultilanguageProperty(
            Dictionary<MetadataEntryKey, string> entries,
            string propertyName,
            PxFileLanguages langs,
            [MaybeNullWhen(false)] out Property property,
            MultilanguageString? firstIdentifier = null,
            MultilanguageString? secondIdentifier = null)
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
            if (entries.TryGetValue(defaultKey, out string? defLangValue)) read[defaultKey] = defLangValue;
            else if ( entries.TryGetValue(defaultKeyWithLang, out defLangValue)) read[defaultKeyWithLang] = defLangValue;
            else
            {
                property = null;
                return false;
            }

            // Other languages
            foreach (string lang in langs.AvailableLanguages.Where(l => l != defLang))
            {
                MetadataEntryKey langKey = new(propertyName, lang, firstIdentifier?[lang], secondIdentifier?[lang]);
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
    }
}
