using PxUtils.PxFile;
using System.Globalization;
using System;

namespace PxUtils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents the result of extracting sections from a string. This struct contains an array of the extracted sections and the remainder of the string after extraction.
    /// </summary>
    /// <param name="parameters">The sections extracted from the string.</param>
    /// <param name="remainder">The remainder of the string after extraction.</param>
    public readonly struct ExtractSectionResult(string[] parameters, string remainder)
    {
        /// <summary>
        /// Gets the sections extracted from the string.
        /// </summary>
        public string[] Sections { get; } = parameters;

        /// <summary>
        /// Gets the remainder of the string after extraction.
        /// </summary>
        public string Remainder { get; } = remainder;
    }

    /// <summary>
    /// Provides a collection of helper methods used during the syntax validation process. These methods include functionality for extracting sections from a string, checking the format of a string, and determining the type of a value from a string.
    /// </summary>
    public static class SyntaxValidationUtilityMethods
    {
        /// <summary>
        /// Determines whether a string contains more than one section enclosed with given symbols.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <param name="startSymbol">Symbol that starts enclosement</param>
        /// <param name="endSymbol">Symbols that closes the enclosement</param>
        /// <returns>Returns a boolean which is true if the input string contains more than one section</returns>
        public static bool HasMoreThanOneSection(string input, char startSymbol, char endSymbol)
        {
            ExtractSectionResult searchResult = ExtractSectionFromString(input, startSymbol, endSymbol);
            return searchResult.Sections.Length > 1;
        }

        /// <summary>
        /// Extracts a section from a string enclosed with given symbols.
        /// </summary>
        /// <param name="input">The input string to extract from</param>
        /// <param name="startSymbol">Symbol that starts enclosement</param>
        /// <param name="optionalEndSymbol">Optional symbol that closes the enclosement. If none given, startSymbol is used for both starting and ending the enclosement</param>
        /// <return>Returns an <see cref="ExtractSectionResult"/> object that contains the extracted sections and the string that remains after the operation</return>
        public static ExtractSectionResult ExtractSectionFromString(string input, char startSymbol, char? optionalEndSymbol = null)
        {
            char endSymbol = optionalEndSymbol ?? startSymbol;

            List<string> parameters = [];
            string remainder = input;
            int startIndex = remainder.IndexOf(startSymbol);
            int endIndex;

            while (startIndex != -1)
            {
                endIndex = remainder.IndexOf(endSymbol, startIndex + 1);
                if (endIndex == -1)
                {
                    break;
                }
                else
                {
                    string parameter = remainder[(startIndex + 1)..endIndex];
                    parameters.Add(parameter);
                    remainder = remainder.Remove(startIndex, endIndex - startIndex + 1);
                    startIndex = remainder.IndexOf(startSymbol);
                }
            }
            return new ExtractSectionResult([.. parameters], remainder);
        }

        /// <summary>
        /// Extracts specifier parts from a parameter string
        /// </summary>
        /// <param name="input">The input string to extract from</param>
        /// <param name="stringDelimeter">The delimeter that encloses the specifier part</param>
        /// <return>Returns an <see cref="ExtractSectionResult"/> object that contains the parameters and the string that remains after the operation</return>
        public static string[] GetSpecifiersFromParameter(string? input, char stringDelimeter)
        {
            if (input is null)
            {
                return [];
            }

            return ExtractSectionFromString(input, stringDelimeter).Sections;
        }

        /// <summary>
        /// Determines whether a string is in a date-time format.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <param name="stringDelimeter">The delimeter that encloses the string</param>
        /// <param name="dateTimeFormat">The format that the date-time should be in</param>
        /// <returns>Returns a boolean which is true if the input string is in the given date-time format</returns>
        public static bool IsDateTimeFormat(string input, char stringDelimeter, string dateTimeFormat)
        {
            if (!IsStringFormat(input, stringDelimeter))
            {
                return false;
            }
            return DateTime.TryParseExact(input.Trim(stringDelimeter), dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }

        /// <summary>
        /// Determines whether a string is in a string format.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <param name="stringDelimeter">The delimeter that encloses the string</param>
        /// <returns>Returns a boolean which is true if the input string is in a string format</returns>
        public static bool IsStringFormat(string input, char stringDelimeter)
        {
            return input.StartsWith(stringDelimeter) && input.EndsWith(stringDelimeter);
        }

        /// <summary>
        /// Determines whether a string is in a list format.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <param name="listDelimeter">The delimeter that separates the list items</param>
        /// <param name="stringDelimeter">The delimeter that encloses the string</param>
        /// <returns>Returns a boolean which is true if the input string is in a list format</returns>
        public static bool IsStringListFormat(string input, char listDelimeter, char stringDelimeter)
        {
            string[] list = input.Split(listDelimeter);
            if (list.Length <= 1)
            {
                return false;
            }
            else if (Array.TrueForAll(list, x => IsStringFormat(x.Trim(), stringDelimeter)))
            {                
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether a string is in a boolean format.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <param name="booleanYes">The string that represents a boolean true value</param>
        /// <param name="booleanNo">The string that represents a boolean false value</param>
        /// <returns>Returns a boolean which is true if the input string is in a boolean format</returns>
        public static bool IsBooleanFormat(string input, string booleanYes, string booleanNo)
        {
            return input == booleanYes || input == booleanNo;
        }

        /// <summary>
        /// Determines whether a string is in a number format.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <returns>Returns a boolean which is true if the input string is in a number format</returns>
        public static bool IsNumberFormat(string input)
        {
            char[] allowedSymbols = [
                '0',
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9',
                '.',
                '-'
            ];

            if (input.Any(c => !allowedSymbols.Contains(c)))
            {
                return false;
            }
            
            if (input.Count(c => c == '.') > 1 || input.Count(c => c == '-') > 1)
            {
                return false;
            }

            if (input.Contains('-') && !input.StartsWith('-'))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the line changes in a string are compliant with the syntax configuration.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file that this validation entry is part of. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
        /// <param name="isList">A boolean that is true if the input string is a list</param>
        /// <returns>Returns a boolean which is true if the line changes in the input string are compliant with the syntax configuration</returns>
        public static bool ValueLineChangesAreCompliant(string input, PxFileSyntaxConf syntaxConf, bool isList)
        {
            int lineChangeIndex = input.IndexOf(syntaxConf.Symbols.LineSeparator);
            while (lineChangeIndex != -1)
            {
                char symbolBefore = isList ? syntaxConf.Symbols.Key.ListSeparator : syntaxConf.Symbols.Key.StringDelimeter;
                if (input[lineChangeIndex - 1] != symbolBefore || input[lineChangeIndex + 1] != syntaxConf.Symbols.Key.StringDelimeter)
                {
                    return false;
                }
                lineChangeIndex = input.IndexOf(syntaxConf.Symbols.LineSeparator, lineChangeIndex + 1);
            }
            return true;
        }

        /// <summary>
        /// Determines the type of a value from a string.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file that this validation entry is part of. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
        /// <returns>Returns a <see cref="ValueType"/> object that represents the type of the value in the input string. If the type cannot be determined, null is returned.</returns>
        public static ValueType? GetValueTypeFromString(string input, PxFileSyntaxConf syntaxConf)
        {
            if (IsStringListFormat(input, syntaxConf.Symbols.Value.ListSeparator, syntaxConf.Symbols.Key.StringDelimeter))
            {
                return ValueType.List;
            }
            else if (IsDateTimeFormat(input, syntaxConf.Symbols.Value.StringDelimeter, syntaxConf.Symbols.Value.DateTimeFormat))
            {
                return ValueType.DateTime;
            }
            else if (IsStringFormat(input, syntaxConf.Symbols.Value.StringDelimeter))
            {
                return ValueType.String;
            }
            else if (IsBooleanFormat(input, syntaxConf.Tokens.Booleans.Yes, syntaxConf.Tokens.Booleans.No))
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
        public static string CleanString(string input)
        {
            if (input is null)
            {
                return string.Empty;
            }
            else
            {
                return input.Replace("\n", "").Replace("\"", "");
            }
        }
    }
}
