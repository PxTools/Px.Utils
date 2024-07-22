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
    }

    // Excluded from code coverage because it is a wrapper around the file system and testing IO operations is not feasible.
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Default file system used for database validation process. Contains default implementations of numerous IO operations
    /// and a function for determining a file's encoding format.
    /// </summary>
    public class DefaultFileSystem : IFileSystem
    {
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            return Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories);
        }

        public Stream GetFileStream(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public IEnumerable<string> EnumerateDirectories(string path)
        {
            return Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories);
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public string GetDirectoryName(string path)
        {
            string? directory = Path.GetDirectoryName(path);
            return directory ?? throw new ArgumentException("Path does not contain a directory.");
        }

        public Encoding GetEncoding(Stream stream)
        {
            const int bomLength = 3;
            long position = stream.Position;

            byte[] bom = new byte[bomLength];
            stream.Read(bom);
            stream.Position = position;

            if (PxFileMetadataReader.GetEncodingFromBOM(bom) is Encoding utf) return utf;

            return Encoding.ASCII;
        }
    }
}
