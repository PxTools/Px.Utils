namespace PxUtils.Validation
{
    /// <summary>
    /// Defines an abstract class for validation objects. A validation object represents a piece of data that is subject to validation.
    /// </summary>
    public abstract class ValidationObject(int line, int character, string file)
    {
        /// <summary>
        /// Line number in the file where the entry is located.
        /// </summary>
        public int Line { get; } = line;

        /// <summary>
        /// Character position in the line where the entry starts.
        /// </summary>
        public int Character { get; } = character;

        /// <summary>
        /// Name of the file where the entry is located.
        /// </summary>
        public string File { get; } = file;
    }
}
