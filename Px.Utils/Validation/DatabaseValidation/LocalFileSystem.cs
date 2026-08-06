using Px.Utils.PxFile.Metadata;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Px.Utils.Validation.DatabaseValidation
{
    /// <summary>
    /// Default file system used for database validation process. Contains default implementations of numerous IO operations
    /// and a function for determining a file's encoding format.
    /// </summary>
    // Excluded from code coverage because it is a wrapper around the file system and testing IO operations is not feasible.
    [ExcludeFromCodeCoverage]
    public class LocalFileSystem : IFileSystem
    {
        /// <inheritdoc/>
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            return Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories);
        }

        /// <inheritdoc/>
        public Stream GetFileStream(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        /// <inheritdoc/>
        public IEnumerable<string> EnumerateDirectories(string path)
        {
            return Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories);
        }

        /// <inheritdoc/>
        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        /// <inheritdoc/>
        public string GetDirectoryName(string path)
        {
            string? directory = Path.GetDirectoryName(path);
            return directory ?? throw new ArgumentException("Path does not contain a directory.");
        }

        /// <inheritdoc/>
        public Encoding GetEncoding(Stream stream)
        {
            PxFileMetadataReader reader = new();
            return reader.GetEncoding(stream);
        }

        /// <inheritdoc/>
        public async Task<Encoding> GetEncodingAsync(Stream stream, CancellationToken cancellationToken)
        {
            PxFileMetadataReader reader = new();
            return await reader.GetEncodingAsync(stream, cancellationToken: cancellationToken);
        }
    }
}
