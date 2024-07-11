using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Language;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Models.Metadata.ExtensionMethods;
using Px.Utils.PxFile;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Px.Utils.ModelBuilders
{
    /// <summary>
    /// Class for building <see cref="MatrixMetadata"/> objects from metadata input.
    /// </summary>
    public class MatrixMetadataBuilder : IMatrixMetadataBuilder
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
        /// Builds a <see cref="MatrixMetadata"/> object from a given set of metadata entries.
        /// </summary>
        /// <param name="metadataInput">An IEnumerable of key-value pairs representing the metadata entries in the Px-File format.</param>
        /// <returns>A MatrixMetadata object constructed from the input metadata.</returns>
        public MatrixMetadata Build(IEnumerable<KeyValuePair<string, string>> metadataInput)
        {
            MetadataEntryKeyBuilder entryKeyBuilder = new();
            Dictionary<MetadataEntryKey, string> entries = [];
            foreach (KeyValuePair<string, string> entry in metadataInput)
            {
                MetadataEntryKey entryKey = entryKeyBuilder.Parse(entry.Key);
                entries[entryKey] = entry.Value;
            }
            return BuildFromentries(entries);
        }
 
        /// <summary>
        /// Builds a <see cref="MatrixMetadata"/> object from a given set of metadata entries.
        /// </summary>
        /// <param name="metadataInput">A <see cref="IReadOnlyDictionary{string, string}"/> of key-value pairs representing the metadata entries in the Px-File format.</param>
        /// <returns>A <see cref="MatrixMetadata"/> object constructed from the input metadata entries.</returns>
        public MatrixMetadata Build(IReadOnlyDictionary<string, string> metadataInput)
        {
            MetadataEntryKeyBuilder entryKeyBuilder = new();
            Dictionary<MetadataEntryKey, string> entries = [];
            foreach (KeyValuePair<string, string> entry in metadataInput)
            {
                MetadataEntryKey entryKey = entryKeyBuilder.Parse(entry.Key);
                entries[entryKey] = entry.Value;
            }
            return BuildFromentries(entries);
        }

        /// <summary>
        /// Builds a <see cref="MatrixMetadata"/> object from a given set of metadata entries.
        /// </summary>
        /// <param name="metadataInput">An IEnumerable of key-value pairs representing the metadata entries in the Px-File format.</param>
        /// <returns>A MatrixMetadata object constructed from the input metadata.</returns>
        public async Task<MatrixMetadata> BuildAsync(IAsyncEnumerable<KeyValuePair<string, string>> metadataInput)
        {
            MetadataEntryKeyBuilder entryKeyBuilder = new();
            Dictionary<MetadataEntryKey, string> entries = [];
            await foreach (KeyValuePair<string, string> entry in metadataInput)
            {
                MetadataEntryKey entryKey = entryKeyBuilder.Parse(entry.Key);
                entries[entryKey] = entry.Value;
            }
            return BuildFromentries(entries);
        }

        /// <summary>
        /// The metadata is constructed using a dictionary because the order they are required doesn't necessarily match the order thay are in the file.
        /// </summary>
        private MatrixMetadata BuildFromentries(Dictionary<MetadataEntryKey, string> entries)
        {
            PxFileLanguages langs = GetLanguages(entries);

            string stubKey = _pxFileSyntaxConf.Tokens.KeyWords.StubDimensions;
            IEnumerable<MultilanguageString> stubDimensionNames = TryGetAndRemoveProperty(entries, stubKey, langs, out MetaProperty? maybeStub)
                ? maybeStub.ValueAsListOfMultilanguageStrings(langs.DefaultLanguage, _listSeparator, _stringDelimeter)
                : throw new ArgumentException("Stub variable names not found in metadata");

            string headingKey = _pxFileSyntaxConf.Tokens.KeyWords.HeadingDimensions;
            IEnumerable<MultilanguageString> headingDimensionNames = TryGetAndRemoveProperty(entries, headingKey, langs, out MetaProperty? maybeHeading)
                ? maybeHeading.ValueAsListOfMultilanguageStrings(langs.DefaultLanguage, _listSeparator, _stringDelimeter)
                : throw new ArgumentException("Heading variable names not found in metadata");

            ContentDimension? maybeCd = GetContentDimensionIfAvailable(entries, langs);

            IEnumerable<Dimension> dimensions = stubDimensionNames.Concat(headingDimensionNames)
                .Select(name =>
                {
                    if (maybeCd is not null && name.Equals(maybeCd.Name)) return maybeCd;
                    else if (TestIfTimeAndBuild(entries, langs, name, out TimeDimension? timeDim)) return timeDim;
                    else return BuildDimension(entries, langs, name);
                });

            MatrixMetadata meta = new(langs.DefaultLanguage, langs.AvailableLanguages, dimensions.ToList(), []);
            AddAdditionalPropertiesToMatrixMetadata(meta, entries, langs);
            return meta; 
        }

        #region Dimension building


        private ContentDimension? GetContentDimensionIfAvailable(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs)
        {
            string contentKey = _pxFileSyntaxConf.Tokens.KeyWords.ContentVariableIdentifier;
            if (TryGetAndRemoveProperty(entries, contentKey, langs, out MetaProperty? contVarNameProp))
            {
                MultilanguageString name = contVarNameProp.ValueAsMultilanguageString(_stringDelimeter, langs.DefaultLanguage);
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
            if (TryGetAndRemoveProperty(entries, timeValIdentifierKey, langs, out MetaProperty? timeVal, dimensionNameToTest))
            {
                string code = GetDimensionCode(entries, langs, dimensionNameToTest);
                ValueList values = GetDimensionValues(entries, langs, dimensionNameToTest);
                TimeDimensionInterval interval = ValueParserUtilities.ParseTimeIntervalFromTimeVal(timeVal.GetRawValueString(), _pxFileSyntaxConf);
                Dictionary<string, MetaProperty> additionalProperties = new() { { timeVal.KeyWord, timeVal } };
                timeDimension = new TimeDimension(code, dimensionNameToTest, additionalProperties, values, interval);
                return true;
            }
            else if (TryGetProperty(entries, dimensionTypeKey, langs, out MetaProperty? dimType, dimensionNameToTest) &&
                ValueParserUtilities.StringToDimensionType(dimType.ValueAsString(_stringDelimeter)) == DimensionType.Time)
            {
                List<MetadataEntryKey> keys = BuildKeys(entries, dimensionTypeKey, langs, dimensionNameToTest);
                keys.ForEach(k => entries.Remove(k));

                string code = GetDimensionCode(entries, langs, dimensionNameToTest);
                ValueList values = GetDimensionValues(entries, langs, dimensionNameToTest);
                timeDimension = new TimeDimension(code, dimensionNameToTest, [], values, TimeDimensionInterval.Irregular);
                return true;
            }

            timeDimension = null;
            return false;
        }

        private Dimension BuildDimension(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimensionName)
        {
            string code = GetDimensionCode(entries, langs, dimensionName);
            ValueList values = GetDimensionValues(entries, langs, dimensionName);

            DimensionType type = GetDimensionType(entries, langs, dimensionName);
            return new Dimension(code, dimensionName, [], values, type);
        }

        private DimensionType GetDimensionType(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimensionName)
        {
            string dimensionTypeKey = _pxFileSyntaxConf.Tokens.KeyWords.DimensionType;
            DimensionType type = TryGetAndRemoveProperty(entries, dimensionTypeKey, langs, out MetaProperty? dimTypeContent, dimensionName)
                ? ValueParserUtilities.StringToDimensionType(dimTypeContent.GetRawValueString(), _pxFileSyntaxConf)
                : DimensionType.Unknown;

            if (type is DimensionType.Other or DimensionType.Unknown)
            {
                string mapKey = _pxFileSyntaxConf.Tokens.KeyWords.Map;
                if (TryGetProperty(entries, mapKey, langs, out MetaProperty? _, dimensionName))
                {
                    return DimensionType.Geographical;
                }
            }
            return type;
        }

        private ContentDimension BuildContentDimension(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimensionName)
        {
            string code = GetDimensionCode(entries, langs, dimensionName);
            ContentValueList values = BuildContentDimensionValues(entries, langs, dimensionName);

            return new ContentDimension(code, dimensionName, [], values);
        }

        private static void AddAdditionalPropertiesToDimensions(
            IEnumerable<Dimension> dimensions,
            Dictionary<MetadataEntryKey, string> entries,
            KeyValuePair<MetadataEntryKey, string> current,
            PxFileLanguages pxLangs)
        {
            string firstIdentifier = current.Key.FirstIdentifier ??
                throw new ArgumentException($"First identifier is required in the key {current.Key.KeyWord} for setting variable properties");

            Dimension? targetDimension = dimensions.FirstOrDefault(d =>
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
                DimensionValue? value = targetDimension?.Values.Find<DimensionValue>(v => v.Name[current.Key.Language ?? pxLangs.DefaultLanguage] == current.Key.SecondIdentifier) ??
                    throw new ArgumentException($"Failed to build property for key {current.Key} because the value with name {current.Key.SecondIdentifier} was not found.");
                AddPropertyToDimensionValue(entries, current, targetDimension.Name, value, pxLangs);
            }
        }

        private static void AddAdditionalPropertyToDimension(
            Dictionary<MetadataEntryKey, string> entries,
            KeyValuePair<MetadataEntryKey, string> current,
            Dimension targetDimension,
            PxFileLanguages pxLangs)
        {
            if (TryGetAndRemoveProperty(entries, current.Key.KeyWord, pxLangs, out MetaProperty? prop, targetDimension.Name))
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

        private ValueList GetDimensionValues(
            Dictionary<MetadataEntryKey, string> entries,
            PxFileLanguages langs,
            MultilanguageString dimensionName)
        {
            string valueNamesKey = _pxFileSyntaxConf.Tokens.KeyWords.VariableValues;
            if (TryGetAndRemoveProperty(entries, valueNamesKey, langs, out MetaProperty? valueNames, dimensionName))
            {
                List<MultilanguageString> valueNamesList = valueNames.ValueAsListOfMultilanguageStrings(langs.DefaultLanguage, _listSeparator, _stringDelimeter);

                string valueCodesKey = _pxFileSyntaxConf.Tokens.KeyWords.VariableValueCodes;
                List<string> codes = TryGetAndRemoveProperty(entries, valueCodesKey, langs, out MetaProperty? codeSet, dimensionName)
                    ? codeSet.ValueAsListOfStrings(_listSeparator, _stringDelimeter, langs.DefaultLanguage)
                    : new(GetDefaultCodes(valueNamesList));

                int numOfCodes = codes.Count;
                DimensionValue[] values = new DimensionValue[numOfCodes];

                for (int index = 0; index < numOfCodes; index++)
                {
                    values[index] = new DimensionValue(codes[index], valueNamesList[index]);
                }

                return new(values);
            }

            throw new ArgumentException($"Value names not found for dimension {dimensionName[langs.DefaultLanguage]}");

            string[] GetDefaultCodes(List<MultilanguageString> names)
            {
                int length = names.Count;
                string[] defaultCodes = new string[length];
                for (int i = 0; i < length; i++)
                {
                    defaultCodes[i] = names[i][langs.DefaultLanguage];
                }
                return defaultCodes;
            }
        }

        private ContentValueList BuildContentDimensionValues(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimensionName)
        {
            List<ContentDimensionValue> values = [];
            foreach (DimensionValue value in GetDimensionValues(entries, langs, dimensionName))
            {
                MultilanguageString unit = GetUnit(entries, langs, dimensionName, value.Name);
                DateTime lastUpdated = GetLastUpdated(entries, langs, dimensionName, value.Name);
                int precision = GetPrecision(entries, langs, dimensionName, value.Name);
                values.Add(new ContentDimensionValue(value, unit, lastUpdated, precision));
            }
            return new(values);
        }

        private static void AddPropertyToDimensionValue(
            Dictionary<MetadataEntryKey, string> entries,
            KeyValuePair<MetadataEntryKey, string> current,
            MultilanguageString targetDimensionName,
            DimensionValue targetValue,
            PxFileLanguages pxLangs)
        {
            if (TryGetAndRemoveProperty(entries, current.Key.KeyWord, pxLangs, out MetaProperty? prop, targetDimensionName, targetValue.Name))
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
            if (TryGetAndRemoveProperty(entries, current.Key.KeyWord, pxLangs, out MetaProperty? prop, targetValue.Name))
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
            string varCodeKey = _pxFileSyntaxConf.Tokens.KeyWords.DimensionCode;
            char stringDelimeter = _pxFileSyntaxConf.Symbols.Value.StringDelimeter;
            return TryGetAndRemoveProperty(entries, varCodeKey, langs, out MetaProperty? code, dimensionName)
            ? code.ValueAsString(stringDelimeter) : dimensionName[langs.DefaultLanguage];
        }

        private MultilanguageString GetUnit(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimName, MultilanguageString valName)
        {
            string unitKey = _pxFileSyntaxConf.Tokens.KeyWords.Units;
            if (TryGetAndRemoveProperty(entries, unitKey, langs, out MetaProperty? unit, dimName, valName) || // Both identifiers
                TryGetAndRemoveProperty(entries, unitKey, langs, out unit, valName) // Only value identifier
                ) return unit.ValueAsMultilanguageString(_stringDelimeter, langs.DefaultLanguage);

            throw new ArgumentException("Unit information not found");
        }

        private DateTime GetLastUpdated(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimName, MultilanguageString valName)
        {
            string lastUpdatedKey = _pxFileSyntaxConf.Tokens.KeyWords.LastUpdated;
            if (TryGetAndRemoveProperty(entries, lastUpdatedKey, langs, out MetaProperty? lastUpdated, dimName, valName) || // Both identifiers
                TryGetAndRemoveProperty(entries, lastUpdatedKey, langs, out lastUpdated, valName)) // Only value identifier
            {
                string formatString = _pxFileSyntaxConf.Tokens.Time.DateTimeFormatString;
                return lastUpdated.ValueAsDateTime(_stringDelimeter, formatString);
            }

            throw new ArgumentException("Last update information not found");
        }

        private int GetPrecision(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimName, MultilanguageString valName)
        {
            string precisionKey = _pxFileSyntaxConf.Tokens.KeyWords.Precision;
            if (TryGetAndRemoveProperty(entries, precisionKey, langs, out MetaProperty? precision, dimName, valName) || // Both identifiers
               TryGetAndRemoveProperty(entries, precisionKey, langs, out precision, valName)) // Only value identifier
            {
                return int.Parse(precision.GetRawValueString(), CultureInfo.InvariantCulture);
            }

            return 0; // Default value
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

            defaultLang = defaultLang.Trim().Trim(_stringDelimeter);

            MetadataEntryKey availableLangsKey = new(_pxFileSyntaxConf.Tokens.KeyWords.AvailableLanguages);
            List<string> availableLangs = [defaultLang];
            if (entries.TryGetValue(availableLangsKey, out string? availableLangsString))
            {
                availableLangs = availableLangsString
                    .Split(_listSeparator)
                    .Select(s => s.Trim().Trim(_stringDelimeter))
                    .ToList();
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
                    if (TryGetAndRemoveProperty(entries, keyWord, langs, out MetaProperty? prop))
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

        private static bool HasContentDimensionWithValue(IEnumerable<Dimension> dimensions, string name, [MaybeNullWhen(false)] out DimensionValue value)
        {
            Dimension? contentDimension = dimensions.FirstOrDefault(d => d.Type == DimensionType.Content);
            if (contentDimension is not null)
            {
                value = contentDimension.Values
                    .Cast<ContentDimensionValue>()
                    .FirstOrDefault(v => v.Name.Languages.Any(l => v.Name[l] == name));
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
            [MaybeNullWhen(false)] out MetaProperty property,
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
            [MaybeNullWhen(false)] out MetaProperty property,
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

        private static MetaProperty BuildProperty(Dictionary<MetadataEntryKey, string> entries, List<MetadataEntryKey> keys, PxFileLanguages langs)
        {
            if (keys.Count == 0) throw new ArgumentException("No metadta entry keys provided for building property");

            Dictionary<MetadataEntryKey, string> read = [];
            foreach (MetadataEntryKey key in keys)
            {
                if (entries.TryGetValue(key, out string? langValue)) read[key] = langValue;
                else throw new ArgumentException($"Can not build property from provided key: {key.KeyWord}. Key not found in metadata.");
            }

            if (keys.Count == 1) return new MetaProperty(keys[0].KeyWord, read[keys[0]]);
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
