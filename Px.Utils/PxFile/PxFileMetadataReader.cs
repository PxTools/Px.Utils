using PxUtils.Exceptions;
using System.Runtime.CompilerServices;
using System.Text;

namespace PxUtils.PxFile.Meta
{
    /// <summary>
    /// Provides methods to read metadata and get the encoding from a stream of a Px file.
    /// </summary>
    public static class PxFileMetadataReader
    {
        private const int DEFAULT_READ_BUFFER_SIZE = 4096;

        /// <summary>
        /// Processes a provided stream to extract metadata, returning the results as an IEnumerable of key-value pairs.
        /// The method uses the specified encoding and symbols configuration (or defaults if not provided) to interpret the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the metadata.</param>
        /// <param name="encoding">The encoding to use when reading the stream.</param>
        /// <param name="symbolsConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <param name="readBufferSize">The size of the buffer to use when reading the stream. If not specified, the default buffer size is used.</param>
        /// <returns>An IEnumerable of key-value pairs representing the metadata entries in the file.</returns>
        public static IEnumerable<KeyValuePair<string, string>> ReadMetadata(Stream stream, Encoding encoding, PxFileSyntaxConf? symbolsConf = null, int readBufferSize = DEFAULT_READ_BUFFER_SIZE)
        {
            symbolsConf ??= PxFileSyntaxConf.Default;

            char keywordSeperator = symbolsConf.Symbols.KeywordSeparator;
            char sectionSeparator = symbolsConf.Symbols.SectionSeparator;
            char stringDelimeter = symbolsConf.Symbols.Value.StringDelimeter;
            string dataKeyword = symbolsConf.Tokens.KeyWords.Data;

            char[] buffer = new char[readBufferSize];
            char nextDelimeter = keywordSeperator;
            bool keyWordMode = true;
            bool readingValueString = false;
            bool endOfMetaSection = false;

            StringBuilder keyWordBldr = new();
            StringBuilder valueStringBldr = new();

            StreamReader reader = new(stream, encoding);

            do
            {
                int readChars = reader.Read(buffer, 0, readBufferSize);
                int lastDelimeterIndx = -1;

                for (int i = 0; i < readChars; i++)
                {
                    if (buffer[i] == stringDelimeter) readingValueString = !readingValueString;
                    if (!readingValueString)
                    {
                        if ((keyWordMode && buffer[i] == sectionSeparator) || (!keyWordMode && buffer[i] == keywordSeperator))
                        {
                            throw new InvalidPxFileMetadataException($"Unexpected character '{buffer[i]}' found at position {i}.");
                        }

                        if (buffer[i] == nextDelimeter)
                        {
                            Append(buffer, lastDelimeterIndx + 1, i, keyWordMode, keyWordBldr, valueStringBldr);
                            if (keyWordBldr.ToString().Trim() == dataKeyword)
                            {
                                endOfMetaSection = true;
                                break;
                            }
                            else if (!keyWordMode)
                            {
                                yield return new KeyValuePair<string, string>(keyWordBldr.ToString().Trim(), valueStringBldr.ToString().Trim());
                                keyWordBldr.Clear();
                                valueStringBldr.Clear();
                            }

                            lastDelimeterIndx = i;
                            keyWordMode = !keyWordMode;
                            nextDelimeter = keyWordMode ? keywordSeperator : sectionSeparator;
                        }
                    }
                }
                Append(buffer, lastDelimeterIndx + 1, readChars, keyWordMode, keyWordBldr, valueStringBldr);

            } while (!endOfMetaSection && !reader.EndOfStream);
        }

        /// <summary>
        /// Asynchronausly processes a provided stream to extract metadata, returning the results as an IEnumerable of key-value pairs.
        /// The method uses the specified encoding and symbols configuration (or defaults if not provided) to interpret the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the metadata.</param>
        /// <param name="encoding">The encoding to use when reading the stream.</param>
        /// <param name="symbolsConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <param name="readBufferSize">The size of the buffer to use when reading the stream. If not specified, the default buffer size is used.</param>
        /// <param name="cancellationToken">Can be used to cancel the operation.</param>
        /// <returns>An IAsyncEnumerable of key-value pairs representing the metadata entries in the file. This can be asynchronously iterated over as the data is read.</returns>
        public static async IAsyncEnumerable<KeyValuePair<string, string>> ReadMetadataAsync(
            Stream stream,
            Encoding encoding,
            PxFileSyntaxConf? symbolsConf = null,
            int readBufferSize = DEFAULT_READ_BUFFER_SIZE,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            symbolsConf ??= PxFileSyntaxConf.Default;

            char keywordSeperator = symbolsConf.Symbols.KeywordSeparator;
            char sectionSeparator = symbolsConf.Symbols.SectionSeparator;
            char stringDelimeter = symbolsConf.Symbols.Value.StringDelimeter;
            string dataKeyword = symbolsConf.Tokens.KeyWords.Data;

            char[] readBuffer = new char[readBufferSize];
            char[] parsingBuffer = new char[readBufferSize];

            StreamReader reader = new(stream, encoding);

            ValueTask<int> readTask = reader.ReadAsync(readBuffer.AsMemory(), cancellationToken);

            char nextDelimeter = keywordSeperator;
            bool keyWordMode = true;
            bool readingValueString = false;
            bool endOfMetaSection = false;

            StringBuilder keyWordBldr = new();
            StringBuilder valueStringBldr = new();

            do
            {
                int readChars = await readTask;
                (readBuffer, parsingBuffer) = (parsingBuffer, readBuffer);
                readTask = reader.ReadAsync(readBuffer.AsMemory(), cancellationToken);

                int lastDelimeterIndx = -1;

                for (int i = 0; i < readChars; i++)
                {
                    if (parsingBuffer[i] == stringDelimeter) readingValueString = !readingValueString;
                    if (!readingValueString)
                    {
                        if ((keyWordMode && parsingBuffer[i] == sectionSeparator) || (!keyWordMode && parsingBuffer[i] == keywordSeperator))
                        {
                            throw new InvalidPxFileMetadataException($"Unexpected character '{parsingBuffer[i]}' found at position {i}.");
                        }

                        if (parsingBuffer[i] == nextDelimeter)
                        {
                            Append(parsingBuffer, lastDelimeterIndx + 1, i, keyWordMode, keyWordBldr, valueStringBldr);
                            if (keyWordBldr.ToString().Trim() == dataKeyword)
                            {
                                endOfMetaSection = true;
                                break;
                            }
                            else if (!keyWordMode)
                            {
                                yield return new KeyValuePair<string, string>(keyWordBldr.ToString().Trim(), valueStringBldr.ToString().Trim());
                                keyWordBldr.Clear();
                                valueStringBldr.Clear();
                            }

                            lastDelimeterIndx = i;
                            keyWordMode = !keyWordMode;
                            nextDelimeter = keyWordMode ? keywordSeperator : sectionSeparator;
                        }
                    }
                }
                Append(parsingBuffer, lastDelimeterIndx + 1, readChars, keyWordMode, keyWordBldr, valueStringBldr);

            } while (!endOfMetaSection && !reader.EndOfStream);
        }

        /// <summary>
        /// Reads the metadata from the provided stream and returns it as a dictionary of key-value pairs.
        /// The method uses the specified encoding and symbols configuration (or defaults if not provided) to interpret the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the metadata.</param>
        /// <param name="encoding">The encoding to use when reading the stream.</param>
        /// <param name="symbolsConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <param name="readBufferSize">The size of the buffer to use when reading the stream. If not specified, the default buffer size is used.</param>
        /// <returns>A dictionary containing the metadata entries in the file.</returns>
        public static IReadOnlyDictionary<string, string> ReadMetadataToDictionary(
            Stream stream,
            Encoding encoding,
            PxFileSyntaxConf? symbolsConf = null,
            int readBufferSize = DEFAULT_READ_BUFFER_SIZE)
            => new Dictionary<string, string>(ReadMetadata(stream, encoding, symbolsConf, readBufferSize));

        /// <summary>
        /// Asynchronously reads the metadata from the provided stream and returns it as a dictionary of key-value pairs.
        /// The method uses the specified encoding and symbols configuration (or defaults if not provided) to interpret the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the metadata.</param>
        /// <param name="encoding">The encoding to use when reading the stream.</param>
        /// <param name="symbolsConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <param name="readBufferSize">The size of the buffer to use when reading the stream. If not specified, the default buffer size is used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task that represents the asynchronous operation. The Task result contains a dictionary of the metadata entries in the file.</returns>
        public static async Task<IReadOnlyDictionary<string, string>> ReadMetadataToDictionaryAsync(
            Stream stream,
            Encoding encoding,
            PxFileSyntaxConf? symbolsConf = null,
            int readBufferSize = DEFAULT_READ_BUFFER_SIZE,
            CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> metaDict = [];
            ConfiguredCancelableAsyncEnumerable<KeyValuePair<string, string>> metaEnumerable = 
                ReadMetadataAsync(stream, encoding, symbolsConf, readBufferSize, cancellationToken)
                .WithCancellation(cancellationToken);

            await foreach (KeyValuePair<string, string> kvp in metaEnumerable) metaDict.Add(kvp.Key, kvp.Value);

            return metaDict;
        }

        /// <summary>
        /// Determines the encoding of the provided stream based on the Byte Order Mark (BOM) or the CODEPAGE keyword in the metadata.
        /// If no BOM or CODEPAGE keyword is found, an exception is thrown. If the encoding specified by the CODEPAGE keyword is not available,
        /// it attempts to register it using the CodePagesEncodingProvider.
        /// </summary>
        /// <param name="stream">The stream from which to determine the encoding.</param>
        /// <param name="symbolsConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <param name="allowSoftException">If true, the method will not throw an exception if the CODEPAGE keyword is not found. This is used by the metadata validation methods.</param>
        /// <returns>The determined Encoding of the stream.</returns>
        public static Encoding? GetEncoding(Stream stream, PxFileSyntaxConf? symbolsConf = null, bool allowSoftException = false)
        {
            symbolsConf ??= PxFileSyntaxConf.Default;
            long position = stream.Position;

            byte[] bom = new byte[3];
            stream.Read(bom);

            stream.Position = position;

            if (GetBom(bom) is Encoding utf) return utf;

            // Use ASCII because encoding is still unknown, CODEPAGE keyword is readable as ASCII
            KeyValuePair<string, string> encoding = ReadMetadata(stream, Encoding.ASCII, symbolsConf, 512)
                .FirstOrDefault(kvp => kvp.Key == symbolsConf.Tokens.KeyWords.CodePage);

            return GetEncodingFromValue(encoding.Value, symbolsConf, allowSoftException);
        }

        /// <summary>
        /// Asynchronously determines the encoding of the provided stream based on the Byte Order Mark (BOM) or the CODEPAGE keyword in the metadata.
        /// If no BOM or CODEPAGE keyword is found, an exception is thrown. If the encoding specified by the CODEPAGE keyword is not available,
        /// it attempts to register it using the CodePagesEncodingProvider.
        /// </summary>
        /// <param name="stream">The stream from which to determine the encoding.</param>
        /// <param name="symbolsConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task that represents the asynchronous operation. The Task result contains the determined Encoding of the stream.</returns>
        public static async Task<Encoding?> GetEncodingAsync(Stream stream, PxFileSyntaxConf? symbolsConf = null, CancellationToken cancellationToken = default, bool allowSoftException = false)
        {
            symbolsConf ??= PxFileSyntaxConf.Default;
            long position = stream.Position;

            byte[] bom = new byte[3];
            await stream.ReadAsync(bom.AsMemory(0, 3), cancellationToken);

            stream.Position = position;

            if (GetBom(bom) is Encoding utf) return utf;

            // Use ASCII because encoding is still unknown, CODEPAGE keyword is readable as ASCII
            KeyValuePair<string, string> encoding = await ReadMetadataAsync(stream, Encoding.ASCII, symbolsConf, 512, cancellationToken)
                .FirstOrDefaultAsync(kvp => kvp.Key == symbolsConf.Tokens.KeyWords.CodePage, cancellationToken);

            return GetEncodingFromValue(encoding.Value, symbolsConf, allowSoftException);
        }

        #region Private Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Append(char[] buffer, int start, int endIndex, bool keyWordMode, StringBuilder keyWordBldr, StringBuilder valueStringBldr)
        {
            if (endIndex - start > 0)
            {
                if (keyWordMode)
                {
                    keyWordBldr.Append(buffer, start, endIndex - start);
                }
                else
                {
                    valueStringBldr.Append(buffer, start, endIndex - start);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Encoding? GetBom(byte[] buffer)
        {
            // UTF8 BOM
            if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            {
                return Encoding.UTF8;
            }

            // UTF16 BOM
            else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
            {
                return Encoding.Unicode;
            }

            // UTF32 BOM
            else if (buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0xFE)
            {
                return Encoding.UTF32;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Encoding? GetEncodingFromValue(string value, PxFileSyntaxConf symbolsConf, bool allowSoftException = false)
        {
            if (value is null) throw new InvalidPxFileMetadataException($"Could not find CODEPAGE keyword in the file.");

            string encodingName = value.Trim(symbolsConf.Symbols.Value.StringDelimeter);

            bool isAvailable = Array.Exists(Encoding.GetEncodings(), e => e.Name == encodingName);
            if (!isAvailable) Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            try
            {
                return Encoding.GetEncoding(encodingName);
            }
            catch (ArgumentException aExp)
            {
                if (allowSoftException)
                    return null;
                else
                {
                    throw new InvalidPxFileMetadataException($"The encoding {encodingName} provided with the CODEPAGE keyword is not available", aExp);
                }
            }
        }

        #endregion
    }
}
