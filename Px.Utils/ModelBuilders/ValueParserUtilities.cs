using PxUtils.Language;
using PxUtils.Models.Metadata;
using PxUtils.Models.Metadata.Enums;
using PxUtils.PxFile;

namespace PxUtils.ModelBuilders
{
    /// <summary>
    /// Class containing utility methods for parsing various types of values from Px-style metadata value string.
    /// </summary>
    public static class ValueParserUtilities
    {
        /// <summary>
        /// Parses a string into a list of strings, using the provided list separator and string delimiter.
        /// It throws an exception if the input string is not in the correct format or contains illegal characters.
        /// </summary>
        /// <param name="input">The string to parse into a list of strings.</param>
        /// <param name="listSeparator">The character used to separate items in the list.</param>
        /// <param name="stringDelimeter">The character used to delimit strings in the list.</param>
        /// <returns>A list of strings parsed from the input string.</returns>
        public static List<string> ParseStringList(string input, char listSeparator, char stringDelimeter)
        {
            List<string> list = [];

            if(string.IsNullOrEmpty(input)) return list;

            bool inString = false;
            bool newSection = true;
            int strStartIndex = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == stringDelimeter)
                {
                    if(!newSection && !inString) throw new ArgumentException($"Invalid symbol {c} found in input string {input}");
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

        /// <summary>
        /// Parses a <see cref="Property"/> object into a list of <see cref="MultilanguageString"/> objects, using the provided list separator and string delimiter.
        /// This method checks if the input <see cref="Property"/> can get a multilanguage value. If it can, it parses each language string into a list of strings and constructs a list of <see cref="MultilanguageString"/> objects.
        /// If the input <see cref="Property"/> cannot get a multilanguage value, it parses the string value of the <see cref="Property"/> into a list of strings and constructs a list of <see cref="MultilanguageString"/> objects with the backup language.
        /// </summary>
        /// <param name="input">The <see cref="Property"/> object to parse into a list of <see cref="MultilanguageString"/> objects.</param>
        /// <param name="backupLang">The backup language to use if the <see cref="Property"/> cannot get a multilanguage value.</param>
        /// <param name="listSeparator">The character used to separate items in the list.</param>
        /// <param name="stringDelimeter">The character used to delimit strings in the list.</param>
        /// <returns>A list of <see cref="MultilanguageString"/> objects parsed from the input <see cref="Property"/>.</returns>
        public static List<MultilanguageString> ParseListOfMultilanguageStrings(Property input, string backupLang, char listSeparator, char stringDelimeter)
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
        /// Parses a string into a <see cref="TimeDimensionInterval"/> enumeration value.
        /// This method extracts the time interval part from the input string and maps it to a <see cref="TimeDimensionInterval"/> enumeration value.
        /// It throws an exception if the input string is not in the correct format or does not represent a valid time interval.
        /// </summary>
        /// <param name="input">The string to parse into a <see cref="TimeDimensionInterval"/> enumeration value.</param>
        /// <param name="conf">An optional <see cref="PxFileSyntaxConf"/> object. If not provided, the default configuration is used.</param>
        /// <returns>A <see cref="TimeDimensionInterval"/> enumeration value parsed from the input string.</returns>
        public static TimeDimensionInterval ParseTimeIntervalFromTimeVal(string input, PxFileSyntaxConf? conf = null)
        {
            conf ??= PxFileSyntaxConf.Default;
            string intervalPart = new(input.TakeWhile(c => c != conf.Symbols.Value.ListSeparator).ToArray());
            string inputStart = conf.Tokens.Time.TimeIntervalIndicator + conf.Symbols.Value.TimeSeriesIntervalStart;
            char inputEnd = conf.Symbols.Value.TimeSeriesIntervalEnd;
            if(!string.IsNullOrEmpty(intervalPart) && intervalPart.StartsWith(inputStart) && intervalPart.EndsWith(inputEnd))
            {
                Dictionary<string, TimeDimensionInterval> map = new()
                {
                    {conf.Tokens.Time.YearInterval, TimeDimensionInterval.Year},
                    {conf.Tokens.Time.HalfYearInterval, TimeDimensionInterval.HalfYear},
                    {conf.Tokens.Time.QuarterYearInterval, TimeDimensionInterval.Quarter},
                    {conf.Tokens.Time.MonthInterval, TimeDimensionInterval.Month},
                    {conf.Tokens.Time.WeekInterval, TimeDimensionInterval.Week}
                };

                if(map.TryGetValue(intervalPart[6..^1], out TimeDimensionInterval interval)) return interval;
                else return TimeDimensionInterval.Other;
            }
            else
            {
                throw new ArgumentException($"Invalid time interval string {input}");
            }
        }

        /// <summary>
        /// Parses a string into a <see cref="DimensionType"/> enumeration value.
        /// This method maps the input string to a <see cref="DimensionType"/> enumeration value based on the provided or default PxFileSyntaxConf configuration.
        /// If the input string does not map to a known <see cref="DimensionType"/>, the method returns <see cref="DimensionType.Unknown"/>.
        /// </summary>
        /// <param name="input">The string to parse into a <see cref="DimensionType"/> enumeration value.</param>
        /// <param name="conf">An optional <see cref="PxFileSyntaxConf"/> object. If not provided, the default configuration is used.</param>
        /// <returns>A <see cref="DimensionType"/> enumeration value parsed from the input string.</returns>
        public static DimensionType StringToDimensionType(string input, PxFileSyntaxConf? conf = null)
        {
            conf ??= PxFileSyntaxConf.Default;
            Dictionary<string, DimensionType> map = new()
            {
                {conf.Tokens.VariableTypes.Content, DimensionType.Content},
                {conf.Tokens.VariableTypes.Time, DimensionType.Time},
                {conf.Tokens.VariableTypes.Ordinal, DimensionType.Ordinal},
                {conf.Tokens.VariableTypes.Nominal, DimensionType.Nominal},
                {conf.Tokens.VariableTypes.Geographical, DimensionType.Geographical},
                {conf.Tokens.VariableTypes.Other, DimensionType.Other},
                {conf.Tokens.VariableTypes.Unknown, DimensionType.Unknown}
            };

            if (map.TryGetValue(input, out DimensionType value)) return value;
            else return DimensionType.Unknown;
        }
    }
}
