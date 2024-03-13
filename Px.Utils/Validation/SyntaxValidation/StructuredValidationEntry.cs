using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation.SyntaxValidation
{
    public readonly record struct ValidationEntryKey
    {
        public string Keyword { get; }
        public string? Language { get; }
        public string? FirstSpecifier { get; }
        public string? SecondSpecifier { get; }

        public ValidationEntryKey(string keyword, string? language = null, string? firstSpecifier = null, string? secondSpecifier = null)
        {
            Keyword = keyword;
            Language = language;
            FirstSpecifier = firstSpecifier;
            SecondSpecifier = secondSpecifier;
        }
    }

    public class StructuredValidationEntry(int line, int character, string file, ValidationEntryKey key, string value) : ValidationEntry(line, character, file)
    {
        public ValidationEntryKey Key { get; } = key;

        public string Value { get; } = value;
    }
}
