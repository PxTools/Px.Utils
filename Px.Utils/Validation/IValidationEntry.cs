namespace PxUtils.Validation
{
    /// <summary>
    /// Defines a common interface for validation entries. A validation entry represents a piece of data that is subject to validation.
    /// </summary>
    public interface IValidationEntry
    {
        /// <summary>
        /// Gets the line number in the file where the entry is located.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the character position in the line where the entry starts.
        /// </summary>
        public int Character { get; }

        /// <summary>
        /// Gets the name of the file where the entry is located.
        /// </summary>
        public string File { get; }
    }
}
