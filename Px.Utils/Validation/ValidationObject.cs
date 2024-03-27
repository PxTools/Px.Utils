namespace PxUtils.Validation
{
    /// <summary>
    /// Defines an abstract class for validation objects. A validation object represents a piece of data that is subject to validation.
    /// </summary>
    public abstract class ValidationObject(string file, int keyStartLineIndex, int[] lineChangeIndexes)
    {
        /// <summary>
        /// Name of the file where the entry is located.
        /// </summary>
        public string File { get; } = file;
        
        /// <summary>
        /// Index of the line where the entry starts
        /// </summary>
        public int KeyStartLineIndex { get; } = keyStartLineIndex;

        /// <summary>
        /// Character indexes of the line changes in the entry
        /// </summary>
        public int[] LineChangeIndexes { get; } = lineChangeIndexes;
    }
}
