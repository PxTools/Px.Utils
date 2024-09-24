using Px.Utils.Models.Metadata.Dimensions;
using Px.Utils.Language;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.Models.Metadata.ExtensionMethods;
using Px.Utils.PxFile;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Px.Utils.Models.Metadata.MetaProperties;

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
            return BuildFromEntries(entries);
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
            return BuildFromEntries(entries);
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
            return BuildFromEntries(entries);
        }

        /// <summary>
        /// The metadata is constructed using a dictionary because the order they are required doesn't necessarily match the order thay are in the file.
        /// </summary>
        private MatrixMetadata BuildFromEntries(Dictionary<MetadataEntryKey, string> entries)
        {
            PxFileLanguages langs = GetLanguages(entries);
            List<MultilanguageString> dimensionNames = [];

            string stubKey = _pxFileSyntaxConf.Tokens.KeyWords.StubDimensions;
            if (TryGetEntries(entries, stubKey, langs, out Dictionary<MetadataEntryKey, string>? stubEntries))
            {
                MultilanguageString stubLists = new(stubEntries.ToDictionary(
                    stubEntries => stubEntries.Key.Language ?? langs.DefaultLanguage,
                    stubEntries => stubEntries.Value
                    ));
                dimensionNames.AddRange(stubLists.ValueAsListOfMultilanguageStrings(_listSeparator, _stringDelimeter));
            }
            else
            {
                throw new ArgumentException("Stub dimension names not found in metadata");
            }

            string headingKey = _pxFileSyntaxConf.Tokens.KeyWords.HeadingDimensions;
            if (TryGetEntries(entries, headingKey, langs, out Dictionary<MetadataEntryKey, string>? headingEntries))
            {
                MultilanguageString headingLists = new(headingEntries.ToDictionary(
                    headingEntries => headingEntries.Key.Language ?? langs.DefaultLanguage,
                    headingEntries => headingEntries.Value
                    ));
                dimensionNames.AddRange(headingLists.ValueAsListOfMultilanguageStrings(_listSeparator, _stringDelimeter));
            }
            else
            {
                throw new ArgumentException("Heading dimension names not found in metadata");
            }

            // Because we dont remove the stub or heading entries they are automatically added as additional properties.
            ContentDimension? maybeCd = GetContentDimensionIfAvailable(entries, langs);

            List<Dimension> dimensions = dimensionNames.Select(name =>
                {
                    if (maybeCd is not null && name.Equals(maybeCd.Name)) return maybeCd;
                    else if (TestIfTimeAndBuild(entries, langs, name, out TimeDimension? timeDim)) return timeDim;
                    else return BuildDimension(entries, langs, name);
                }).ToList();

            MatrixMetadata meta = new(langs.DefaultLanguage, langs.AvailableLanguages, dimensions, []);
            AddAdditionalPropertiesToMatrixMetadata(meta, entries, langs);
            return meta;
        }

        #region Dimension building

        private ContentDimension? GetContentDimensionIfAvailable(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs)
        {
            string contentKey = _pxFileSyntaxConf.Tokens.KeyWords.ContentVariableIdentifier;
            if (TryGetEntries(entries, contentKey, langs, out Dictionary<MetadataEntryKey, string>? contVarNameEntries))
            {
                foreach (MetadataEntryKey key in contVarNameEntries.Keys) entries.Remove(key);
                MultilanguageString name = new(contVarNameEntries.ToDictionary(kvp => kvp.Key.Language ?? langs.DefaultLanguage, kvp => kvp.Value));
                return BuildContentDimension(entries, langs, name.CleanStringDelimeters(_stringDelimeter));
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
            if (TryGetEntries(entries, timeValIdentifierKey, langs, out Dictionary<MetadataEntryKey, string>? timeValEntries, dimensionNameToTest))
            {
                string timeValValueString = timeValEntries.Values.First();
                List<string> timeValList = ValueParserUtilities.GetTimeValValueList(timeValValueString, _pxFileSyntaxConf);

                timeDimension = new(
                    code: GetDimensionCode(entries, langs, dimensionNameToTest),
                    name: dimensionNameToTest,
                    additionalProperties: new() { { timeValIdentifierKey, new StringListProperty(timeValList) } },
                    values: GetDimensionValues(entries, langs, dimensionNameToTest),
                    interval: ValueParserUtilities.ParseTimeIntervalFromTimeVal(timeValValueString, _pxFileSyntaxConf)
                    );

                foreach (MetadataEntryKey key in timeValEntries.Keys) entries.Remove(key);
                return true;
            }
            else if (TryGetEntries(entries, dimensionTypeKey, langs, out Dictionary<MetadataEntryKey, string>? dimTypeEntries, dimensionNameToTest) &&
                ValueParserUtilities.StringToDimensionType(dimTypeEntries.Values.First()) == DimensionType.Time)
            {
                timeDimension = new(
                    code: GetDimensionCode(entries, langs, dimensionNameToTest),
                    name: dimensionNameToTest,
                    additionalProperties: [],
                    values: GetDimensionValues(entries, langs, dimensionNameToTest),
                    interval: TimeDimensionInterval.Irregular
                    );

                foreach (MetadataEntryKey key in dimTypeEntries.Keys) entries.Remove(key);
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
            DimensionType type = DimensionType.Unknown;
            if (TryGetEntries(entries, dimensionTypeKey, langs, out Dictionary<MetadataEntryKey, string>? dimTypeEntries, dimensionName))
            {
                type = ValueParserUtilities.StringToDimensionType(dimTypeEntries.Values.First());
                foreach (MetadataEntryKey key in dimTypeEntries.Keys) entries.Remove(key);
            }

            if (type is DimensionType.Other or DimensionType.Unknown)
            {
                // Checks for a map property, this is a legacy way of defining geographical dimensions
                string mapKey = _pxFileSyntaxConf.Tokens.KeyWords.Map;
                if (TryGetEntries(entries, mapKey, langs, out Dictionary<MetadataEntryKey, string>? _, dimensionName))
                {
                    type = DimensionType.Geographical;
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

        private void AddAdditionalPropertiesToDimensions(
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
                DimensionValue? value = (targetDimension?.Values.Find<DimensionValue>(v => v.Name[current.Key.Language ?? pxLangs.DefaultLanguage] == current.Key.SecondIdentifier)) ??
                    throw new ArgumentException($"Failed to build property for key {current.Key} because the value with name {current.Key.SecondIdentifier} was not found.");
                AddPropertyToDimensionValue(entries, current, targetDimension.Name, value, pxLangs);
            }
        }

        private void AddAdditionalPropertyToDimension(
            Dictionary<MetadataEntryKey, string> entries,
            KeyValuePair<MetadataEntryKey, string> current,
            Dimension targetDimension,
            PxFileLanguages pxLangs)
        {
            if (TryGetAndRemoveProperty(entries, current.Key.KeyWord, pxLangs, out MetaProperty? prop, targetDimension.Name))
            {
                targetDimension.AdditionalProperties[current.Key.KeyWord] = prop;
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
            Dictionary<string, string> nameListsByLang = [];
            if (TryGetEntries(entries, valueNamesKey, langs, out Dictionary<MetadataEntryKey, string>? valueNameEntries, dimensionName))
            {
                foreach (KeyValuePair<MetadataEntryKey, string> kvp in valueNameEntries)
                {
                    entries.Remove(kvp.Key);
                    nameListsByLang[kvp.Key.Language ?? langs.DefaultLanguage] = kvp.Value;
                }
            }
            else
            {
                throw new ArgumentException($"Value names not found for dimension {dimensionName[langs.DefaultLanguage]}");
            }
            
            List<MultilanguageString> valueNames = new MultilanguageString(nameListsByLang)
                .ValueAsListOfMultilanguageStrings(_listSeparator, _stringDelimeter);

            DimensionValue[] values = new DimensionValue[valueNames.Count];
            string valueCodesKey = _pxFileSyntaxConf.Tokens.KeyWords.VariableValueCodes;
            if(TryGetEntries(entries, valueCodesKey, langs, out Dictionary<MetadataEntryKey, string>? codeSet, dimensionName))
            {
                foreach (MetadataEntryKey key in codeSet.Keys) entries.Remove(key);
                List<string> codes = codeSet.Values.First().SplitToListOfStrings(_listSeparator, _stringDelimeter);
                for (int index = 0; index < valueNames.Count; index++)
                {
                    values[index] = new DimensionValue(codes[index], valueNames[index]);
                }
            }
            else
            {
                for (int index = 0; index < valueNames.Count; index++)
                {
                    values[index] = new DimensionValue(valueNames[index][langs.DefaultLanguage], valueNames[index]);
                }
            }

            return new(values);
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

        private void AddPropertyToDimensionValue(
            Dictionary<MetadataEntryKey, string> entries,
            KeyValuePair<MetadataEntryKey, string> current,
            MultilanguageString targetDimensionName,
            DimensionValue targetValue,
            PxFileLanguages pxLangs)
        {
            if (TryGetAndRemoveProperty(entries, current.Key.KeyWord, pxLangs, out MetaProperty? prop, targetDimensionName, targetValue.Name))
            {
                targetValue.AdditionalProperties[current.Key.KeyWord] = prop;
            }
            else
            {
                string eMsg = $"Failed to build property for key {current.Key}, either some required metadata is missing or contains errors.";
                throw new ArgumentException(eMsg);
            }
        }

        private void AddPropertyToContentDimensionValue(
            Dictionary<MetadataEntryKey, string> entries,
            KeyValuePair<MetadataEntryKey, string> current,
            DimensionValue targetValue,
            PxFileLanguages pxLangs)
        {
            if (TryGetAndRemoveProperty(entries, current.Key.KeyWord, pxLangs, out MetaProperty? prop, targetValue.Name))
            {
                targetValue.AdditionalProperties[current.Key.KeyWord] = prop;
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
            if (TryGetEntries(entries, varCodeKey, langs, out Dictionary<MetadataEntryKey, string>? codeEntries, dimensionName))
            {
                foreach (MetadataEntryKey key in codeEntries.Keys) entries.Remove(key);
                return codeEntries.Values.First().CleanStringDelimeters(_stringDelimeter);
            }
            return dimensionName[langs.DefaultLanguage];
        }

        private MultilanguageString GetUnit(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimName, MultilanguageString valName)
        {
            string unitKey = _pxFileSyntaxConf.Tokens.KeyWords.Units;
            if (TryGetEntries(entries, unitKey, langs, out Dictionary<MetadataEntryKey, string>? unitEntries, dimName, valName) || // Both identifiers
                TryGetEntries(entries, unitKey, langs, out unitEntries, valName) || // Only value identifier 
                TryGetEntries(entries, unitKey, langs, out unitEntries) // No identifiers 
            {
                Dictionary<string, string> translations = [];
                foreach (KeyValuePair<MetadataEntryKey, string> kvp in unitEntries)
                {
                    translations[kvp.Key.Language ?? langs.DefaultLanguage] = kvp.Value.CleanStringDelimeters(_stringDelimeter);
                    entries.Remove(kvp.Key);
                }
                return new MultilanguageString(translations);
            }
            throw new ArgumentException("Unit information not found");
        }

        private DateTime GetLastUpdated(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimName, MultilanguageString valName)
        {
            string lastUpdatedKey = _pxFileSyntaxConf.Tokens.KeyWords.LastUpdated;
            if (TryGetEntries(entries, lastUpdatedKey, langs, out Dictionary<MetadataEntryKey, string>? lastUpdatedEntries, dimName, valName) || // Both identifiers
                TryGetEntries(entries, lastUpdatedKey, langs, out lastUpdatedEntries, valName)) // Only value identifier
            {
                foreach (MetadataEntryKey key in lastUpdatedEntries.Keys) entries.Remove(key);
                string formatString = _pxFileSyntaxConf.Tokens.Time.DateTimeFormatString;
                return DateTime lastUpdated = lastUpdatedEntries.Values
                    .Select(v => DateTime.ParseExact(v.CleanStringDelimeters(_stringDelimeter), formatString, CultureInfo.InvariantCulture))
                    .OrderByDescending().First();
            }

            throw new ArgumentException("Last update information not found");
        }

        private int GetPrecision(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimName, MultilanguageString valName)
        {
            string precisionKey = _pxFileSyntaxConf.Tokens.KeyWords.Precision;
            if ((TryGetEntries(entries, precisionKey, langs, out Dictionary<MetadataEntryKey, string>? precisionEntries, dimName, valName) || // Both identifiers
               TryGetEntries(entries, precisionKey, langs, out precisionEntries, valName)) // Only value identifier
               && int.TryParse(precisionEntries.Values.First(), out int result)) 
            {
                foreach (MetadataEntryKey key in precisionEntries.Keys) entries.Remove(key);
                return result;
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

        private void AddAdditionalPropertiesToMatrixMetadata(MatrixMetadata metadata, Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs)
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

        #region Property reading

        private bool TryGetAndRemoveProperty(
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

        private MetaProperty BuildProperty(Dictionary<MetadataEntryKey, string> entries, List<MetadataEntryKey> keys, PxFileLanguages langs)
        {
            if (keys.Count == 0) throw new ArgumentException("No metadata entry keys provided for building property");

            Dictionary<MetadataEntryKey, string> read = [];
            foreach (MetadataEntryKey key in keys)
            {
                if (entries.TryGetValue(key, out string? langValue)) read[key] = langValue;
                else throw new ArgumentException($"Cannot build property from provided key: {key.KeyWord}. Key not found in metadata.");
            }

            char stringDelimeter = _pxFileSyntaxConf.Symbols.Value.StringDelimeter;
            char listSeparator = _pxFileSyntaxConf.Symbols.Value.ListSeparator;

            Dictionary<string, MetaPropertyType> typeDict = PxFileSyntaxConf.ContentConfiguration.EntryValueTypes.GetTypeDictionary(_pxFileSyntaxConf);
            if (!typeDict.TryGetValue(keys[0].KeyWord, out MetaPropertyType type))
            {
                type = read[keys[0]].GetPropertyValueType(_pxFileSyntaxConf);
            }

            MultilanguageString BuildMLS() => new(read.Select(kvp => new KeyValuePair<string, string>(kvp.Key.Language ?? langs.DefaultLanguage, kvp.Value)));

            switch (type)
            {
                case MetaPropertyType.Text:
                case MetaPropertyType.MultilanguageText:
                    // Number of keys == number of translations
                    if (keys.Count == 1 && keys[0].Language is null)
                        return new StringProperty(read[keys[0]].CleanStringDelimeters(stringDelimeter));
                    else
                        return new MultilanguageStringProperty(BuildMLS().CleanStringDelimeters(stringDelimeter));
                case MetaPropertyType.TextArray:
                case MetaPropertyType.MultilanguageTextArray:
                    // Number of keys == number of tranaslations
                    if (keys.Count == 1 && keys[0].Language is null)
                        return new StringListProperty(read[keys[0]].SplitToListOfStrings(listSeparator, stringDelimeter));
                    else
                        return new MultilanguageStringListProperty(BuildMLS().ValueAsListOfMultilanguageStrings(listSeparator, stringDelimeter));
                case MetaPropertyType.Boolean:
                    return new BooleanProperty(read[keys[0]] == _pxFileSyntaxConf.Tokens.Booleans.Yes);
                case MetaPropertyType.Numeric:
                    return new NumericProperty(double.Parse(read[keys[0]], CultureInfo.InvariantCulture));
                default:
                    throw new ArgumentException($"Unsupported MetaValueType: {type}");
            }
        }

        private static bool TryGetEntries(
            Dictionary<MetadataEntryKey, string> entries,
            string propertyName,
            PxFileLanguages langs,
            [MaybeNullWhen(false)] out Dictionary<MetadataEntryKey,string> readEntries,
            MultilanguageString? firstIdentifier = null,
            MultilanguageString? secondIdentifier = null)
        {
            List<MetadataEntryKey> keys = BuildKeys(entries, propertyName, langs, firstIdentifier, secondIdentifier);
            if (keys.Count == 0)
            {
                readEntries = null;
                return false;
            }
            else
            {
                readEntries = [];
                foreach (MetadataEntryKey key in keys)
                {
                    if (entries.TryGetValue(key, out string? langValue)) readEntries[key] = langValue;
                    else throw new ArgumentException($"Requested key not found in metadata: {key.KeyWord}.");
                }

                return true;
            }
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

            // Other languages
            List<MetadataEntryKey> otherLanguageKeys = [];
            foreach (string lang in langs.AvailableLanguages.Where(l => l != defLang))
            {
                MetadataEntryKey langKey = new(propertyName, lang, firstIdentifier?[lang], secondIdentifier?[lang]);
                if (entries.ContainsKey(langKey)) otherLanguageKeys.Add(langKey);
            }

            keys.AddRange(otherLanguageKeys);
            return keys;
        }

        #endregion
    }
}
