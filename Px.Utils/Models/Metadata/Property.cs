using PxUtils.Language;

namespace PxUtils.Models.Metadata
{
    public class Property : IReadOnlyProperty
    {
        public string KeyWord { get; }

        public bool CanGetStringValue { get; }
        public bool CanGetMultilanguageValue { get; }

        private MultilanguageString Entries { get; }

        public Property(string keyWord, string value)
        {
            Entries = new MultilanguageString("none", value);
            KeyWord = keyWord;
            CanGetStringValue = true;
            CanGetMultilanguageValue = false;
        }

        public Property(string keyWord, MultilanguageString entries)
        {
            Entries = entries;
            KeyWord = keyWord;
            CanGetStringValue = entries.Languages
                .Select(l => entries[l])
                .Distinct()
                .Count() == 1;
            CanGetMultilanguageValue = true;
        }

        public string GetString()
        {
            if (!CanGetStringValue) throw new InvalidOperationException("Property value can not be represented as a single string");
            else return Entries.UniqueValue();
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
                value = Entries.UniqueValue();
                return true;
            }
        }

        public MultilanguageString GetMultiLanguageString()
        {
            if (!CanGetMultilanguageValue) throw new InvalidOperationException("Value can not be represented as a multilanguage string");
            else return new(Entries);
        }

        public bool TryGetMultilanguageString(out MultilanguageString? value)
        {
            if(!CanGetMultilanguageValue)
            {
                value = null;
                return false;
            }
            else
            {
                value = new(Entries);
                return true;
            }
        }
    }
}
