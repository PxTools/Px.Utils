namespace PxUtils.Validation
{
    /// <summary>
    /// Defines an abstract class for validation entries. A validation entry represents a piece of data that is subject to validation.
    /// </summary>
    public abstract class ValidationEntry(int line, int character, string file)
    {
        /// <summary>
        /// Gets the line number in the file where the entry is located.
        /// </summary>
        public int Line { get; } = line;

        /// <summary>
        /// Gets the character position in the line where the entry starts.
        /// </summary>
        public int Character { get; } = character;

        /// <summary>
        /// Gets the name of the file where the entry is located.
        /// </summary>
        public string File { get; } = file;
    }
}
