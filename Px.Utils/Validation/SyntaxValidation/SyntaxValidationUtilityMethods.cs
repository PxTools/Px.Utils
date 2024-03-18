using PxUtils.PxFile;
using System.Globalization;
using System;

namespace PxUtils.Validation.SyntaxValidation
{
    public readonly struct ExtractSectionResult(string[] parameters, string remainder)
    {
        public string[] Sections { get; } = parameters;
        public string Remainder { get; } = remainder;
    }

    public static class SyntaxValidationUtilityMethods
    {
        public static bool HasMoreThanOneSection(string input, char startSymbol, char endSymbol)
        {
            ExtractSectionResult searchResult = ExtractSectionFromString(input, startSymbol, endSymbol);
            return searchResult.Sections.Length > 1;
        }

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

        public static string[] GetSpecifiersFromParameter(string? input, char stringDelimeter)
        {
            if (input is null)
            {
                return [];
            }

            return ExtractSectionFromString(input, stringDelimeter).Sections;
        }

        public static bool IsDateTimeFormat(string input, char stringDelimeter, string dateTimeFormat)
        {
            if (!IsStringFormat(input, stringDelimeter))
            {
                return false;
            }
            return DateTime.TryParseExact(input.Trim(stringDelimeter), dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }

        public static bool IsStringFormat(string input, char stringDelimeter)
        {
            return input.StartsWith(stringDelimeter) && input.EndsWith(stringDelimeter);
        }

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

        public static bool IsBooleanFormat(string input, string booleanYes, string booleanNo)
        {
            return input == booleanYes || input == booleanNo;
        }

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
