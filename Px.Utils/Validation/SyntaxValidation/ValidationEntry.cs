using PxUtils.PxFile;

namespace PxUtils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents one entry in PX file metadata. Used for validation functions and building key-value-pair entries. This class implements the <see cref="ValidationObject"/> interface.
    /// </summary>
    /// <param name="line">The line numbers in the file where the validation entry is located.</param>
    /// <param name="file">The name of the file where the validation entry is located.</param>
    /// <param name="entry">The string that this validation entry represents.</param>
    /// <param name="entryIndex">The index of the entry in the file.</param>
    public class ValidationEntry(string file, string entry, int entryIndex, int startIndex) : ValidationObject(file, startIndex)
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
