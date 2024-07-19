using Px.Utils.PxFile.Metadata;
using System.Text;

namespace Px.Utils.Validation.DatabaseValidation
{
    /// <summary>
    /// TODO: Summary
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

    /// <summary>
    /// TODO: Summary
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
