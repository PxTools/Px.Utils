namespace Px.Utils.UnitTests.Validation.DatabaseValidation
{
    /// <summary>
    /// TODO: Summary
    /// </summary>
    public interface IFileSystem
    {
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
    }

    /// <summary>
    /// TODO: Summary
    /// </summary>
    public class DefaultFileSystem : IFileSystem
    {
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.EnumerateFiles(path, searchPattern, searchOption);
        }
    }
}
