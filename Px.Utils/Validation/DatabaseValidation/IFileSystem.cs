using Px.Utils.PxFile.Metadata;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Px.Utils.Validation.DatabaseValidation
{
    /// <summary>
    /// Interface for implementing custom file processing systems for database validation.
    /// </summary>
    public interface IFileSystem
    {
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern);
        public Stream GetFileStream(string path);
        public IEnumerable<string> EnumerateDirectories(string path);
        public string GetFileName(string path);
        public string GetDirectoryName(string path);
        public Encoding GetEncoding(Stream stream);
        public Task<Encoding> GetEncodingAsync(Stream stream, CancellationToken cancellationToken);
    }
}
