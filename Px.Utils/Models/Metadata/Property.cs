using PxUtils.Language;

namespace PxUtils.Models.Metadata
{
    public class Property : IReadOnlyProperty
    {
        public IReadOnlyDictionary<MetadataEntryKey, string> Entries { get; }

        public bool CanGetStringValue { get; }
        public bool CanGetMultilanguageValue { get; }

        public Property(MetadataEntryKey key, string value)
        {
            Entries = new Dictionary<MetadataEntryKey, string> { { key, value } };
            CanGetStringValue = true;
            CanGetMultilanguageValue = false;
        }

        public Property(IEnumerable<KeyValuePair<MetadataEntryKey, string>> entries)
        {
            Entries = new Dictionary<MetadataEntryKey, string>(entries);
            CanGetStringValue = Entries.Distinct().Count() == 1;
            CanGetMultilanguageValue = true;
        }

        public string GetString()
        {
            if (!CanGetStringValue) throw new InvalidOperationException("Property value can not be represented as a single string");
            else return Entries.First().Value;
        }

        public bool TryGetString(out string? value)
        {
            if (!CanGetStringValue)
            {
                value = null;
                return false;
            }
            else
            {
                value = Entries.First().Value;
                return true;
            }
        }

        public IReadOnlyMultilanguageString GetMultiLanguageString()
        {
            if (!CanGetMultilanguageValue) throw new InvalidOperationException("Value can not be represented as a multilanguage string");
            else return new MultilanguageString(Entries.Select(kvp => {
                if(kvp.Key.Language is null)
                {
                    throw new InvalidOperationException($"Property {kvp.Key.KeyWord} is missing a language identifier and therefore the value cannot be represented as a multilanguage string.");
                }
                else
                {
                    return new KeyValuePair<string, string>(kvp.Key.Language, kvp.Value);
                }
            }));
        }

        public bool TryGetMultilanguageString(out IReadOnlyMultilanguageString? value)
        {
            if(!CanGetMultilanguageValue)
            {
                value = null;
                return false;
            }
            else
            {
                value =  new MultilanguageString(Entries.Select(kvp => {
                    if (kvp.Key.Language is null)
                    {
                        throw new InvalidOperationException($"Property {kvp.Key.KeyWord} is missing a language identifier and therefore the value cannot be represented as a multilanguage string.");
                    }
                    else
                    {
                        return new KeyValuePair<string, string>(kvp.Key.Language, kvp.Value);
                    }
                }));
                return true;
            }
        }
    }
}
