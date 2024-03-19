using PxUtils.PxFile;

namespace PxUtils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents a key-value pair validation entry. A key-value pair validation entry is a type of validation entry that contains a key-value pair. This class implements the <see cref="ValidationEntry"/> interface.
    /// </summary>
    /// <param name="line">The line number in the file where the validation entry is located.</param>
    /// <param name="character">The character position in the line where the validation entry starts.</param>
    /// <param name="file">The name of the file where the validation entry is located.</param>
    /// <param name="keyValueEntry">The key-value pair that this validation entry represents.</param>
    /// <param name="syntaxConf">The syntax configuration for the PX file that this validation entry is part of. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
    public class KeyValuePairValidationEntry(int line, int character, string file, KeyValuePair<string, string> keyValueEntry, PxFileSyntaxConf syntaxConf) : ValidationEntry(line, character, file)
    {
        /// <summary>
        /// Gets the key-value pair that this validation entry represents.
        /// </summary>
        public KeyValuePair<string, string> KeyValueEntry { get; } = keyValueEntry;

        /// <summary>
        /// Gets the syntax configuration for the PX file that this validation entry is part of. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.
        /// </summary>
        public PxFileSyntaxConf SyntaxConf { get; } = syntaxConf;
    }
}
