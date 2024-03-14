using PxUtils.Language;

namespace PxUtils.Models.Metadata.ExtensionMethods
{
    public static class PropertyExtensions
    {
        /// <summary>
        /// Extension method that returns the value of a <see cref="Property"/> object as a list of <see cref="MultilanguageString"/> objects, using the provided list separator and string delimiter.
        /// This method checks if the input <see cref="Property"/> can get a multilanguage value. If it can, it parses each language string into a list of strings and constructs a list of <see cref="MultilanguageString"/> objects.
        /// If the input <see cref="Property"/> cannot get a multilanguage value, it parses the string value of the <see cref="Property"/> into a list of strings and constructs a list of <see cref="MultilanguageString"/> objects with the backup language.
        /// </summary>
        /// <param name="input">The <see cref="Property"/> object to read the value from.</param>
        /// <param name="backupLang">The backup language to use if the <see cref="Property"/> cannot get a multilanguage value.</param>
        /// <param name="listSeparator">The character used to separate items in the list.</param>
        /// <param name="stringDelimeter">The character used to delimit strings in the list.</param>
        /// <returns>A list of <see cref="MultilanguageString"/> objects parsed from the input <see cref="Property"/>.</returns>
        public static List<MultilanguageString> ValueAsListOfMultilanguageStrings(this Property input, string backupLang, char listSeparator, char stringDelimeter)
        {
            if (input.CanGetMultilanguageValue)
            {
                MultilanguageString values = input.GetMultiLanguageString();
                Dictionary<string, List<string>> langToListDict = [];

                // Build dictionary of language to list of strings
                foreach (string lang in values.Languages) langToListDict[lang] = ParseStringList(values[lang], listSeparator, stringDelimeter);

                // Create list of multilanguage strings
                return langToListDict.First().Value.Select((_, index) =>
                {
                    IEnumerable<KeyValuePair<string, string>> translations = langToListDict.Keys
                        .Select(lang => new KeyValuePair<string, string>(lang, langToListDict[lang][index]));
                    return new MultilanguageString(translations);
                }).ToList();
            }
            else
            {
                List<string> list = ParseStringList(input.GetString(), listSeparator, stringDelimeter);
                List<MultilanguageString> result = [];
                foreach (string s in list) result.Add(new MultilanguageString(backupLang, s));
                return result;
            }
        }

        /// <summary>
        /// Extension method that returns the value of a <see cref="Property"/> object as a list of strings, using the provided list separator and string delimiter.
        /// </summary>
        /// <param name="input">The <see cref="Property"/> object to read the value from.</param>
        /// <param name="listSeparator">The character used to separate items in the list.</param>
        /// <param name="stringDelimeter">The character used to delimit strings in the list.</param>
        /// <returns>A list of strings parsed from the input <see cref="Property"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the value of the <see cref="Property"/> cannot be presented as a list of strings.</exception>
        public static List<string> ValueAsListOfStrings(this Property input, char listSeparator, char stringDelimeter)
        {
            if(input.CanGetStringValue)
            {
                return ParseStringList(input.GetString(), listSeparator, stringDelimeter);
            }
            else
            {
                throw new ArgumentException($"The value of the property {input.KeyWord} can not ne presented as a list of strings");
            }
        }

        /// <summary>
        /// Extension method that returns the value of a <see cref="Property"/> object as a string, using the provided string delimiter.
        /// </summary>
        /// <param name="input">The <see cref="Property"/> object to read the value from.</param>
        /// <param name="stringDelimeter">The character used to delimit strings in the list.</param>
        /// <returns>A string parsed from the input <see cref="Property"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the value of the <see cref="Property"/> cannot be presented as a string.</exception>
        public static string ValueAsString(this Property input, char stringDelimeter)
        {
            if(input.CanGetStringValue)
            {
                return input.GetString().Trim().Trim(stringDelimeter);
            }
            else
            {
                throw new ArgumentException($"The value of the property {input.KeyWord} can not be presented as a string");
            }  
        }

        /// <summary>
        /// Parses a string into a list of strings, using the provided list separator and string delimiter.
        /// It throws an exception if the input string is not in the correct format or contains illegal characters.
        /// </summary>
        /// <param name="input">The string to parse into a list of strings.</param>
        /// <param name="listSeparator">The character used to separate items in the list.</param>
        /// <param name="stringDelimeter">The character used to delimit strings in the list.</param>
        /// <returns>A list of strings parsed from the input string.</returns>
        private static List<string> ParseStringList(string input, char listSeparator, char stringDelimeter)
        {
            List<string> list = [];

            if (string.IsNullOrEmpty(input)) return list;

            bool inString = false;
            bool newSection = true;
            int strStartIndex = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == stringDelimeter)
                {
                    if (!newSection && !inString) throw new ArgumentException($"Invalid symbol {c} found in input string {input}");
                    inString = !inString;
                    newSection = false;
                }
                else if (!inString && !char.IsWhiteSpace(c))
                {
                    if (c == listSeparator)
                    {
                        list.Add(input[strStartIndex..i].Trim().Trim(stringDelimeter));
                        strStartIndex = i + 1;
                        newSection = true;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid symbol {c} found in input string {input}");
                    }
                }
            }
            if (inString) throw new ArgumentException($"String not closed in input string {input}");
            if (newSection) throw new ArgumentException($"Invalid input string {input}");
            list.Add(input[strStartIndex..].Trim().Trim(stringDelimeter));

            return list;
        }
    }
}
