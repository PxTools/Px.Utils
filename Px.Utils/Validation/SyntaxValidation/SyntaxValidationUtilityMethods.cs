﻿using PxUtils.PxFile;
using System.Globalization;
using System;
using System.Linq;
using System.Text.RegularExpressions;

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
        private const int DEFAULT_TIMEOUT = 1;

        /// <summary>
        /// Determines whether a string contains more than one section enclosed with given symbols.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <param name="startSymbol">Symbol that starts enclosement</param>
        /// <param name="syntaxConf">Object that contains the symbols and tokens for structuring the file syntax. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
        /// <param name="endSymbol">Symbols that closes the enclosement</param>
        /// <returns>Returns a boolean which is true if the input string contains more than one section</returns>
        public static bool HasMoreThanOneSection(string input, char startSymbol, char endSymbol, PxFileSyntaxConf syntaxConf)
        {
            ExtractSectionResult searchResult = ExtractSectionFromString(input, startSymbol, syntaxConf, endSymbol);
            return searchResult.Sections.Length > 1;
        }

        /// <summary>
        /// Extracts a section from a string enclosed with given symbols.
        /// </summary>
        /// <param name="input">The input string to extract from</param>
        /// <param name="startSymbol">Symbol that starts enclosement</param>
        /// <param name="syntaxConf">Object that contains the symbols and tokens for structuring the file syntax. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
        /// <param name="optionalEndSymbol">Optional symbol that closes the enclosement. If none given, startSymbol is used for both starting and ending the enclosement</param>
        /// <return>Returns an <see cref="ExtractSectionResult"/> object that contains the extracted sections and the string that remains after the operation</return>
        public static ExtractSectionResult ExtractSectionFromString(string input, char startSymbol, PxFileSyntaxConf syntaxConf, char? optionalEndSymbol = null)
        {
            char endSymbol = optionalEndSymbol ?? startSymbol;

            List<string> sections = [];
            string remainder = input;

            Dictionary<int, int> stringIndeces = [];
            // If we are not searching for strings, any instances of start or end symbols enclosed within string delimeters are to be ignored
            if (startSymbol != syntaxConf.Symbols.Key.StringDelimeter)
            {
                stringIndeces =  GetEnclosingCharacterIndeces(input, syntaxConf.Symbols.Key.StringDelimeter);
            }

            int startIndex = FindSymbolIndex(remainder, startSymbol, stringIndeces);

            while (startIndex != -1)
            {
                int endIndex = FindSymbolIndex(remainder, endSymbol, stringIndeces, startIndex + 1);
                if (endIndex == -1)
                {
                    break;
                }

                string section = remainder[(startIndex + 1)..endIndex];
                sections.Add(section);
                remainder = remainder.Remove(startIndex, endIndex - startIndex + 1);
                startIndex = FindSymbolIndex(remainder, startSymbol, stringIndeces);
            }
            return new ExtractSectionResult([.. sections], remainder);
        }

        /// <summary>
        /// Extracts specifier parts from a parameter string
        /// </summary>
        /// <param name="input">The input string to extract from</param>
        /// <param name="stringDelimeter">The delimeter that encloses the specifier part</param>
        /// <param name="syntaxConf">Object that contains the symbols and tokens for structuring the file syntax. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
        /// <return>Returns an <see cref="ExtractSectionResult"/> object that contains the sections and the string that remains after the operation</return>
        public static string[] GetSpecifiersFromParameter(string? input, char stringDelimeter, PxFileSyntaxConf syntaxConf)
        {
            if (input is null)
            {
                return [];
            }

            return ExtractSectionFromString(input, stringDelimeter, syntaxConf).Sections;
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
            if (input.Length > 29)
            {
                return false;
            }
            if (!decimal.TryParse(input, out decimal _))
            {
                return false;
            }

            // Create a regex pattern to match valid number format
            string pattern = @"^-?(\d+\.?\d*|\.\d+)$";


            TimeSpan timeout = TimeSpan.FromSeconds(1);
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
        /// Determines whether the line changes in a string are compliant with the syntax configuration.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <param name="syntaxConf">Object that contains the symbols and tokens for structuring the file syntax. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
        /// <param name="isList">A boolean that is true if the input string is a list</param>
        /// <returns>Returns a boolean which is true if the line changes in the input string are compliant with the syntax configuration</returns>
        public static bool ValueLineChangesAreCompliant(string input, PxFileSyntaxConf syntaxConf, bool isList)
        {
            int lineChangeIndex = GetNextLineChangeIndex(input, syntaxConf);
            while (lineChangeIndex != -1)
            {
                char symbolBefore = isList ? syntaxConf.Symbols.Key.ListSeparator : syntaxConf.Symbols.Key.StringDelimeter;
                if (input[lineChangeIndex - 1] != symbolBefore || input[lineChangeIndex + 1] != syntaxConf.Symbols.Key.StringDelimeter)
                {
                    return false;
                }
                lineChangeIndex = GetNextLineChangeIndex(input, syntaxConf, lineChangeIndex + 1);
            }
            return true;
        }

        /// <summary>
        /// Determines the type of a value from a string.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <param name="syntaxConf">Object that contains the symbols and tokens for structuring the file syntax. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
        /// <returns>Returns a <see cref="ValueType"/> object that represents the type of the value in the input string. If the type cannot be determined, null is returned.</returns>
        public static ValueType? GetValueTypeFromString(string input, PxFileSyntaxConf syntaxConf)
        {
            if (IsStringListFormat(input, syntaxConf.Symbols.Value.ListSeparator, syntaxConf.Symbols.Key.StringDelimeter))
            {
                return ValueType.ListOfStrings;
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
        /// <param name="input">The input string to clean</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
        /// <returns>Returns a string that is the input string cleaned from new line characters and quotation marks</returns>
        public static string CleanString(string input, PxFileSyntaxConf syntaxConf)
        {
            return input
                .Trim(syntaxConf.Symbols.CarriageReturn)
                .Trim(syntaxConf.Symbols.Linebreak)
                .Trim(syntaxConf.Symbols.Key.StringDelimeter);
        }

        /// <summary>
        /// Gets the indeces of the enclosing characters in a string.
        /// </summary>
        /// <param name="input">String to process</param>
        /// <param name="startSymbol">Symbol that starts an enclosement</param>
        /// <param name="endSymbol">Optional symbol that closes the enclosement. If not provided, startSymbol will be used for both.</param>
        /// <returns>A dictionary in which keys are the indeces of opening characters and values are the corresponding closing characters.</returns>
        public static Dictionary<int, int> GetEnclosingCharacterIndeces(string input, char startSymbol, char? endSymbol = null)
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
        /// <param name="syntaxConf">The syntax configuration for the PX file. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
        /// <returns>An array of integers representing the indeces of keyword separators which are not enclosed by string delimeters from the input string</returns>
        public static int[] FindKeywordSeparatorIndeces(string input, PxFileSyntaxConf syntaxConf)
        {
            Dictionary<int, int> stringIndeces = GetEnclosingCharacterIndeces(input, syntaxConf.Symbols.Key.StringDelimeter);
            List<int> separators = [];

            // Find all indeces of syntaxConf.Symbols.KeywordSeparator in the input string that are not enclosed within strings using FindSymbolIndex method
            int separatorIndex = -1;
            do
            {
                separatorIndex = FindSymbolIndex(input, syntaxConf.Symbols.KeywordSeparator, stringIndeces, separatorIndex + 1);
                if (separatorIndex != -1)
                {
                    separators.Add(separatorIndex);
                }
            }
            while (separatorIndex != -1);

            return [..separators];
        }

        /// <summary>
        /// Finds the illegal characters in a string using a regular expression pattern.
        /// </summary>
        /// <param name="input">The input string to search</param>
        /// <param name="illegalCharacters">An array of characters that are considered illegal</param>
        /// <param name="foundIllegalCharacters">An array of characters that were found in the input string</param>
        /// <returns>Returns a boolean which is true if the search was successful. If the search times out, false is returned.</returns>
        public static bool FindIllegalCharactersInString(string input, char[] illegalCharacters, out char[] foundIllegalCharacters, int customTimeout = DEFAULT_TIMEOUT)
        {
            string processedCharacters = ProcessCharsForRegexPattern(illegalCharacters);
            string pattern = $"[{processedCharacters}]";

            TimeSpan timeout = TimeSpan.FromSeconds(customTimeout);
            try
            {
                MatchCollection matches = Regex.Matches(input, pattern, RegexOptions.Singleline, timeout);
                foundIllegalCharacters = matches.Cast<Match>().Select(m => m.Value[0]).ToArray();
                return true;
            }
            catch (RegexMatchTimeoutException)
            {
                foundIllegalCharacters = [];
                return false;
            }
        }
        private static int GetNextLineChangeIndex(string input, PxFileSyntaxConf syntaxConf, int startIndex = 0)
        {
            int lineBreakIndex = input.IndexOf(syntaxConf.Symbols.Linebreak, startIndex);
            int carriageReturnIndex = input.IndexOf(syntaxConf.Symbols.CarriageReturn, startIndex);

            // If neither character was found, return -1
            if (lineBreakIndex == -1 && carriageReturnIndex == -1)
            {
                return -1;
            }

            // If only one character was found, return its index
            if (lineBreakIndex == -1)
            {
                return carriageReturnIndex;
            }
            if (carriageReturnIndex == -1)
            {
                return lineBreakIndex;
            }

            // If both characters were found, return the index of the one that appears first
            return Math.Min(lineBreakIndex, carriageReturnIndex);
        }

        private static string ProcessCharsForRegexPattern(char[] illegalCharacters)
        {
            string[] specialCharacters = [ "\\", "^", "$", ".", "|", "?", "*", "+", "(", ")", "[", "]", "[", "]" ];
            string[] processedCharacters = new string[illegalCharacters.Length];

            for (int i = 0; i < illegalCharacters.Length; i++)
            {
                string character = illegalCharacters[i].ToString();

                if (specialCharacters.Contains(character))
                {
                    // If the character is a special character, escape it
                    character = "\\" + character;
                }

                processedCharacters[i] = character;
            }

            return string.Join("", processedCharacters);
        }

        private static int FindSymbolIndex(string remainder, char symbol, Dictionary<int, int> stringIndeces, int startSearchIndex = 0)
        {
            // If there's no string sections to be excluded from the search, we can just use the regular IndexOf method
            if (stringIndeces.Count == 0)
            {
                return remainder.IndexOf(symbol, startSearchIndex);
            }

            int symbolIndex = -1;
            int searchIndex = startSearchIndex;
            do
            {
                symbolIndex = remainder.IndexOf(symbol, searchIndex);
                searchIndex = symbolIndex + 1;
                if (symbolIndex == -1)
                {
                    break;
                }
                // If the symbol is enclosed within a string, we need to search again
                if (stringIndeces.Any(i => i.Key < symbolIndex && i.Value > symbolIndex))
                {
                    symbolIndex = -1;
                }
            }
            while (symbolIndex == -1); 

            return symbolIndex;
        }
    }
}
