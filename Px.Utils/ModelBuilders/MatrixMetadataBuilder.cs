using PxUtils.Models.Metadata;
using PxUtils.Models.Metadata.Dimensions;
using PxUtils.PxFile;

namespace PxUtils.ModelBuilders
{
    public class MatrixMetadataBuilder
    {
        public MatrixMetadata Build(IEnumerable<KeyValuePair<string, string>> metadataInput, PxFileSyntaxConf? pxFileSyntaxConf = null)
        {
            pxFileSyntaxConf ??= PxFileSyntaxConf.Default;
            MetadataEntryKeyBuilder entryKeyBuilder = new();
            IEnumerable<KeyValuePair<MetadataEntryKey, string>> entryIterator =
                metadataInput.Select(kvp =>
                {
                    var entryKey = entryKeyBuilder.Parse(kvp.Key);
                    return new KeyValuePair<MetadataEntryKey, string>(entryKey, kvp.Value);
                });
            Dictionary<MetadataEntryKey, string> entries = new(entryIterator);

            char listSeparator = pxFileSyntaxConf.Symbols.Key.ListSeparator;
            char stringDelimeter = pxFileSyntaxConf.Symbols.Key.StringDelimeter;

            MetadataEntryKey defaultLangKey = new(pxFileSyntaxConf.Tokens.KeyWords.DefaultLanguage);
            MetadataEntryKey availableLangsKey = new(pxFileSyntaxConf.Tokens.KeyWords.AvailableLanguages);

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

            List<IDimension> dimensions = BuildDimensionsAndRemoveRelatedEntries(entries, defaultLang, availableLangs);
            List<Property> additionalProperties = BuildAdditionalMatrixMetadataProperties(entries, defaultLang, availableLangs);

            return new MatrixMetadata(defaultLang, availableLangs, dimensions, additionalProperties);
        }

        private List<IDimension> BuildDimensionsAndRemoveRelatedEntries(Dictionary<MetadataEntryKey, string> entries, string defaultLang, IReadOnlyList<string> availableLangs)
        {
            throw new NotImplementedException();
        }

        private List<Property> BuildAdditionalMatrixMetadataProperties(Dictionary<MetadataEntryKey, string> entries, string defaultLang, IReadOnlyList<string> availableLangs)
        {
            throw new NotImplementedException();
        }
    }
}
