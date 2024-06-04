using Px.Utils.PxFile;

namespace Px.Utils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents a key-value pair built from a PX file metadata entry. Key consists of the keyword and parameter sections separated by = by default.
    /// If the value part of the entry is split on multiple lines, all of it will be included.
    /// Used for validation functions and building structured metadata entries. This class implements the <see cref="ValidationObject"/> interface.
    /// </summary>
    /// <param name="file">The name of the file where the validation entry is located.</param>
    /// <param name="keyValuePair">The key-value pair that this validation entry represents.</param>
    /// <param name="keyStartLineIndex">Index of the line where the entry starts.</param>
    /// <param name="lineChangeIndexes">Character indexes of the line changes in the entry starting from the entry start.</param>
    /// <param name="valueStartIndex">Index of the first character of the value in the entry.</param>
    public class ValidationKeyValuePair(
        string file,
        KeyValuePair<string, string> keyValuePair,
        int keyStartLineIndex,
        int[] lineChangeIndexes, 
        int valueStartIndex) : ValidationObject(file, keyStartLineIndex, lineChangeIndexes)
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
