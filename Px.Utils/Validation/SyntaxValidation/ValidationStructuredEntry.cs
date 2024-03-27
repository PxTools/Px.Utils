using System.ComponentModel;

namespace PxUtils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents a key for a <see cref="ValidationStructuredEntry"/>. A key consists of a keyword and two optional language and specifier strings.
    /// </summary>
    public readonly record struct ValidationStructuredEntryKey
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
        /// Initializes a new instance of the <see cref="ValidationStructuredEntryKey"/> struct.
        /// </summary>
        /// <param name="keyword">Keyword of the entry.</param>
        /// <param name="language">Language of the entry, if any.</param>
        /// <param name="firstSpecifier">First, dimension level specifier of the entry, if any.</param>
        /// <param name="secondSpecifier">Second, value level specifier of the entry, if any.</param>
        public ValidationStructuredEntryKey(string keyword, string? language = null, string? firstSpecifier = null, string? secondSpecifier = null)
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
    /// <param name="lines">The line numbers in the file where the entry is located.</param>
    /// <param name="file">The name of the file where the entry is located.</param>
    /// <param name="key">The key part of the entry, represented by a <see cref="ValidationStructuredEntryKey"/> object.</param>
    /// <param name="value">The value part of the entry.</param>
    public class ValidationStructuredEntry(string file, ValidationStructuredEntryKey key, string value, int keyStartLineIndex, int[] lineChangeIndexes, int valueStartIndex) : ValidationObject(file, keyStartLineIndex, lineChangeIndexes)
    {
        /// <summary>
        /// The key part of the entry, represented by a <see cref="ValidationStructuredEntryKey"/> object.
        /// </summary>
        public ValidationStructuredEntryKey Key { get; } = key;

        /// <summary>
        /// The value part of the entry.
        /// </summary>
        public string Value { get; } = value;
        
        /// <summary>
        /// Index of the first character of the value in the entry.
        /// </summary>
        public int ValueStartIndex { get; } = valueStartIndex;
    }
}
