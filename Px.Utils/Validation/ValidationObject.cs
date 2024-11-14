namespace Px.Utils.Validation
{
    /// <summary>
    /// Class for validation objects. A validation object represents an entry in a PX file that is subject to validation.
    /// </summary>
    public class ValidationObject(string file, int keyStartLineIndex, int[] lineChangeIndexes)
    {
        /// <summary>
        /// Name of the file where the entry is located.
        /// </summary>
        public string File { get; } = file;
        
        /// <summary>
        /// Index of the line where the entry starts.
        /// </summary>
        public int KeyStartLineIndex { get; } = keyStartLineIndex;

        /// <summary>
        /// Character indexes of the line changes in the entry starting from the entry start.
        /// </summary>
        public int[] LineChangeIndexes { get; } = lineChangeIndexes;
    }
}
