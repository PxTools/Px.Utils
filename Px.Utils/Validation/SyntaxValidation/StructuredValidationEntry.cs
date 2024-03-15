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

    public class StructuredValidationEntry(int line, int character, string file, ValidationEntryKey key, string value) : IValidationEntry
    {
        public ValidationEntryKey Key { get; } = key;

        public string Value { get; } = value;
        public int Line { get; } = line;
        public int Character { get; } = character;
        public string File { get; } = file;
    }
}
