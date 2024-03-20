using PxUtils.PxFile;

namespace PxUtils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents one line of a PX file metadata. Used for validation functions and building key-value-pair entries. This class implements the <see cref="ValidationEntry"/> interface.
    /// </summary>
    /// <param name="line">The line number in the file where the validation entry is located.</param>
    /// <param name="character">The character position in the line where the validation entry starts.</param>
    /// <param name="file">The name of the file where the validation entry is located.</param>
    /// <param name="entry">The string that this validation entry represents.</param>
    /// <param name="syntaxConf">The syntax configuration for the PX file that this validation entry is part of. The syntax configuration is represented by a <see cref="PxFileSyntaxConf"/> object.</param>
    /// <param name="entryIndex">The index of the entry in the file.</param>
    public class StringValidationEntry(int line, int character, string file, string entry, int entryIndex) : ValidationEntry(line, character, file)
    {
        /// <summary>
        /// Gets the string that this validation entry represents.
        /// </summary>
        public string EntryString { get; } = entry;

        /// <summary>
        /// Gets the index of the entry in the file.
        /// </summary>
        public int EntryIndex { get; } = entryIndex;
    }
}
