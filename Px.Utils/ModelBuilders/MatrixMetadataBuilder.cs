using PxUtils.Language;
using PxUtils.Models.Metadata;
using PxUtils.Models.Metadata.Dimensions;
using PxUtils.Models.Metadata.Enums;
using PxUtils.PxFile;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace PxUtils.ModelBuilders
{
    /// <summary>
    /// Class for building <see cref="MatrixMetadata"/> objects from metadata input.
    /// </summary>
    public class MatrixMetadataBuilder
    {
        private readonly PxFileSyntaxConf _pxFileSyntaxConf;
        private readonly char _listSeparator;
        private readonly char _stringDelimeter;

        /// <summary>
        /// Initializes a new instance of the MatrixMetadataBuilder class.
        /// This constructor takes an optional PxFileSyntaxConf object. If none is provided, it uses the default configuration.
        /// </summary>
        /// <param name="pxFileSyntaxConf">An optional configuration object for Px file syntax. If not provided, the default configuration is used.</param>
        public MatrixMetadataBuilder(PxFileSyntaxConf? pxFileSyntaxConf = null)
        {
            _pxFileSyntaxConf = pxFileSyntaxConf ?? PxFileSyntaxConf.Default;
            _listSeparator = _pxFileSyntaxConf.Symbols.Value.ListSeparator;
            _stringDelimeter = _pxFileSyntaxConf.Symbols.Value.StringDelimeter;
        }

        /// <summary>
        /// Builds a MatrixMetadata object from a given set of metadata entries.
        /// </summary>
        /// <param name="metadataInput">An IEnumerable of key-value pairs representing the metadata entries in the Px-File format.</param>
        /// <returns>A MatrixMetadata object constructed from the input metadata.</returns>
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
            IEnumerable<MultilanguageString> stubDimensionNames = TryGetAndRemoveProperty(entries, stubKey, langs, out Property? maybeStub)
                ? ValueParserUtilities.ParseListOfMultilanguageStrings(maybeStub, langs.DefaultLanguage, _listSeparator, _stringDelimeter)
                : throw new ArgumentException("Stub variable names not found in metadata");

            string headingKey = _pxFileSyntaxConf.Tokens.KeyWords.HeadingDimensions;
            IEnumerable<MultilanguageString> headingDimensionNames = TryGetAndRemoveProperty(entries, headingKey, langs, out Property? maybeHeading)
                ? ValueParserUtilities.ParseListOfMultilanguageStrings(maybeHeading, langs.DefaultLanguage, _listSeparator, _stringDelimeter)
                : throw new ArgumentException("Heading variable names not found in metadata");

            ContentDimension? maybeCd = GetContentDimensionIfAvailable(entries, langs);

            IEnumerable<IDimension> dimensions = stubDimensionNames.Concat(headingDimensionNames)
                .Select<MultilanguageString, IDimension>(name =>
                {
                    if (maybeCd is not null && name.Equals(maybeCd.Name)) return maybeCd;
                    else if(TestIfTimeAndBuild(entries, langs, name, out TimeDimension? timeDim)) return timeDim;
                    else return BuildDimension(entries, langs, name);
                });

            MatrixMetadata meta = new (langs.DefaultLanguage, langs.AvailableLanguages, dimensions.ToList(), []);
            AddAdditionalPropertiesToMatrixMetadata(meta, entries, langs);
            return meta;
        }

        #region Dimension building


        private ContentDimension? GetContentDimensionIfAvailable(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs)
        {
            string contentKey = _pxFileSyntaxConf.Tokens.KeyWords.ContentVariableIdentifier;
            if(TryGetAndRemoveProperty(entries, contentKey, langs, out Property? contVarNameProp))
            {
                MultilanguageString name = contVarNameProp
                    .ForceToMultilanguageString(langs.DefaultLanguage)
                    .CopyAndEditAll(s => s.Trim(_stringDelimeter));
                return BuildContentDimension(entries, langs, name);
            }

            return null;
        }

        private bool TestIfTimeAndBuild(
            Dictionary<MetadataEntryKey, string> entries,
            PxFileLanguages langs,
            MultilanguageString dimensionNameToTest,
            [MaybeNullWhen(false)] out TimeDimension timeDimension)
        {
            string timeValIdentifierKey = _pxFileSyntaxConf.Tokens.KeyWords.TimeVal;
            string dimensionTypeKey = _pxFileSyntaxConf.Tokens.KeyWords.DimensionType;
            if(TryGetAndRemoveProperty(entries, timeValIdentifierKey, langs, out Property? timeVal, dimensionNameToTest))
            {
                string code = GetDimensionCode(entries, langs, dimensionNameToTest);
                DimensionValue[] values = BuildDimensionValues(entries, langs, dimensionNameToTest);
                DimensionValue? maybeDefault = GetDefaultValue(entries, langs, dimensionNameToTest, values);
                TimeDimensionInterval interval = ValueParserUtilities.ParseTimeIntervalFromTimeVal(timeVal.GetString(), _pxFileSyntaxConf);
                Dictionary<string, Property> additionalProperties = new() { { timeVal.KeyWord, timeVal } };
                timeDimension = new TimeDimension(code, dimensionNameToTest, additionalProperties, [.. values], maybeDefault, interval);
                return true;
            }
            else if (TryGetProperty(entries, dimensionTypeKey, langs, out Property? dimType, dimensionNameToTest) &&
                ValueParserUtilities.StringToDimensionType(dimType.GetString()) == DimensionType.Time)
            {
                List<MetadataEntryKey> keys = BuildKeys(entries, dimensionTypeKey, langs, dimensionNameToTest);
                keys.ForEach(k => entries.Remove(k));

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
            DimensionType type = TryGetAndRemoveProperty(entries, dimensionTypeKey, langs, out Property? dimTypeContent, dimensionName)
                ? ValueParserUtilities.StringToDimensionType(dimTypeContent.GetString(), _pxFileSyntaxConf)
                : DimensionType.Unknown;

            return new Dimension(code, dimensionName, [], [.. values], maybeDefault, type);
        }

        private ContentDimension BuildContentDimension(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimensionName)
        {
            string code = GetDimensionCode(entries, langs, dimensionName);
            List<ContentDimensionValue> values = BuildContentDimensionValues(entries, langs, dimensionName).ToList();
            string defaultValueKey = _pxFileSyntaxConf.Tokens.KeyWords.DimensionDefaultValue;
            if (TryGetAndRemoveProperty(entries, defaultValueKey, langs, out Property? defaultValueProperty))
            {
                MultilanguageString defaultValueName = defaultValueProperty.ForceToMultilanguageString(langs.DefaultLanguage);
                ContentDimensionValue? defaultValue = values.Find(v => v.Name.Equals(defaultValueName));

                if (defaultValue is null)
                {
                    string defaultName = defaultValueName[langs.DefaultLanguage];
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
            if (TryGetAndRemoveProperty(entries, current.Key.KeyWord, pxLangs, out Property? prop, targetDimension.Name))
            {
                targetDimension.AdditionalProperties[prop.KeyWord] = prop;
            }
            else
            {
                string eMsg = $"Failed to build property for key {current.Key}, either some required metadata is missing or contains errors.";
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
            if (TryGetAndRemoveProperty(entries, valueNamesKey, langs, out Property? valueNames, dimensionName))
            {
                return GetVariableValueIterator(entries, langs, dimensionName, valueNames);
            }

            throw new ArgumentException($"Value names not found for dimension {dimensionName[langs.DefaultLanguage]}");
        }

        private DimensionValue[] GetVariableValueIterator(
            Dictionary<MetadataEntryKey, string> entries,
            PxFileLanguages langs,
            MultilanguageString dimensionName,
            Property valueNamesProperty)
        {
            Dictionary<string, List<string>> valueNames = langs.AvailableLanguages.ToDictionary(
                    lang => lang,
                    lang => ValueParserUtilities.ParseStringList(valueNamesProperty.ForceToMultilanguageString(langs.DefaultLanguage)[lang],
                    _listSeparator, _stringDelimeter));

            string valueCodesKey = _pxFileSyntaxConf.Tokens.KeyWords.VariableValueCodes;
            List<string> codes = TryGetAndRemoveProperty(entries, valueCodesKey, langs, out Property? codeSet, dimensionName)
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
                MultilanguageString unit = GetUnit(entries, langs, dimensionName, value.Name);
                DateTime lastUpdated = GetLastUpdated(entries, langs, dimensionName, value.Name);
                int precision = GetPrecision(entries, langs, dimensionName, value.Name);
                yield return new ContentDimensionValue(value, unit, lastUpdated, precision);
            }
        }

        private static void AddPropertyToDimensionValue(
            Dictionary<MetadataEntryKey, string> entries,
            KeyValuePair<MetadataEntryKey, string> current,
            MultilanguageString targetDimensionName,
            DimensionValue targetValue,
            PxFileLanguages pxLangs)
        {
            if (TryGetAndRemoveProperty(entries, current.Key.KeyWord, pxLangs, out Property? prop, targetDimensionName, targetValue.Name))
            {
                targetValue.AdditionalProperties[prop.KeyWord] = prop;
            }
            else
            {
                string eMsg = $"Failed to build property for key {current.Key}, either some required metadata is missing or contains errors.";
                throw new ArgumentException(eMsg);
            }
        }

        private static void AddPropertyToContentDimensionValue(
            Dictionary<MetadataEntryKey, string> entries,
            KeyValuePair<MetadataEntryKey, string> current,
            DimensionValue targetValue,
            PxFileLanguages pxLangs)
        {
            if (TryGetAndRemoveProperty(entries, current.Key.KeyWord, pxLangs, out Property? prop, targetValue.Name))
            {
                targetValue.AdditionalProperties[prop.KeyWord] = prop;
            }
            else
            {
                string eMsg = $"Failed to build property for key {current.Key}, either some required metadata is missing or contains errors.";
                throw new ArgumentException(eMsg);
            }
        }

        private string GetDimensionCode(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimensionName)
        {
            string varCodeKey = _pxFileSyntaxConf.Tokens.KeyWords.VariableCode;
            return TryGetAndRemoveProperty(entries, varCodeKey, langs, out Property? codeSet, dimensionName)
            ? codeSet.GetString() : dimensionName[langs.DefaultLanguage];
        }

        private MultilanguageString GetUnit(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimName, MultilanguageString valName)
        {
            string unitKey = _pxFileSyntaxConf.Tokens.KeyWords.Units;
            if (TryGetAndRemoveProperty(entries, unitKey, langs, out Property? unit, dimName, valName) || // Both identifiers
                TryGetAndRemoveProperty(entries, unitKey, langs, out unit, valName) // Only value identifier
                ) return unit.ForceToMultilanguageString(langs.DefaultLanguage);

            throw new ArgumentException("Unit information not found");
        }

        private DateTime GetLastUpdated(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimName, MultilanguageString valName)
        {
            string lastUpdatedKey = _pxFileSyntaxConf.Tokens.KeyWords.LastUpdated;
            if (TryGetAndRemoveProperty(entries, lastUpdatedKey, langs, out Property? lastUpdated, dimName, valName) || // Both identifiers
                TryGetAndRemoveProperty(entries, lastUpdatedKey, langs, out lastUpdated, valName)) // Only value identifier
            {
                string formatString = _pxFileSyntaxConf.Tokens.Time.DateTimeFormatString;
                return DateTime.ParseExact(lastUpdated.GetString(), formatString, CultureInfo.InvariantCulture);
            }

            throw new ArgumentException("Last update information not found");
        }

        private int GetPrecision(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimName, MultilanguageString valName)
        {
            string precisionKey = _pxFileSyntaxConf.Tokens.KeyWords.Precision;
            if (TryGetAndRemoveProperty(entries, precisionKey, langs, out Property? precision, dimName, valName) || // Both identifiers
               TryGetAndRemoveProperty(entries, precisionKey, langs, out precision, valName)) // Only value identifier
            {
                return int.Parse(precision.GetString());
            }

            return 0; // Default value
        }

        private Dim? GetDefaultValue<Dim>(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimensionName, Dim[] values) where Dim : DimensionValue
        { 
            string defaultValueKey = _pxFileSyntaxConf.Tokens.KeyWords.DimensionDefaultValue;
            if (TryGetAndRemoveProperty(entries, defaultValueKey, langs, out Property? defaultValueName, dimensionName))
            {
                MultilanguageString name = defaultValueName.ForceToMultilanguageString(langs.DefaultLanguage);
                if (Array.Find(values, v => v.Name.Equals(name)) is Dim dimensionValue)
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
                    if (TryGetAndRemoveProperty(entries, keyWord, langs, out Property? prop))
                    {
                        metadata.AdditionalProperties[keyWord] = prop;
                    }
                    else
                    {
                        string eMsg = $"Failed to build property for key {keyWord}, either some required metadata is missing or contains errors.";
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

        private static bool TryGetProperty(
            Dictionary<MetadataEntryKey, string> entries,
            string propertyName,
            PxFileLanguages langs,
            [MaybeNullWhen(false)] out Property property,
            MultilanguageString? firstIdentifier = null,
            MultilanguageString? secondIdentifier = null)
        {
            List<MetadataEntryKey> keys = BuildKeys(entries, propertyName, langs, firstIdentifier, secondIdentifier);
            if (keys.Count == 0)
            {
                property = null;
                return false;
            }
            else
            {
                property = BuildProperty(entries, keys, langs);
                return true;
            }
        }

        private static bool TryGetAndRemoveProperty(
            Dictionary<MetadataEntryKey, string> entries,
            string propertyName,
            PxFileLanguages langs,
            [MaybeNullWhen(false)] out Property property,
            MultilanguageString? firstIdentifier = null,
            MultilanguageString? secondIdentifier = null)
        {
            List<MetadataEntryKey> keys = BuildKeys(entries, propertyName, langs, firstIdentifier, secondIdentifier);
            if (keys.Count == 0)
            {
                property = null;
                return false;
            }
            else
            {
                property = BuildProperty(entries, keys, langs);
                keys.ForEach(k => entries.Remove(k));
                return true;
            }
        }

        private static Property BuildProperty(Dictionary<MetadataEntryKey, string> entries, List<MetadataEntryKey> keys, PxFileLanguages langs)
        {
            if (keys.Count == 0) throw new ArgumentException("No metadta entry keys provided for building property");

            Dictionary<MetadataEntryKey, string> read = [];
            foreach (MetadataEntryKey key in keys)
            {
                if (entries.TryGetValue(key, out string? langValue)) read[key] = langValue;
                else throw new ArgumentException($"Can not build property from provided key: {key.KeyWord}. Key not found in metadata.");
            }

            if(keys.Count == 1) return new Property(keys[0].KeyWord, read[keys[0]]);
            else return new(keys[0].KeyWord, new MultilanguageString(read.Select(kvp => 
                new KeyValuePair<string, string>(kvp.Key.Language ?? langs.DefaultLanguage, kvp.Value))));
        }

        private static List<MetadataEntryKey> BuildKeys(
            Dictionary<MetadataEntryKey, string> entries,
            string propertyName,
            PxFileLanguages langs,
            MultilanguageString? firstIdentifier = null,
            MultilanguageString? secondIdentifier = null)
        {
            if (firstIdentifier is not null && firstIdentifier.Languages.Any(l => !langs.AvailableLanguages.Contains(l)))
            {
                throw new ArgumentException("Required language was not found for the variable name");
            }

            string defLang = langs.DefaultLanguage;

            // Default language
            List<MetadataEntryKey> keys = [];
            MetadataEntryKey defaultKey = new(propertyName, null, firstIdentifier?[defLang], secondIdentifier?[defLang]);
            MetadataEntryKey defaultKeyWithLang = new(propertyName, defLang, firstIdentifier?[defLang], secondIdentifier?[defLang]);
            if (entries.ContainsKey(defaultKey)) keys.Add(defaultKey);
            else if (entries.ContainsKey(defaultKeyWithLang)) keys.Add(defaultKeyWithLang);
            else return keys;

            // Other languages
            List<MetadataEntryKey> otherLanguageKeys = [];
            foreach (string lang in langs.AvailableLanguages.Where(l => l != defLang))
            {
                MetadataEntryKey langKey = new(propertyName, lang, firstIdentifier?[lang], secondIdentifier?[lang]);
                if (entries.ContainsKey(langKey)) otherLanguageKeys.Add(langKey);
                else return keys;
            }
            
            keys.AddRange(otherLanguageKeys);
            return keys;
        }

        #endregion
    }
}
