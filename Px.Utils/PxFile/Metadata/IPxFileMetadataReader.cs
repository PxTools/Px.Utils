using System.Text;

namespace Px.Utils.PxFile.Metadata
{
    /// <summary>
    /// Interface for reading the metadata section from a px-file.
    /// </summary>
    public interface IPxFileMetadataReader
    {
        /// <summary>
        /// Determines the encoding of the provided stream based on the Byte Order Mark (BOM) or the CODEPAGE keyword in the metadata.
        /// </summary>
        /// <param name="stream">The stream from which to determine the encoding.</param>
        /// <param name="syntaxConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <returns>The determined encoding of the stream.</returns>
        Encoding GetEncoding(Stream stream, PxFileSyntaxConf? syntaxConf = null);
       
        /// <summary>
        /// Asynchronously determines the encoding of the provided stream based on the Byte Order Mark (BOM) or the CODEPAGE keyword in the metadata.
        /// </summary>
        /// <param name="stream">The stream from which to determine the encoding.</param>
        /// <param name="syntaxConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <returns>The determined encoding of the stream.</returns>
        Task<Encoding> GetEncodingAsync(Stream stream, PxFileSyntaxConf? syntaxConf = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes a provided stream to extract metadata, returning the results as an <see cref="IEnumerable{T}"/> of key-value pairs.
        /// The method uses the specified encoding and symbols configuration (or defaults if not provided) to interpret the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the metadata.</param>
        /// <param name="encoding">The encoding to use when reading the stream.</param>
        /// <param name="syntaxConf">The syntax configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <param name="readBufferSize">The size of the buffer to use when reading the stream. If not specified, the default buffer size is used.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of key-value pairs representing the metadata entries in the file.</returns>
        IEnumerable<KeyValuePair<string, string>> ReadMetadata(Stream stream, Encoding encoding, PxFileSyntaxConf? syntaxConf = null, int readBufferSize = 4096);

        /// <summary>
        /// Asynchronously processes a provided stream to extract metadata, returning the results as an <see cref="IEnumerable{T}"/> of key-value pairs.
        /// The method uses the specified encoding and symbols configuration (or defaults if not provided) to interpret the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the metadata.</param>
        /// <param name="encoding">The encoding to use when reading the stream.</param>
        /// <param name="syntaxConf">The syntax configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <param name="readBufferSize">The size of the buffer to use when reading the stream. If not specified, the default buffer size is used.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of key-value pairs representing the metadata entries in the file.</returns>
        IAsyncEnumerable<KeyValuePair<string, string>> ReadMetadataAsync(Stream stream, Encoding encoding, PxFileSyntaxConf? syntaxConf = null, int readBufferSize = 4096, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads the metadata from the provided stream and returns it as a dictionary of key-value pairs.
        /// The method uses the specified encoding and symbols configuration (or defaults if not provided) to interpret the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the metadata.</param>
        /// <param name="encoding">The encoding to use when reading the stream.</param>
        /// <param name="syntaxConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <param name="readBufferSize">The size of the buffer to use when reading the stream. If not specified, the default buffer size is used.</param>
        /// <returns>A dictionary containing the metadata entries in the file.</returns>
        Dictionary<string, string> ReadMetadataToDictionary(Stream stream, Encoding encoding, PxFileSyntaxConf? syntaxConf = null, int readBufferSize = 4096);

        /// <summary>
        /// Asynchronously reads the metadata from the provided stream and returns it as a dictionary of key-value pairs.
        /// The method uses the specified encoding and symbols configuration (or defaults if not provided) to interpret the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the metadata.</param>
        /// <param name="encoding">The encoding to use when reading the stream.</param>
        /// <param name="syntaxConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <param name="readBufferSize">The size of the buffer to use when reading the stream. If not specified, the default buffer size is used.</param>
        /// <returns>A dictionary containing the metadata entries in the file.</returns>
        Task<Dictionary<string, string>> ReadMetadataToDictionaryAsync(Stream stream, Encoding encoding, PxFileSyntaxConf? syntaxConf = null, int readBufferSize = 4096, CancellationToken cancellationToken = default);
    }
}