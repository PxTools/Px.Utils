using Px.Utils.Validation.DatabaseValidation;
using System.Text;

namespace Px.Utils.UnitTests.Validation.DatabaseValidation
{
    /// <summary>
    /// Mock file system used for database validation tests.
    /// </summary>
    public class MockFileSystem : IFileSystem
    {
        private readonly Dictionary<string, IEnumerable<string>> _directories = new()
        {
            { "database", ["database/category", "database/category/directory", "database/_INDEX"] },
            { "database_invalid", ["database_invalid/category", "database_invalid/category/directory", "database_invalid/_INDEX"] },
            { "database_single_language", ["database_single_language/category", "database_single_language/category/directory", "database_single_language/_INDEX"] },
            { "database_empty", [] },
            { "database_unreadable_alias", [] }
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
                "database_invalid/Alias_fi.txt",
                "database_invalid/Alias_en.txt",
                "database_invalid/Alias_sv.txt",
                "database_invalid/category/Alias_fi.txt",
                "database_invalid/category/Alias_en.txt",
                "database_invalid/category/Alias_sv.txt",
                "database_invalid/category/directory/foo.px",
                "database_invalid/category/directory/bar.px",
                "database_invalid/category/directory/baz.px",
                "database_invalid/category/directory/invalid.px",
                "database_invalid/category/directory/Alias_fi.txt",
                "database_invalid/category/directory/Alias_en.txt",
                "database_invalid/category/directory/Alias_sv.txt",
            ] },
            { "database_single_language", [
                "database_single_language/Alias_en.txt",
                "database_single_language/category/Alias_en.txt",
                "database_single_language/category/directory/foo_sl.px",
                "database_single_language/category/directory/bar_sl.px",
                "database_single_language/category/directory/baz_sl.px",
                "database_single_language/category/directory/Alias_en.txt",
            ] },
            { "database_empty", [] },
            { "database_unreadable_alias", ["database_unreadable_alias/Alias_en.txt"] }
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
            if (path.StartsWith("database_unreadable_alias", StringComparison.Ordinal))
            {
                return new MemoryStream();
            }

            byte[] data = path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
                ? Encoding.UTF8.GetBytes("Alias file content with special characters: ÅÄÖ")
                : Encoding.UTF8.GetBytes(MockDatabaseFileStreams.FileStreams[GetFixturePath(path)]);
            return new MemoryStream(data);
        }

        private static string GetFixturePath(string path)
        {
            return path
                .Replace("database_invalid", "database", StringComparison.Ordinal)
                .Replace("database_single_language", "database", StringComparison.Ordinal);
        }

        public Encoding GetEncoding(Stream stream)
        {
            return Encoding.UTF8;
        }

        public async Task<Encoding> GetEncodingAsync(Stream stream, CancellationToken cancellationToken)
        {
            return await Task.Run(() => GetEncoding(stream));
        }
    }
}
