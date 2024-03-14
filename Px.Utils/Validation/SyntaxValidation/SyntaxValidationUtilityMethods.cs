using PxUtils.PxFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation.SyntaxValidation
{
    public struct ExtractSectionResult(string[] parameters, string remainder)
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
                    // TODO: Create a ValidationFeedbackItem for this
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

        public static string[] GetSpecifiersFromParameter(string? input, PxFileSyntaxConf syntaxConf)
        {
            if (input == null)
            {
                return [];
            }

            return ExtractSectionFromString(input, syntaxConf.Symbols.Key.StringDelimeter).Sections;
        }

        public static bool IsStringFormat(string input)
        {
            return input.StartsWith('"') && input.EndsWith('"');
        }

        public static bool IsStringList(string input)
        {
            string[] list = input.Split(",");
            if (list.Length <= 1)
            {
                return false;
            }
            else if (Array.TrueForAll(list, x => IsStringFormat(x.Trim())))
            {                
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsBooleanFormat(string input)
        {
            return input == "YES" || input == "NO";
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
    }
}
