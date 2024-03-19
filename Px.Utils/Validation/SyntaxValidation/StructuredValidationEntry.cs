namespace PxUtils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents a key for a <see cref="StructuredValidationEntry"/>. A key consists of a keyword and two optional language and specifier strings.
    /// </summary>
    public readonly record struct ValidationEntryKey
    {
        /// <summary>
        /// Gets the keyword of the validation entry key.
        /// </summary>
        public string Keyword { get; }

        /// <summary>
        /// Gets the language of the validation entry key, if any.
        /// </summary>
        public string? Language { get; }

        /// <summary>
        /// Gets the first, dimension level specifier of the validation entry key, if any.
        /// </summary>
        public string? FirstSpecifier { get; }

        /// <summary>
        /// Gets the second, value level specifier of the validation entry key, if any.
        /// </summary>
        public string? SecondSpecifier { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationEntryKey"/> struct.
        /// </summary>
        /// <param name="keyword">The keyword of the validation entry key.</param>
        /// <param name="language">The language of the validation entry key, if any.</param>
        /// <param name="firstSpecifier">The first, dimension level specifier of the validation entry key, if any.</param>
        /// <param name="secondSpecifier">The second, value level specifier of the validation entry key, if any.</param>
        public ValidationEntryKey(string keyword, string? language = null, string? firstSpecifier = null, string? secondSpecifier = null)
        {
            Keyword = keyword;
            Language = language;
            FirstSpecifier = firstSpecifier;
            SecondSpecifier = secondSpecifier;
        }
    }

    /// <summary>
    /// Represents a structured validation entry. A structured validation entry is a type of validation entry that contains a key and a value. This class implements the <see cref="IValidationEntry"/> interface.
    /// </summary>
    /// <param name="line">The line number in the file where the validation entry is located.</param>
    /// <param name="character">The character position in the line where the validation entry starts.</param>
    /// <param name="file">The name of the file where the validation entry is located.</param>
    /// <param name="key">The key of the validation entry, represented by a <see cref="ValidationEntryKey"/> object.</param>
    /// <param name="value">The value of the validation entry.</param>
    public class StructuredValidationEntry(int line, int character, string file, ValidationEntryKey key, string value) : IValidationEntry
    {
        /// <summary>
        /// Gets the key part of the validation entry, represented by a <see cref="ValidationEntryKey"/> object.
        /// </summary>
        public ValidationEntryKey Key { get; } = key;

        /// <summary>
        /// Gets the value part of the validation entry.
        /// </summary>
        public string Value { get; } = value;

        /// <summary>
        /// Gets the line number in the file where the validation entry is located.
        /// </summary>
        public int Line { get; } = line;

        /// <summary>
        /// Gets the character position in the line where the validation entry starts.
        /// </summary>
        public int Character { get; } = character;

        /// <summary>
        /// Gets the name of the file where the validation entry is located.
        /// </summary>
        public string File { get; } = file;
    }
}
