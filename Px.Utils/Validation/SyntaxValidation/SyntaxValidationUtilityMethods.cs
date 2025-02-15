using Px.Utils.PxFile;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Concurrent;

namespace Px.Utils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents the result of extracting sections from a string. This struct contains an array of the extracted sections and the input of the string after extraction.
    /// </summary>
    /// <param name="parameters">The sections extracted from the string.</param>
    /// <param name="remainder">The input of the string after extraction.</param>
    /// <param name="startIndexes">The indexes of the starting characters of the sections in the input string.</param>
    internal readonly struct ExtractSectionResult(string[] parameters, string remainder, int[] startIndexes)
    {
        /// <summary>
        /// Gets the sections extracted from the string.
        /// </summary>
        public string[] Sections { get; } = parameters;

        /// <summary>
        /// Gets the input of the string after extraction.
        /// </summary>
        public string Remainder { get; } = remainder;

        /// <summary>
        /// Gets the indexes of the starting characters of the sections in the input string.
        /// </summary>
        public int[] StartIndexes { get; } = startIndexes;
    }

    /// <summary>
    /// Provides a collection of helper methods used during the syntax validation process. 
    /// These methods include functionality for extracting sections from a string, checking the format of a string, and determining the type of a value from a string.
    /// </summary>
    public static class SyntaxValidationUtilityMethods
    {
        private static readonly ConcurrentDictionary<string, Regex> regexPatterns = [];
        private static readonly TimeSpan timeout = TimeSpan.FromMilliseconds(50);

        /// <summary>
        /// Extracts a section from a string enclosed with given symbols.
        /// </summary>
        /// <param name="input">The input string to extract from</param>
        /// <param name="startSymbol">Symbol that starts enclosement</param>
        /// <param name="endSymbol">Optional symbol that closes the enclosement. If none given, startSymbol is used for both starting and ending the enclosement</param>
        /// <return>Returns an <see cref="ExtractSectionResult"/> object that contains the extracted sections,
        /// the string that remains after the operation and starting indexes of extracted sections</return>
        internal static ExtractSectionResult ExtractSectionFromString(string input, char startSymbol, char stringDelimeter, char? endSymbol = null)
        {
            // If no end symbol is provided, the start symbol is used for both starting and ending the enclosement
            endSymbol ??= startSymbol;

            List<string> sections = [];
            StringBuilder remainder = new();
            StringBuilder sectionBuilder = new();
            List<int> startIndexes = [];

            // If we are not searching for strings, any instances of start or end symbols enclosed within string delimeters are to be ignored
            bool ignoreStringContents = startSymbol != stringDelimeter;
            bool insideString = false;
            bool insideSection = false;

            // Iterate through the input string and extract sections enclosed with start and end symbols
            for(int i = 0; i < input.Length; i++)
            {
                char currentCharacter = input[i];
                if (currentCharacter == stringDelimeter)
                {
                    HandleStringDelimiter(ref insideString, ref insideSection, ignoreStringContents, sectionBuilder, sections, startIndexes, i);
                }
                 
                if (ignoreStringContents && (currentCharacter == startSymbol && !insideString))
                {
                    startIndexes.Add(i);
                    insideSection = true;
                }
                else if (ignoreStringContents && (currentCharacter == endSymbol && !insideString))
                {
                    insideSection = false;
                    sections.Add(sectionBuilder.ToString());
                    sectionBuilder.Clear();
                }
                else if (insideSection)
                {
                    sectionBuilder.Append(currentCharacter);
                }
                else
                {
                    remainder.Append(currentCharacter);
                }
            }

            return new ExtractSectionResult([..sections], remainder.ToString(), [..startIndexes]);
        }

        /// <summary>
        /// Determines whether a string is in a list format.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <param name="listDelimeter">The delimeter that separates the list items</param>
        /// <param name="stringDelimeter">The delimeter that encloses the string</param>
        /// <returns>Returns a boolean which is true if the input string is in a list format</returns>
        internal static bool IsStringListFormat(string input, char listDelimeter, char stringDelimeter)
        {
            List<string> items = GetListItemsFromString(input, listDelimeter, stringDelimeter);
            if (items.Count <= 1)
            {
                return false;
            }
            else if (Array.TrueForAll([..items], x => x.TrimStart().StartsWith(stringDelimeter) && x.TrimEnd().EndsWith(stringDelimeter)))
            {                
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Splits a string into a list of items using a list delimeter and a string delimeter.
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="listDelimeter">Character used for separating list items</param>
        /// <param name="stringDelimeter">Character used for starting and ending a string</param>
        /// <returns>List of strings that represent the listed items</returns>
        internal static List<string> GetListItemsFromString(string input, char listDelimeter, char stringDelimeter)
        {
            List<string> items = [];
            StringBuilder itemBuilder = new();
            bool insideString = false;
            for (int i = 0; i < input.Length; i++)
            {
                char currentCharacter = input[i];
                if (currentCharacter == stringDelimeter)
                {
                    insideString = !insideString;
                }
                if (currentCharacter == listDelimeter && !insideString)
                {
                    items.Add(itemBuilder.ToString());
                    itemBuilder.Clear();
                }
                else
                {
                    itemBuilder.Append(currentCharacter);
                }
            }
            if (itemBuilder.Length > 0)
            {
                items.Add(itemBuilder.ToString());
            }

            return items;
        }

        /// <summary>
        /// Determines whether a string is in a number format.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <returns>Returns a boolean which is true if the input string is in a number format</returns>
        internal static bool IsNumberFormat(string input)
        {
            const int maxNumberLength = 29;
            if (input.Length > maxNumberLength)
            {
                return false;
            }
            if (!decimal.TryParse(input, out decimal _))
            {
                return false;
            }

            // Create a regex pattern to match valid number format
            const string pattern = @"^-?(\d+\.?\d*|\.\d+)$";

            try
            {
                return Regex.IsMatch(input, pattern, RegexOptions.Singleline, timeout);
            }
            catch (RegexMatchTimeoutException)
            {
                // If the regex times out, we can't be sure if the input is a valid number
                return false;
            }
        }

        /// <summary>
        /// Determines the type of a value from a string.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <param name="conf">Object that contains the symbols and tokens for structuring the file syntax.</param>
        /// <returns>Returns a <see cref="ValueType"/> object that represents the type of the value in the input string. If the type cannot be determined, null is returned.</returns>
        internal static ValueType? GetValueTypeFromString(string input, PxFileConfiguration conf)
        {
            if (GetTimeValueFormat(input, out ValueType? valueFormat, conf))
            {
                return valueFormat;
            }
            else if (IsStringListFormat(input, conf.Symbols.Value.ListSeparator, conf.Symbols.Key.StringDelimeter))
            {
                return ValueType.ListOfStrings;
            }
            else if (IsDateTimeFormat(input, conf))
            {
                return ValueType.DateTime;
            }
            else if (IsStringFormat(input, conf.Symbols.Key.StringDelimeter))
            {
                return ValueType.StringValue;
            }
            else if (input == conf.Tokens.Booleans.Yes || input == conf.Tokens.Booleans.No)
            {
                return ValueType.Boolean;
            }
            else if (IsNumberFormat(input))
            {
                return ValueType.Number;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Cleans a string from new line characters and quotation marks.
        /// </summary>
        /// <param name="input">The input string to clean</param>
        /// <param name="conf">The configuration for the PX file.</param>
        /// <returns>Returns a string that is the input string cleaned from new line characters and quotation marks</returns>
        internal static string CleanString(string input, PxFileConfiguration conf)
        {
            char[] charactersToTrim = [
                CharacterConstants.CHARCARRIAGERETURN,
                conf.Symbols.Linebreak,
                conf.Symbols.Key.StringDelimeter
            ];

            return input.Trim(charactersToTrim);
        }

        /// <summary>
        /// Gets the indeces of the enclosing characters in a string.
        /// </summary>
        /// <param name="input">String to process</param>
        /// <param name="startSymbol">Symbol that starts an enclosement</param>
        /// <param name="endSymbol">Optional symbol that closes the enclosement. If not provided, startSymbol will be used for both.</param>
        /// <returns>A dictionary in which keys are the indeces of opening characters and values are the corresponding closing characters.</returns>
        internal static Dictionary<int, int> GetEnclosingCharacterIndexes(string input, char startSymbol, char? endSymbol = null)
        {
            Dictionary<int, int> enclosingCharacterIndeces = [];
            endSymbol ??= startSymbol;
            int startIndex = input.IndexOf(startSymbol);
            while(startIndex != -1)
            {
                int endIndex = input.IndexOf((char)endSymbol, startIndex + 1);
                if (endIndex == -1)
                {
                    break;
                }
                enclosingCharacterIndeces.Add(startIndex, endIndex);
                startIndex = input.IndexOf(startSymbol, endIndex + 1);
            }
            return enclosingCharacterIndeces;
        }

        /// <summary>
        /// Finds the keyword separators in a string. Only indeces that are not enclosed within strings are returned.
        /// </summary>
        /// <param name="input">The input string to search</param>
        /// <param name="conf">The configuration for the PX file.</param>
        /// <returns>An array of integers representing the indeces of keyword separators which are not enclosed by string delimeters from the input string</returns>
        internal static int[] FindKeywordSeparatorIndeces(string input, PxFileConfiguration conf)
        {
            Dictionary<int, int> stringIndeces = GetEnclosingCharacterIndexes(input, conf.Symbols.Key.StringDelimeter);
            List<int> separators = [];

            // Find all indeces of SyntaxConf.Symbols.KeywordSeparator in the input string that are not enclosed within strings using FindSymbolIndex method
            int separatorIndex = -1;
            do
            {
                separatorIndex = FindSymbolIndex(input, conf.Symbols.KeywordSeparator, stringIndeces, separatorIndex + 1);
                if (separatorIndex != -1)
                {
                    separators.Add(separatorIndex);
                }
            }
            while (separatorIndex != -1);

            return [..separators];
        }

        /// <summary>
        /// Gets the position of a character as a line and character index.
        /// </summary>
        /// <param name="startLine">The line index of the first character of the entry</param>
        /// <param name="characterIndex">The index of the character starting from the entry start</param>
        /// <param name="lineChangeIndexes">An array of integers representing the indexes of line changes in the entry</param>
        /// <returns>A key-value pair where the key is the line index and the value is the character index</returns>
        public static KeyValuePair<int, int> GetLineAndCharacterIndex(int startLine, int characterIndex, int[] lineChangeIndexes)
        {
            int lineIndex = Array.FindLastIndex(lineChangeIndexes, x => x < characterIndex);
            if (lineIndex == -1)
            {
                lineIndex = 0;
            }
            else
            { 
                characterIndex -= lineChangeIndexes[lineIndex];
                lineIndex += 1;
            }
            lineIndex += startLine;
            return new KeyValuePair<int, int>(lineIndex, characterIndex);
        }

        /// <summary>
        /// Finds the index of the first occurence of a substring in a string. If the substring is enclosed within a string, the search is repeated until the substring is found outside of the string.
        /// </summary>
        /// <param name="input">The input string to search</param>
        /// <param name="substrings">An array of substrings to search for</param>
        /// <param name="stringDelimeter">The delimeter that encloses a string</param>
        /// <returns>Returns the index of the first occurence of the substring in the input string</returns>
        internal static int FirstSubstringIndexFromString(string input, string[] substrings, char stringDelimeter)
        {
            Dictionary<int, int> stringSections = GetEnclosingCharacterIndexes(input, stringDelimeter);
            int firstIndex = -1;
            foreach (string substring in substrings)
            {
                int index = input.IndexOf(substring, StringComparison.Ordinal);
                if (
                    index != -1 && 
                    (index < firstIndex || firstIndex == -1) &&
                    !stringSections.Any(x => x.Key < index && x.Value > index))
                {
                    firstIndex = index;
                }
            }
            return firstIndex;
        }

        /// <summary>
        /// Finds symbol index in a string. If the symbol is enclosed within a string, the search is repeated until the symbol is found outside of the string.
        /// </summary>
        /// <param name="input">The input string to search</param>
        /// <param name="symbol">The symbol to search for</param>
        /// <param name="stringIndexes">A dictionary in which keys are the indeces of opening characters and values are the corresponding closing characters.</param>
        /// <param name="startSearchIndex">The index from which to start the search</param>
        /// <returns>Returns the index of the symbol in the input string</returns>
        internal static int FindSymbolIndex(string input, char symbol, Dictionary<int, int> stringIndexes, int startSearchIndex = 0)
        {
            // If there's no string sections to be excluded from the search, we can just use the regular IndexOf method
            if (stringIndexes.Count == 0)
            {
                return input.IndexOf(symbol, startSearchIndex);
            }

            int symbolIndex = -1;
            int searchIndex = startSearchIndex;
            do
            {
                symbolIndex = input.IndexOf(symbol, searchIndex);
                searchIndex = symbolIndex + 1;
                if (symbolIndex == -1)
                {
                    break;
                }
                // If the symbol is enclosed within a string, we need to search again
                if (stringIndexes.Any(i => i.Key < symbolIndex && i.Value > symbolIndex))
                {
                    symbolIndex = -1;
                }
            }
            while (symbolIndex == -1); 

            return symbolIndex;
        }

        /// <summary>
        /// Determines the validity of line changes in a string or list of strings type value.
        /// </summary>
        /// <param name="value">String or list of strings to be validated</param>
        /// <param name="conf">PX file configuration object</param>
        /// <param name="type">Type of the value to be validated. This validation is only relevant to strings or list of strings</param>
        /// <returns></returns>
        internal static int GetLineChangesValidity(string value, PxFileConfiguration conf, ValueType type)
        {
            bool insideString = false;
            for (int i = 0; i < value.Length; i++)
            {
                char currentCharacter = value[i];
                if (currentCharacter == conf.Symbols.Key.StringDelimeter)
                {
                    insideString = !insideString;
                }
                if (!insideString && currentCharacter == conf.Symbols.Linebreak && i > 1)
                {
                    char symbolBefore = type is ValueType.ListOfStrings ?
                        conf.Symbols.Key.ListSeparator :
                        conf.Symbols.Key.StringDelimeter;

                    // In case of Windows linebreak, check if the character before the linebreak is the correct symbol
                    char characterToInspect = value[i - 1] != CharacterConstants.CARRIAGERETURN ? value[i - 1] : value[i - 2];

                    if (characterToInspect != symbolBefore)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
      
        private static bool GetTimeValueFormat(string input, out ValueType? valueFormat, PxFileConfiguration conf)
        {
            string timeIntervalIndicator = conf.Tokens.Time.TimeIntervalIndicator;

            // Value has to start with the time interval indicator (TLIST by default)
            if (!input.StartsWith(timeIntervalIndicator, StringComparison.Ordinal))
            {
                valueFormat = null;
                return false;
            }

            ExtractSectionResult intervalSection = ExtractSectionFromString(
                input, 
                conf.Symbols.Value.TimeSeriesIntervalStart, 
                conf.Symbols.Key.StringDelimeter,
                conf.Symbols.Value.TimeSeriesIntervalEnd);
            // There can only be one time interval specifier section
            if (intervalSection.Sections.Length != 1)
            {
                valueFormat = null;
                return false;
            }
            // Depending on the format, interval section may have the time range specified. Interval token is always the first part of the section
            string[] splitIntervalSection = intervalSection.Sections[0].Split(conf.Symbols.Value.ListSeparator);
            string intervalToken = splitIntervalSection[0];
            string? timeRange = splitIntervalSection.Length == 2 ? splitIntervalSection[1] : null;
            if (!conf.Tokens.Time.TimeIntervalTokens.Contains(intervalToken))
            {
                valueFormat = null;
                return false;
            }
            string remainder = intervalSection.Remainder.Remove(0, timeIntervalIndicator.Length);
            valueFormat = timeRange is not null ? GetTimeValueRangeFormat(timeRange, intervalToken, conf) : GetTimeValueSeriesFormat(remainder, intervalToken, conf);
            return valueFormat is not null;
        }

        private static ValueType? GetTimeValueSeriesFormat(string input, string timeInterval, PxFileConfiguration conf)
        {
            char listSeparator = conf.Symbols.Value.ListSeparator;
            if (input.Length == 0 || input[0] != listSeparator)
            {
                return null;
            }

            // Remove preceding list separator from input
            input = input.Remove(0, 1);
            if (!IsStringListFormat(input, listSeparator, conf.Symbols.Value.StringDelimeter))
            {
                return null;
            }

            // Time value series should be a list of time values
            string[] timeValSeries = input.Split(listSeparator);
            if (Array.TrueForAll(timeValSeries, x => IsValidTimestampFormat(x, timeInterval, conf)))
            {
                return ValueType.TimeValSeries;
            }
            else
            {
                return null;
            }
        }

        private static bool IsValidTimestampFormat(string input, string timeInterval, PxFileConfiguration conf)
        {
            input = input
                .Replace(CharacterConstants.CHARCARRIAGERETURN.ToString(), "")
                .Replace(conf.Symbols.Linebreak.ToString(), "")
                .Replace(CharacterConstants.CHARSPACE.ToString(), "");
            
            input = CleanString(input, conf);
                

            // Timestamp should match with a regex pattern for the given time interval
            Regex regex = GetRegexForTimeInterval(timeInterval, conf);

            try
            {
                return regex.IsMatch(input);
            }
            catch (RegexMatchTimeoutException)
            {
                // If the regex times out, we can't be sure if the input is a valid timestamp
                return false;
            }
        }

        private static Regex GetRegexForTimeInterval(string timeInterval, PxFileConfiguration conf)
        {
            if (!regexPatterns.TryGetValue(timeInterval, out Regex? regex))
            {
                string pattern = GetPatternForTimeInterval(timeInterval, conf);
                regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline, timeout);
                regexPatterns[timeInterval] = regex;
            }

            return regex;
        }

        private static string GetPatternForTimeInterval(string timeInterval, PxFileConfiguration conf)
        {
            return timeInterval switch
            {
                string interval when interval == conf.Tokens.Time.YearInterval => @"^\d{4}$",
                string interval when interval == conf.Tokens.Time.HalfYearInterval => @"^\d{4}[1-4]$",
                string interval when interval == conf.Tokens.Time.TrimesterInterval => @"^\d{4}[1-4]$",
                string interval when interval == conf.Tokens.Time.QuarterYearInterval => @"^\d{4}[1-4]$",
                string interval when interval == conf.Tokens.Time.MonthInterval => @"^\d{4}(0[1-9]|1[0-2])$",
                string interval when interval == conf.Tokens.Time.WeekInterval => @"^\d{4}(0[1-9]|[1-4][0-9]|5[0-2])$",
                string interval when interval == conf.Tokens.Time.DayInterval => @"^\d{4}(0[1-9]|1[0-2])(0[1-9]|[1-2][0-9]|3[0-1])$",
                _ => throw new ArgumentException("Invalid time interval", timeInterval)
            };
        }

        private static ValueType? GetTimeValueRangeFormat(string input, string timeInterval, PxFileConfiguration conf)
        {
            input = input.TrimStart();
            // Time range is split in to start and end by the time series limits separator
            string[] timeValRange = input.Split(conf.Symbols.Value.TimeSeriesLimitsSeparator);
            if (
                timeValRange.Length != 2 ||
                !input.StartsWith(conf.Symbols.Key.StringDelimeter) || 
                !input.EndsWith(conf.Symbols.Key.StringDelimeter))
            {
                return null;
            }
            // Time range parts should be valid timestamps
            if (IsValidTimestampFormat(CleanString(timeValRange[0], conf), timeInterval, conf) &&
                IsValidTimestampFormat(CleanString(timeValRange[1], conf), timeInterval, conf))
            {
                return ValueType.TimeValRange;
            }
            else
            {
                return null;
            }
        }

        private static void HandleStringDelimiter(
            ref bool insideString,
            ref bool insideSection,
            bool ignoreStringContents,
            StringBuilder sectionBuilder,
            List<string> sections, 
            List<int> startIndexes, 
            int i)
        {
            insideString = !insideString;
            if (!ignoreStringContents)
            {
                insideSection = !insideSection;
                if (!insideSection)
                {
                    sections.Add(sectionBuilder.ToString());
                    sectionBuilder.Clear();
                }
                else
                {
                    startIndexes.Add(i);
                }
            }
        }

        private static bool IsStringFormat(string input, char stringDelimeter)
        {
            return input.StartsWith(stringDelimeter) && input.EndsWith(stringDelimeter);
        }

        private static bool IsDateTimeFormat(string input, PxFileConfiguration conf)
        {
            if (!IsStringFormat(input, conf.Symbols.Key.StringDelimeter)) return false;

            return DateTime.TryParseExact(
                input.Trim(conf.Symbols.Key.StringDelimeter),
                conf.Symbols.Value.DateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _);
        }
    }
}
