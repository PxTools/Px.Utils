namespace PxUtils.Validation
{
    /// <summary>
    /// Defines an abstract class for validation objects. A validation object represents a piece of data that is subject to validation.
    /// </summary>
    public abstract class ValidationObject(string file, int keyStartIndex)
    {
        /// <summary>
        /// Name of the file where the entry is located.
        /// </summary>
        public string File { get; } = file;

        /// <summary>
        /// Index of the character that starts the key part of the entry
        /// </summary>
        public int KeyStartIndex { get; } = keyStartIndex;
    }
}
