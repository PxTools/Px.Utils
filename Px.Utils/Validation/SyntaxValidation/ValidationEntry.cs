using Px.Utils.PxFile;

namespace Px.Utils.Validation.SyntaxValidation
{
    /// <summary>
    /// Represents one entry in PX file metadata. Used for validation functions and building key-value-pair entries. This class implements the <see cref="ValidationObject"/> interface.
    /// </summary>
    /// <param name="file">The name of the file where the validation entry is located.</param>
    /// <param name="entry">The string that this validation entry represents.</param>
    /// <param name="entryIndex">The index of the entry in the file.</param>
    /// <param name="keyStartLineIndex">Index of the line where the entry starts.</param>
    /// <param name="lineChangeIndexes">Character indexes of the line changes in the entry starting from the entry start.</param>
    public class ValidationEntry(string file, string entry, int entryIndex, int keyStartLineIndex, int[] lineChangeIndexes) : ValidationObject(file, keyStartLineIndex, lineChangeIndexes)
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
