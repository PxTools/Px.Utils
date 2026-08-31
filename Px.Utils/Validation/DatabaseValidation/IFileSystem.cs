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
        /// <summary>
        /// Enumerates all files in the specified directory and its subdirectories that match the given search pattern.
        /// </summary>
        /// <param name="path">Path to the directory to search.</param>
        /// <param name="searchPattern">Pattern to match against the names of files in the directory. This parameter can contain a combination of literal and wildcard characters.</param>
        /// <returns>Enumerable collection of file paths that match the search pattern.</returns>
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern);
        /// <summary>
        /// Gets a read-only stream for the specified file path.
        /// </summary>
        /// <param name="path">Path to the file to open.</param>
        /// <returns>Stream for reading the specified file.</returns>
        public Stream GetFileStream(string path);
        /// <summary>
        /// Enumerates all directories in the specified path and its subdirectories.
        /// </summary>
        /// <param name="path">Path to the directory to search.</param>
        /// <returns>Enumerable collection of directory paths.</returns>
        public IEnumerable<string> EnumerateDirectories(string path);
        /// <summary>
        /// Gets the file name and extension of the specified path string.
        /// </summary>
        /// <param name="path">Path string from which to get the file name and extension.</param>
        /// <returns>Name and extension of the specified path string.</returns>
        public string GetFileName(string path);
        /// <summary>
        /// Gets the directory name of the specified path string.
        /// </summary>
        /// <param name="path">Path string from which to get the directory name.</param>
        /// <returns>Name of the directory from the specified path string.</returns>
        public string GetDirectoryName(string path);
        /// <summary>
        /// Gets the encoding of the specified stream. This method may read from the stream to determine its encoding, so the stream position may be changed after calling this method.
        /// </summary>
        /// <param name="stream">Stream from which to determine the encoding.</param>
        /// <returns>Encoding of the specified stream.</returns>
        public Encoding GetEncoding(Stream stream);
        /// <summary>
        /// Asynchronously gets the encoding of the specified stream. This method may read from the stream to determine its encoding, so the stream position may be changed after calling this method.
        /// </summary>
        /// <param name="stream">Stream from which to determine the encoding.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>Task representing the asynchronous operation, with the encoding of the specified stream as the result.</returns>
        public Task<Encoding> GetEncodingAsync(Stream stream, CancellationToken cancellationToken);
    }
}
