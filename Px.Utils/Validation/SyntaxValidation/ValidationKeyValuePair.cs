using PxUtils.PxFile;

namespace PxUtils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents a key-value pair built from a PX file metadata entry. Key consists of the keyword and parameter sections, for example: KEYWORD[lang]("specifier"), while value is the value part of the entry.
    /// If the value part of the entry is split on multiple lines, all of it will be included in the value part since the line delimeter character (default is ';') is not used to separate the entries.
    /// Used for validation functions and building structured metadata entries. This class implements the <see cref="ValidationObject"/> interface.
    /// </summary>
    /// <param name="lines">The line numbers in the file where the entry is located in the PX file.</param>
    /// <param name="file">The name of the file where the validation entry is located.</param>
    /// <param name="keyValuePair">The key-value pair that this validation entry represents.</param>
    public class ValidationKeyValuePair(string file, KeyValuePair<string, string> keyValuePair, int keyStartLineIndex, int[] lineChangeIndexes, int valueStartIndex) : ValidationObject(file, keyStartLineIndex, lineChangeIndexes)
    {
        /// <summary>
        /// Contents of the entry, represented as a key-value pair.
        /// </summary>
        public KeyValuePair<string, string> KeyValuePair { get; } = keyValuePair;
        
        /// <summary>
        /// Index of the first character of the value part of the entry.
        /// </summary>
        public int ValueStartIndex { get; } = valueStartIndex;
    }
}
