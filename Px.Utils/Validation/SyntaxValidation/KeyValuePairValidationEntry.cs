using PxUtils.PxFile;

namespace PxUtils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents a key-value pair of a PX file metadata entry. Key consists of the keyword and parameter sections, for example: KEYWORD[lang]("specifier"), while value is the value part of the entry.
    /// If the value part of the entry is split on multiple lines, all of it will be included in the value part since the line delimeter character (default is ';') is not used to separate the entries.
    /// Used for validation functions and building structured metadata entries. This class implements the <see cref="ValidationEntry"/> interface.
    /// </summary>
    /// <param name="line">The line number in the file where the validation entry is located.</param>
    /// <param name="character">The character position in the line where the validation entry starts.</param>
    /// <param name="file">The name of the file where the validation entry is located.</param>
    /// <param name="keyValueEntry">The key-value pair that this validation entry represents.</param>
    /// <param name="syntaxConf">The syntax configuration for the PX file that this validation entry is part of. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
    public class KeyValuePairValidationEntry(int line, int character, string file, KeyValuePair<string, string> keyValueEntry) : ValidationEntry(line, character, file)
    {
        /// <summary>
        /// Gets the key-value pair that this validation entry represents.
        /// </summary>
        public KeyValuePair<string, string> KeyValueEntry { get; } = keyValueEntry;
    }
}
