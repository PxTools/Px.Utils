using Px.Utils.Exceptions;
using Px.Utils.Serializers.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Px.Utils.Language
{
    /// <summary>
    /// Class representing a string that has translations in multiple languages
    /// </summary>
    [JsonConverter(typeof(MultilanguageStringConverter))]
    public sealed class MultilanguageString : IEquatable<MultilanguageString>, IEqualityComparer<MultilanguageString>
    {
        private readonly Dictionary<string, string> _translations;

        /// <summary>
        /// Used to get the translation for a specific language
        /// </summary>
        /// <param name="key">The language for which the translation is requested</param>
        /// <returns>A string in the requested language</returns>
        /// <exception cref="TranslationNotFoundException">Thrown if the requested language is not available</exception>
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

        /// <summary>
        /// Returns a new <see cref="MultilanguageString"/> with the specified translation added
        /// </summary>
        /// <param name="lang">Language for the new translation</param>
        /// <param name="text">Value of the new translation</param>
        /// <returns>New <see cref="MultilanguageString"/> with the specified translation added</returns>
        /// <exception cref="TranslationAlreadyDefinedException">Thrown if the specified language already has a translation</exception>
        public MultilanguageString CopyAndAdd(string lang, string text)
        {
            if (_translations.ContainsKey(lang))
            {
                throw new TranslationAlreadyDefinedException(text);
            }
            else
            {
                Dictionary<string, string> newTranslations = new(_translations)
                {
                    [lang] = text
                };
                return new(newTranslations);
            }
        }

        /// <summary>
        /// Returns a new <see cref="MultilanguageString"/> with the specified translations added
        /// </summary>
        /// <param name="translations">Collection of language content pairs to be added</param>
        /// <returns>A new <see cref="MultilanguageString"/> with the specified translations added</returns>
        /// <exception cref="TranslationAlreadyDefinedException">Thrown if any of the specified languages already have a translation</exception>
        public MultilanguageString CopyAndAdd(IEnumerable<KeyValuePair<string, string>> translations)
        {
            Dictionary<string, string> newTranslations = new(_translations);
            foreach (KeyValuePair<string, string> translation in translations)
            {
                if (_translations.ContainsKey(translation.Key)) throw new TranslationAlreadyDefinedException(translation.Key);
                else newTranslations[translation.Key] = translation.Value;
            }
            return new(newTranslations);
        }

        /// <summary>
        /// Returns a new <see cref="MultilanguageString"/> with the specified translation replaced
        /// </summary>
        /// <param name="lang">The language for the translation to be replaced</param>
        /// <param name="newText">The new value of the translation</param>
        /// <returns>A new <see cref="MultilanguageString"/> with the specified translation replaced</returns>
        /// <exception cref="TranslationNotFoundException">Thrown if the specified language does not have a translation</exception>
        public MultilanguageString CopyAndReplace(string lang, string newText)
        {
            if (_translations.ContainsKey(lang))
            {
                Dictionary<string, string> newTranslations = new(_translations)
                {
                    [lang] = newText
                };
                return new(newTranslations);
            }
            else throw new TranslationNotFoundException(lang);
        }

        /// <summary>
        /// Returns a new <see cref="MultilanguageString"/> with the specified function performed on all translations
        /// </summary>
        /// <param name="operation">A function to be performed on all translations</param>
        /// <returns>A new <see cref="MultilanguageString"/> with the specified function performed on all translations</returns>
        public MultilanguageString CopyAndEditAll(Func<string,string> operation)
        {
            return new(_translations.Select(kvp => new KeyValuePair<string, string>(kvp.Key, operation(kvp.Value))));
        }

        /// <summary>
        /// Override for object.Equals() method.
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>true if the objects are equal, false otherwise</returns>
        public override bool Equals(object? obj)
        {
            if(obj is MultilanguageString romls) return Equals(this, romls);
            else return false;
        }

        /// <summary>
        /// Check if this <see cref="MultilanguageString"/> is equal to another <see cref="MultilanguageString"/>
        /// </summary>
        /// <param name="other"><see cref="MultilanguageString"/> to compare to</param>"/>
        /// <returns>true if the objects are equal, false otherwise</returns>
        public bool Equals(MultilanguageString? other) => Equals(this, other);

        /// <summary>
        /// Check if two <see cref="MultilanguageString"/> objects are equal
        /// </summary>
        /// <returns>true if the objects are equal, false otherwise</returns>
        public bool Equals(MultilanguageString? x, MultilanguageString? y)
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

        /// <summary>
        /// Override for object.GetHashCode() method.
        /// </summary>
        /// <returns>An integer hash code</returns>
        public override int GetHashCode() => GetHashCode(this);

        /// <summary>
        /// Get the hash code for a <see cref="MultilanguageString"/> object.
        /// The hascode is based on the translations and two <see cref="MultilanguageString"/> objects with the same translations will have the same hash code.
        /// </summary>
        /// <param name="obj">Object to get the hash code for</param>
        /// <returns>An integer hash code</returns>
        public int GetHashCode([DisallowNull] MultilanguageString obj)
        {
            return obj.Languages
                .OrderBy(lang => lang)
                .Aggregate(0, (hash, lang) => hash ^ obj[lang].GetHashCode());
        }
    }
}
