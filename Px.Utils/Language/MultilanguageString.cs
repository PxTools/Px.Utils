using PxUtils.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace PxUtils.Language
{
    public sealed class MultilanguageString : IReadOnlyMultilanguageString
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

        /// <summary>
        /// Languages for which translations are available
        /// </summary>
        public IEnumerable<string> Languages => _translations.Keys;

        /// <summary>
        /// Constructor for creating a multilanguage string with a single translation
        /// </summary>
        /// <param name="lang">Language for the translation</param>
        /// <param name="text">Content</param>
        public MultilanguageString(string lang, string text)
        {
            _translations = [];
            _translations[lang] = text;
        }

        /// <summary>
        /// Constructor for creating a multilanguage string with multiple translations
        /// </summary>
        /// <param name="translations">Collection of language content pairs</param>
        public MultilanguageString(IEnumerable<KeyValuePair<string, string>> translations)
        {
            _translations = new(translations);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="multilanguageString">To be copied</param>
        public MultilanguageString(MultilanguageString multilanguageString)
        {
            _translations = multilanguageString.Languages
                .ToDictionary(lang => lang, lang => multilanguageString[lang]);
        }

        public MultilanguageString() => _translations = [];

        public void Add(string lang, string text)
        {
            if (_translations.ContainsKey(lang)) throw new TranslationAlreadyDefinedException(text);
            else _translations[lang] = text;
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

        public override bool Equals(object? obj)
        {
            if(obj is IReadOnlyMultilanguageString romls) return Equals(this, romls);
            else return false;
        }

        public bool Equals(IReadOnlyMultilanguageString? other) => Equals(this, other);

        public bool Equals(IReadOnlyMultilanguageString? x, IReadOnlyMultilanguageString? y)
        {
            if (x is null || y is null) return false;
            if (x.Languages.Count() != y.Languages.Count()) return false;
            else foreach (var lang in x.Languages)
            {
                if (!y.Languages.Contains(lang)) return false;
                if (x[lang] != y[lang]) return false;
            }
            return true;
        }

        public override int GetHashCode() => GetHashCode(this);

        public int GetHashCode([DisallowNull] IReadOnlyMultilanguageString obj)
        {
            return obj.Languages
                .OrderBy(lang => lang)
                .Aggregate(0, (hash, lang) => hash ^ obj[lang].GetHashCode());
        }
    }
}
