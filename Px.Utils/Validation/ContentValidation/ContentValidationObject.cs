namespace PxUtils.Validation.ContentValidation
{
    /// <summary>
    /// Object that stores info about the metadata content of a Px file entry during validation process
    /// </summary>
    /// <param name="file">Name of the Px file</param>
    /// <param name="keyStartLineIndex">Index of the line where the entry starts</param>
    /// <param name="lineChangeIndexes">Array of character indexes where lines change within the entry</param>
    public class ContentValidationObject(string file, int keyStartLineIndex, int[] lineChangeIndexes) : ValidationObject(file, keyStartLineIndex, lineChangeIndexes) {}
}
