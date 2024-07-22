using Px.Utils.Validation.DatabaseValidation;
using System.Text;

namespace Px.Utils.UnitTests.Validation.DatabaseValidation
{
    /// <summary>
    /// TODO: Summary
    /// </summary>
    public class MockFileSystem : IFileSystem
    {
        private readonly Dictionary<string, IEnumerable<string>> _directories = new()
        {
            { "database", ["database/category", "database/category/directory", "database/_INDEX"] },
            { "database_invalid", ["database_invalid/category", "database_invalid/category/directory", "database_invalid/_INDEX"] },
            { "database_single_language", ["database_single_language/category", "database_single_language/category/directory", "database_single_language/_INDEX"] }
        };

        private readonly Dictionary<string, IEnumerable<string>> _files = new()
        {
            { "database", [
                "database/Alias_fi.txt",
                "database/Alias_en.txt",
                "database/Alias_sv.txt",
                "database/category/Alias_fi.txt",
                "database/category/Alias_en.txt",
                "database/category/Alias_sv.txt",
                "database/category/directory/foo.px",
                "database/category/directory/bar.px",
                "database/category/directory/baz.px",
                "database/category/directory/Alias_fi.txt",
                "database/category/directory/Alias_en.txt",
                "database/category/directory/Alias_sv.txt",
            ] },
            { "database_invalid", [
                "database/Alias_fi.txt",
                "database/Alias_en.txt",
                "database/Alias_sv.txt",
                "database/category/Alias_fi.txt",
                "database/category/Alias_en.txt",
                "database/category/Alias_sv.txt",
                "database/category/directory/foo.px",
                "database/category/directory/bar.px",
                "database/category/directory/baz.px",
                "database/category/directory/invalid.px",
                "database/category/directory/Alias_fi.txt",
                "database/category/directory/Alias_en.txt",
                "database/category/directory/Alias_sv.txt",
            ] },
            { "database_single_language", [
                "database/Alias_en.txt",
                "database/category/Alias_en.txt",
                "database/category/directory/foo_sl.px",
                "database/category/directory/bar_sl.px",
                "database/category/directory/baz_sl.px",
                "database/category/directory/Alias_en.txt",
            ] }
        };

        public IEnumerable<string> EnumerateDirectories(string path)
        {
            return _directories[path];
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            IEnumerable<string> files = _files[path];
            if (searchPattern == "*.px")
            {
                return files.Where(f => f.EndsWith(".px", StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                return files.Where(f => f.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public string GetDirectoryName(string path)
        {
            string? directoryName = Path.GetDirectoryName(path);
            return directoryName ?? throw new ArgumentException("Path does not contain a directory.");
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public Stream GetFileStream(string path)
        {
            byte[] data = Encoding.UTF8.GetBytes(MockDatabaseFileStreams.FileStreams[path]);
            return new MemoryStream(data);
        }

        public Encoding GetEncoding(Stream stream)
        {
            return Encoding.UTF8;
        }
    }
}
