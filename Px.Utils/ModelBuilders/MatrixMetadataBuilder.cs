using Px.Utils.Language;
using PxUtils.Language;
using PxUtils.Models.Metadata;
using PxUtils.Models.Metadata.Dimensions;
using PxUtils.PxFile;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace PxUtils.ModelBuilders
{
    public class MatrixMetadataBuilder(PxFileSyntaxConf? pxFileSyntaxConf = null)
    {
        private readonly PxFileSyntaxConf _pxFileSyntaxConf = pxFileSyntaxConf ?? PxFileSyntaxConf.Default;

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

            char listSeparator = _pxFileSyntaxConf.Symbols.Key.ListSeparator;
            char stringDelimeter = _pxFileSyntaxConf.Symbols.Key.StringDelimeter;

            MetadataEntryKey defaultLangKey = new(_pxFileSyntaxConf.Tokens.KeyWords.DefaultLanguage);
            MetadataEntryKey availableLangsKey = new(_pxFileSyntaxConf.Tokens.KeyWords.AvailableLanguages);

            if(entries.TryGetValue(defaultLangKey, out string? defaultLang))
            {
                entries.Remove(defaultLangKey);
            }
            else
            {
                throw new ArgumentException($"Default language not found in metadata");
            }

            List<string> availableLangs = [defaultLang];
            if( entries.TryGetValue(availableLangsKey, out string? availableLangsString))
            {
                availableLangs = ValueParserUtilities.ParseStringList(availableLangsString, listSeparator, stringDelimeter);
                entries.Remove(availableLangsKey);
            }

            throw new NotImplementedException();
        }

        private ContentDimension GetContentDimension(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs)
        {
            string contentKey = _pxFileSyntaxConf.Tokens.KeyWords.ContentVariableIdentifier;
            if(TryGetAndRemoveMultilanguageProperty(entries, contentKey, langs, out MultilanguageString? contVarName))
            {
                return BuildContentDimension(entries, langs, contVarName);
            }
            else
            {
                return BuildVirtualContentDimension(entries, langs);
            }
        }

        private ContentDimension BuildContentDimension(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString dimensionName)
        {
            string varCodeKey = _pxFileSyntaxConf.Tokens.KeyWords.VariableCode;
            
            string code = TryGetAndRemoveMultilanguageProperty(entries, varCodeKey, langs, out MultilanguageString? codeSet, dimensionName)
            ? codeSet.UniqueValue() : dimensionName[langs.DefaultLanguage];

            List<ContentDimensionValue> values = BuildContentDimensionValues(entries, langs, dimensionName);

            string defaultValueKey = _pxFileSyntaxConf.Tokens.KeyWords.DimensionDefaultValue;
            if(TryGetAndRemoveMultilanguageProperty(entries, defaultValueKey, langs, out MultilanguageString? defaultValueName))
            {
                ContentDimensionValue? defaultValue = values.Find(v => v.Name == defaultValueName);

                if (defaultValue is null)
                {
                    string eMsg = $"Default value is defined for the content variable but the variable does not contain a value with name {defaultValueName[langs.DefaultLanguage]}";
                    throw new ArgumentException(eMsg);
                }

                return new ContentDimension(code, dimensionName, [], values, defaultValue);
            }

            return new ContentDimension(code, dimensionName, [], values);
        }

        private ContentDimension BuildVirtualContentDimension(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs)
        {
            throw new NotImplementedException();
        }

        private List<ContentDimensionValue> BuildContentDimensionValues(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString varName)
        {
            char listSeparator = _pxFileSyntaxConf.Symbols.Key.ListSeparator;
            char stringDelimeter = _pxFileSyntaxConf.Symbols.Key.StringDelimeter;

            string valueNamesKey = _pxFileSyntaxConf.Tokens.KeyWords.VariableValues;
            if(TryGetAndRemoveMultilanguageProperty(entries, valueNamesKey, langs, out MultilanguageString? nameString, varName))
            {
                Dictionary<string, List<string>> valueNames = langs.AvailableLanguages
                    .ToDictionary(l => l, l => ValueParserUtilities.ParseStringList(nameString[l], listSeparator, stringDelimeter));

                string valueCodesKey = _pxFileSyntaxConf.Tokens.KeyWords.VariableValueCodes;
                List<string> codes = TryGetAndRemoveMultilanguageProperty(entries, valueCodesKey, langs, out MultilanguageString? codeString, varName)
                    ? ValueParserUtilities.ParseStringList(codeString[langs.DefaultLanguage], listSeparator, stringDelimeter)
                    : valueNames[langs.DefaultLanguage];

                return codes.Select((code, index) =>
                {
                    IEnumerable<KeyValuePair<string, string>> langNamePairs = valueNames.Select(langToNameList =>
                        new KeyValuePair<string, string>(langToNameList.Key, langToNameList.Value[index]));
                    
                    MultilanguageString name = new(langNamePairs);
                    MultilanguageString unit = GetUnit(entries, langs, name);
                    DateTime lastUpdated = GetLastUpdated(entries, langs);

                    return new ContentDimensionValue(code, name, unit, lastUpdated);

                }).ToList();
            }

            throw new ArgumentException("Value names not found for the content dimension");
        }

        private MultilanguageString GetUnit(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs, MultilanguageString name)
        {
            string unitKey = _pxFileSyntaxConf.Tokens.KeyWords.Units;
            if (TryGetAndRemoveMultilanguageProperty(entries, unitKey, langs, out MultilanguageString? unit, name) ||
              TryGetAndRemoveMultilanguageProperty(entries, unitKey, langs, out unit))
            {
                return unit;
            }

            throw new ArgumentException("Unit information not found");
        }

        private DateTime GetLastUpdated(Dictionary<MetadataEntryKey, string> entries, PxFileLanguages langs)
        {
            string lastUpdatedKey = _pxFileSyntaxConf.Tokens.KeyWords.LastUpdated;
            if (TryGetAndRemoveMultilanguageProperty(entries, lastUpdatedKey, langs, out MultilanguageString? lastUpdatedString))
            {
                string formatString = _pxFileSyntaxConf.Tokens.Time.DateTimeFormatString;
                return DateTime.ParseExact(lastUpdatedString.UniqueValue(), formatString, CultureInfo.InvariantCulture);
            }

            throw new ArgumentException("Last update information not found");
        }

        private bool TryGetAndRemoveMultilanguageProperty(
            Dictionary<MetadataEntryKey, string> entries,
            string property,
            PxFileLanguages langs,
            [MaybeNullWhen(false)] out MultilanguageString value,
            MultilanguageString? firstIdentifier = null,
            MultilanguageString? secondIdentifier = null)
        {
            if (firstIdentifier is not null && firstIdentifier.Languages.Any(l => !langs.AvailableLanguages.Contains(l)))
            {
                throw new ArgumentException("Required language was not found for the variable name");
            }

            string defLang = langs.DefaultLanguage;

            // Default language
            List<MetadataEntryKey> readKeys = [];
            MetadataEntryKey defaultKey = new(property, null, firstIdentifier?[defLang], secondIdentifier?[defLang]);
            MetadataEntryKey defaultKeyWithLang = new(property, defLang, firstIdentifier?[defLang], secondIdentifier?[defLang]);
            if (entries.TryGetValue(defaultKey, out string? defLangValue))
            {
                readKeys.Add(defaultKey);
            }
            else if (entries.TryGetValue(defaultKeyWithLang, out defLangValue))
            {
                readKeys.Add(defaultKeyWithLang);
            }
            else
            {
                value = null;
                return false;
            }

            value = new(defLang, defLangValue);

            // Other languages
            foreach (string lang in langs.AvailableLanguages.Where(l => l != defLangValue))
            {
                MetadataEntryKey langKey = new(property, lang, firstIdentifier?[lang], secondIdentifier?[defLang]);
                if (entries.TryGetValue(langKey, out string? langValue))
                {
                    value.Add(lang, langValue);
                    readKeys.Add(langKey);
                }
                else
                {
                    value = null;
                    return false;
                }
            }
            readKeys.ForEach(k => entries.Remove(k));
            return true;
        }
    }
}
