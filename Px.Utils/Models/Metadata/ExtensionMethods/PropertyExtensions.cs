using PxUtils.Language;
using System.Globalization;

namespace PxUtils.Models.Metadata.ExtensionMethods
{
    public static class PropertyExtensions
    {
        /// <summary>
        /// Extension method that returns the value of a <see cref="MetaProperty"/> object as a list of <see cref="MultilanguageString"/> objects,
        /// using the provided list separator and string delimiter.
        /// This method checks if the input <see cref="MetaProperty"/> can get a multilanguage value.
        /// If it can, it parses each language string into a list of strings and constructs a list of <see cref="MultilanguageString"/> objects.
        /// If the input <see cref="MetaProperty"/> cannot get a multilanguage value, it parses the string value of the <see cref="MetaProperty"/> into a list of strings 
        /// and constructs a list of <see cref="MultilanguageString"/> objects with the backup language.
        /// </summary>
        /// <param name="input">The <see cref="MetaProperty"/> object to read the value from.</param>
        /// <param name="backupLang">The backup language to use if the <see cref="MetaProperty"/> cannot get a multilanguage value.</param>
        /// <param name="listSeparator">The character used to separate items in the list.</param>
        /// <param name="stringDelimeter">The character used to delimit strings in the list.</param>
        /// <returns>A list of <see cref="MultilanguageString"/> objects parsed from the input <see cref="MetaProperty"/>.</returns>
        public static List<MultilanguageString> ValueAsListOfMultilanguageStrings(this MetaProperty input, string backupLang, char listSeparator, char stringDelimeter)
        {
            if (input.CanGetMultilanguageValue)
            {
                MultilanguageString values = input.GetRawValueMultiLanguageString();
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
                List<string> list = ParseStringList(input.GetRawValueString(), listSeparator, stringDelimeter);
                List<MultilanguageString> result = [];
                foreach (string s in list) result.Add(new MultilanguageString(backupLang, s));
                return result;
            }
        }

        /// <summary>
        /// Extension method that returns the value of a <see cref="MetaProperty"/> object as a list of strings, using the provided list separator and string delimiter.
        /// </summary>
        /// <param name="input">The <see cref="MetaProperty"/> object to read the value from.</param>
        /// <param name="listSeparator">The character used to separate items in the list.</param>
        /// <param name="stringDelimeter">The character used to delimit strings in the list.</param>
        /// <returns>A list of strings parsed from the input <see cref="MetaProperty"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the value of the <see cref="MetaProperty"/> cannot be presented as a list of strings.</exception>
        public static List<string> ValueAsListOfStrings(this MetaProperty input, char listSeparator, char stringDelimeter, string? forceLanguage = null)
        {
            if(input.CanGetStringValue)
            {
                return ParseStringList(input.GetRawValueString(), listSeparator, stringDelimeter);
            }

            MultilanguageString multilangValue = input.GetRawValueMultiLanguageString();

            if (forceLanguage is not null && multilangValue.Languages.Contains(forceLanguage))
            {
                return ParseStringList(multilangValue[forceLanguage], listSeparator, stringDelimeter);
            }
            else
            {
                throw new ArgumentException($"The value of the property {input.KeyWord} can not ne presented as a list of strings");
            }
        }

        public static MultilanguageString ValueAsMultilanguageString(this MetaProperty input, char stringDelimeter, string backupLang)
        {
            if (input.CanGetMultilanguageValue)
            {
                return input.GetRawValueMultiLanguageString().CopyAndEditAll(s => CleanString(s, stringDelimeter));
            }
            else
            {
                return new MultilanguageString(backupLang, input.ValueAsString(stringDelimeter));
            }
        }

        /// <summary>
        /// Extension method that returns the value of a <see cref="MetaProperty"/> object as a string, using the provided string delimiter.
        /// </summary>
        /// <param name="input">The <see cref="MetaProperty"/> object to read the value from.</param>
        /// <param name="stringDelimeter">The character used to delimit strings in the list.</param>
        /// <returns>A string parsed from the input <see cref="MetaProperty"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the value of the <see cref="MetaProperty"/> cannot be presented as a string.</exception>
        public static string ValueAsString(this MetaProperty input, char stringDelimeter)
        {
            if(input.CanGetStringValue)
            {
                return CleanString(input.GetRawValueString(), stringDelimeter);
            }
            else
            {
                throw new ArgumentException($"The value of the property {input.KeyWord} can not be presented as a string");
            }  
        }


        /// <summary>
        /// Extension method that returns the value of a <see cref="MetaProperty"/> object as a <see cref="DateTime"/> object, using the provided string delimiter and format string.
        /// </summary>
        /// <param name="input">Input property</param>
        /// <param name="stringDelimeter">The datetimes are surrounded by this character in the px file</param>
        /// <param name="formatString">The format string to use when parsing the datetime</param></param>
        /// <returns>A <see cref="DateTime"/> object parsed from the input <see cref="MetaProperty"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the value of the <see cref="MetaProperty"/> cannot be presented as a <see cref="DateTime"/>.</exception>
        public static DateTime ValueAsDateTime(this MetaProperty input, char stringDelimeter, string formatString)
        {
            if (input.CanGetStringValue)
            {
                return DateTime.ParseExact(input.ValueAsString(stringDelimeter), formatString, CultureInfo.InvariantCulture);
            }
            else
            {
                throw new ArgumentException($"The value of the property {input.KeyWord} can not be presented as a DateTime");
            }
        }

        #region Private methods

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

        private static string CleanString(string input, char stringDelimeter)
        {
            char[] output = new char[input.Length];
            int outputIndex = 0;
            bool inString = false;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == stringDelimeter)
                {
                    inString = !inString;
                }
                else if (inString)
                {
                    output[outputIndex++] = input[i];
                }
            }
            return new(output, 0, outputIndex);
        }

        #endregion
    }
}
