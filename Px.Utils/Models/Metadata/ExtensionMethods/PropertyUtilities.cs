using Px.Utils.Language;
using Px.Utils.Models.Metadata.Enums;
using Px.Utils.PxFile;
using System.Globalization;

namespace Px.Utils.Models.Metadata.ExtensionMethods
{
    public static class PropertyUtilities
    {
        /// <summary>
        /// Converts a <see cref="MultilanguageString"/> object to a list of <see cref="MultilanguageString"/> objects.
        /// Each string in the original object is split into a list of strings using the provided list separator and string delimiter.
        /// The resulting list of objects is then constructed from those lists, with each source list representing the translations in different languages.
        /// </summary>
        /// <param name="input">The <see cref="MultilanguageString"/> object to convert.</param>
        /// <param name="listSeparator">The character used to separate items in the list.</param>
        /// <param name="stringDelimeter">The character used to delimit strings in the list.</param>
        /// <returns>A list of <see cref="MultilanguageString"/> objects where each object is constructed from the translations in the same position in the source lists.</returns>
        public static List<MultilanguageString> ValueAsListOfMultilanguageStrings(this MultilanguageString input, char listSeparator, char stringDelimeter)
        {
            Dictionary<string, List<string>> langToListDict = [];

            // Build dictionary of language to list of strings
            foreach (string lang in input.Languages) langToListDict[lang] = ParseStringList(input[lang], listSeparator, stringDelimeter);

            // Create list of multilanguage strings
            return langToListDict.First().Value.Select((_, index) =>
            {
                IEnumerable<KeyValuePair<string, string>> translations = langToListDict.Keys
                    .Select(lang => new KeyValuePair<string, string>(lang, langToListDict[lang][index]));
                return new MultilanguageString(translations);
            }).ToList();
        }

        /// <summary>
        /// Converts a string into a list of strings by splitting the input string using the provided list separator and string delimiter.
        /// </summary>
        /// <param name="input">The string to convert into a list of strings.</param>
        /// <param name="listSeparator">The character used to separate items in the list.</param>
        /// <param name="stringDelimeter">The character used to delimit strings in the list.</param>
        /// <returns>A list of strings parsed from the input string.</returns>
        public static List<string> SplitToListOfStrings(this string input, char listSeparator, char stringDelimeter)
        {
            return ParseStringList(input, listSeparator, stringDelimeter);
        }

        /// <summary>
        /// Cleans the strings in a <see cref="MultilanguageString"/> object by removing the string delimiters
        /// and if the string was in multiple parts, combines those parts.
        /// </summary>
        /// <param name="input">Multilanguage string to clean</param>
        /// <param name="stringDelimeter">There symbols will be removed from the strings</param>
        /// <returns><see cref="MultilanguageString"/> object with cleaned strings.</returns>
        public static MultilanguageString CleanStringDelimeters(this MultilanguageString input, char stringDelimeter)
        {
            return input.CopyAndEditAll(s => CleanString(s, stringDelimeter));
        }

        /// <summary>
        /// Removes the delimeters from a string and combines possible multiple strings into a single string.
        /// </summary>
        /// <param name="input">Input to be cleaned and combined.</param>
        /// <param name="stringDelimeter">Removes this character from the string</param>
        /// <returns>Cleaned and combined string</returns>
        public static string CleanStringDelimeters(this string input, char stringDelimeter)
        {
            return CleanString(input, stringDelimeter);
        }


        /// <summary>
        /// Converts a string representation of a date and time to a DateTime object using the specified string delimiter and format string.
        /// </summary>
        /// <param name="input">The string representation of the date and time.</param>
        /// <param name="stringDelimeter">The character used to delimit strings in the input.</param>
        /// <param name="formatString">The format string used to parse the input string into a DateTime object.</param>
        /// <returns>A DateTime object representing the parsed date and time.</returns>
        public static DateTime AsDatetime(this string input, char stringDelimeter, string formatString)
        {
            return DateTime.ParseExact(CleanString(input, stringDelimeter), formatString, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Determines the content type of the input string based on the provided configuration.
        /// </summary>
        /// <param name="input">The input string to determine the content type of.</param>
        /// <param name="config">The configuration object that defines the symbols and tokens used for content type determination.</param>
        /// <returns>The content type of the input string.</returns>
        /// <exception cref="ArgumentException">Thrown when the input string is not in the correct format or contains illegal characters.</exception>
        public static MetaPropertyType GetPropertyValueType(this string input, PxFileSyntaxConf config)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input string is empty");
            }

            if (input[0] == config.Symbols.Value.StringDelimeter)
            {
                return GetStringType(input, config);
            }

            if (input == config.Tokens.Booleans.Yes || input == config.Tokens.Booleans.No)
            {
                return MetaPropertyType.Boolean;
            }

            if (double.TryParse(input, out _))
            {
                return MetaPropertyType.Numeric;
            }

            throw new ArgumentException($"Invalid input string {input} when determining content type");
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
            string outputStr = new(output, 0, outputIndex);
            if (string.IsNullOrEmpty(outputStr)) return input;
            else return outputStr;
        }


        private static MetaPropertyType GetStringType(string input, PxFileSyntaxConf config)
        {
            int count = 0;
            bool inString = false;

            foreach (char c in input)
            {
                if (c == config.Symbols.Value.StringDelimeter)
                {
                    inString = !inString;
                }
                else if (!inString)
                {
                    if (c == config.Symbols.Value.ListSeparator)
                    {
                        count++;
                    }
                    else if (!char.IsWhiteSpace(c))
                    {
                        throw new ArgumentException($"Invalid symbol {c} found in input string {input} when determining content type");
                    }
                }
            }

            if (inString)
            {
                throw new ArgumentException($"String not closed in input string {input} when determining content type");
            }
            else if (count > 0)
            {
                return MetaPropertyType.TextArray;
            }
            else
            {
                return MetaPropertyType.Text;
            }
        }

        #endregion
    }
}
