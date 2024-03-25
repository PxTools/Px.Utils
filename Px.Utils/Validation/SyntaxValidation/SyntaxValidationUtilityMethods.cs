using PxUtils.PxFile;
using System.Globalization;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PxUtils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents the result of extracting sections from a string. This struct contains an array of the extracted sections and the remainder of the string after extraction.
    /// </summary>
    /// <param name="parameters">The sections extracted from the string.</param>
    /// <param name="remainder">The remainder of the string after extraction.</param>
    internal readonly struct ExtractSectionResult(string[] parameters, string remainder)
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
    internal static class SyntaxValidationUtilityMethods
    {
        /// <summary>
        /// Determines whether a string contains more than one section enclosed with given symbols.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <param name="startSymbol">Symbol that starts enclosement</param>
        /// <param name="syntaxConf">Object that contains the symbols and tokens for structuring the file syntax. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
        /// <param name="endSymbol">Symbols that closes the enclosement</param>
        /// <returns>Returns a boolean which is true if the input string contains more than one section</returns>
        internal static bool HasMoreThanOneSection(string input, char startSymbol, char endSymbol, PxFileSyntaxConf syntaxConf)
        {
            ExtractSectionResult searchResult = ExtractSectionFromString(input, startSymbol, syntaxConf.Symbols.Key.StringDelimeter, endSymbol);
            return searchResult.Sections.Length > 1;
        }

        /// <summary>
        /// Extracts a section from a string enclosed with given symbols.
        /// </summary>
        /// <param name="input">The input string to extract from</param>
        /// <param name="startSymbol">Symbol that starts enclosement</param>
        /// <param name="syntaxConf">Object that contains the symbols and tokens for structuring the file syntax. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
        /// <param name="endSymbol">Optional symbol that closes the enclosement. If none given, startSymbol is used for both starting and ending the enclosement</param>
        /// <return>Returns an <see cref="ExtractSectionResult"/> object that contains the extracted sections and the string that remains after the operation</return>
        internal static ExtractSectionResult ExtractSectionFromString(string input, char startSymbol, char stringDelimeter, char? endSymbol = null)
        {
            // If no end symbol is provided, the start symbol is used for both starting and ending the enclosement
            endSymbol ??= startSymbol;

            List<string> sections = [];
            string remainder = input;

            // If we are not searching for strings, any instances of start or end symbols enclosed within string delimeters are to be ignored
            Dictionary<int, int> stringDelimeterIndexes = [];
            if (startSymbol != stringDelimeter)
            {
                stringDelimeterIndexes =  GetEnclosingCharacterIndexes(input, stringDelimeter);
            }

            // Loop through the string and extract sections until no more start symbols are found
            int startIndex;
            do
            {
                startIndex = FindSymbolIndex(remainder, startSymbol, stringDelimeterIndexes);
                int endIndex = FindSymbolIndex(remainder, (char)endSymbol, stringDelimeterIndexes, startIndex + 1);
                if (endIndex == -1)
                {
                    break;
                }

                string section = remainder[(startIndex + 1)..endIndex];
                sections.Add(section);
                remainder = remainder.Remove(startIndex, endIndex - startIndex + 1);
            }
            while (startIndex != -1);

            return new ExtractSectionResult([.. sections], remainder);
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
            string[] list = input.Split(listDelimeter);
            if (list.Length <= 1)
            {
                return false;
            }
            else if (Array.TrueForAll(list, x => x.TrimStart().StartsWith(stringDelimeter) && x.TrimEnd().EndsWith(stringDelimeter)))
            {                
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether a string is in a number format.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <returns>Returns a boolean which is true if the input string is in a number format</returns>
        internal static bool IsNumberFormat(string input)
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
        internal static bool ValueLineChangesAreCompliant(string input, PxFileSyntaxConf syntaxConf, bool isList)
        {
            string trimmedInput = input.Replace(CharacterConstants.CarriageReturn.ToString(), "");
            if (trimmedInput.Contains(CharacterConstants.CarriageReturn))
            {
                Debug.WriteLine("This is impossible");
            }
            int lineChangeIndex = trimmedInput.IndexOf(CharacterConstants.LineFeed);
            while (lineChangeIndex != -1)
            {
                char symbolBefore = isList ? syntaxConf.Symbols.Key.ListSeparator : syntaxConf.Symbols.Key.StringDelimeter;
                if (trimmedInput[lineChangeIndex - 1] != symbolBefore || trimmedInput[lineChangeIndex + 1] != syntaxConf.Symbols.Key.StringDelimeter)
                {
                    return false;
                }
                lineChangeIndex = trimmedInput.IndexOf(CharacterConstants.LineFeed, lineChangeIndex + 1);
            }
            return true;
        }

        /// <summary>
        /// Determines the type of a value from a string.
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <param name="syntaxConf">Object that contains the symbols and tokens for structuring the file syntax. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
        /// <returns>Returns a <see cref="ValueType"/> object that represents the type of the value in the input string. If the type cannot be determined, null is returned.</returns>
        internal static ValueType? GetValueTypeFromString(string input, PxFileSyntaxConf syntaxConf)
        {
            if (IsStringListFormat(input, syntaxConf.Symbols.Value.ListSeparator, syntaxConf.Symbols.Key.StringDelimeter))
            {
                return ValueType.ListOfStrings;
            }
            else if (input.StartsWith(syntaxConf.Symbols.Key.StringDelimeter) && input.EndsWith(syntaxConf.Symbols.Key.StringDelimeter))
            {
                return ValueType.String;
            }
            else if (input == syntaxConf.Tokens.Booleans.Yes || input == syntaxConf.Tokens.Booleans.No)
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
        internal static string CleanString(string input, PxFileSyntaxConf syntaxConf)
        {
            return input
                .Trim(CharacterConstants.CarriageReturn)
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
        /// <param name="syntaxConf">The syntax configuration for the PX file. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
        /// <returns>An array of integers representing the indeces of keyword separators which are not enclosed by string delimeters from the input string</returns>
        internal static int[] FindKeywordSeparatorIndeces(string input, PxFileSyntaxConf syntaxConf)
        {
            Dictionary<int, int> stringIndeces = GetEnclosingCharacterIndexes(input, syntaxConf.Symbols.Key.StringDelimeter);
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
