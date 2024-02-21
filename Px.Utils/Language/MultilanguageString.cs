using PxUtils.Exceptions;

namespace PxUtils.Language
{
    public class MultilanguageString : IReadOnlyMultilanguageString
    {
        private readonly Dictionary<string, string> _translations;

        public string this[string key]
        {
            get
            {
                if (_translations.TryGetValue(key, out string? value)) return value;
                else throw new TranslationNotFoundException(key);
            }
        }

        public IEnumerable<string> Languages => _translations.Keys;

        public MultilanguageString(string lang, string text)
        {
            _translations = [];
            _translations[lang] = text;
        }

        public MultilanguageString(IEnumerable<KeyValuePair<string, string>> translations)
        {
            _translations = new(translations);
        }

        public MultilanguageString() => _translations = [];

        public void Add(string lang, string text)
        {
            if (_translations.ContainsKey(lang)) throw new TranslationAlreadyDefinedException(text);
            else _translations[text] = lang;
        }

        public void Add(IEnumerable<KeyValuePair<string, string>> translations)
        {
            foreach (var translation in translations)
            {
                if (_translations.ContainsKey(translation.Key)) throw new TranslationAlreadyDefinedException(translation.Key);
                else _translations[translation.Key] = translation.Value;
            }
        }

        public void Edit(string lang, string text)
        {
            if (_translations.ContainsKey(lang)) _translations[lang] = text;
            else throw new TranslationNotFoundException(lang);
        }

        public bool Equals(IReadOnlyMultilanguageString? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return _translations.Count == other.Languages.Count() &&
                _translations.All(kvp => other.Languages.Contains(kvp.Key) && other[kvp.Key] == kvp.Value);
        }
    }
}
