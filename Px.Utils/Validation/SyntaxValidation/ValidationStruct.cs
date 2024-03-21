namespace PxUtils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents a key for a <see cref="ValidationStruct"/>. A key consists of a keyword and two optional language and specifier strings.
    /// </summary>
    public readonly record struct ValidationStructKey
    {
        /// <summary>
        /// Keyword of the entry
        /// </summary>
        public string Keyword { get; }

        /// <summary>
        /// Language of the entry, if any
        /// </summary>
        public string? Language { get; }

        /// <summary>
        /// First, dimension level specifier of the entry, if any.
        /// </summary>
        public string? FirstSpecifier { get; }

        /// <summary>
        /// Second, value level specifier of the entry, if any.
        /// </summary>
        public string? SecondSpecifier { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationStructKey"/> struct.
        /// </summary>
        /// <param name="keyword">Keyword of the entry.</param>
        /// <param name="language">Language of the entry, if any.</param>
        /// <param name="firstSpecifier">First, dimension level specifier of the entry, if any.</param>
        /// <param name="secondSpecifier">Second, value level specifier of the entry, if any.</param>
        public ValidationStructKey(string keyword, string? language = null, string? firstSpecifier = null, string? secondSpecifier = null)
        {
            Keyword = keyword;
            Language = language;
            FirstSpecifier = firstSpecifier;
            SecondSpecifier = secondSpecifier;
        }
    }

    /// <summary>
    /// Represents a structured validation object. A structured validation object contains a structured key and a value. This class implements the <see cref="ValidationObject"/> interface.
    /// </summary>
    /// <param name="line">The line number in the file where the entry is located.</param>
    /// <param name="character">The character position in the line where the entry starts.</param>
    /// <param name="file">The name of the file where the entry is located.</param>
    /// <param name="key">The key part of the entry, represented by a <see cref="ValidationStructKey"/> object.</param>
    /// <param name="value">The value part of the entry.</param>
    public class ValidationStruct(int line, int character, string file, ValidationStructKey key, string value) : ValidationObject(line, character, file)
    {
        /// <summary>
        /// The key part of the entry, represented by a <see cref="ValidationStructKey"/> object.
        /// </summary>
        public ValidationStructKey Key { get; } = key;

        /// <summary>
        /// The value part of the entry.
        /// </summary>
        public string Value { get; } = value;
    }
}
