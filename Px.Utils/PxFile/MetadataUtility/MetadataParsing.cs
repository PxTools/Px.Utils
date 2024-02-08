using Px.Utils.PxFile.Exceptions;
using System.Runtime.CompilerServices;
using System.Text;

namespace PxUtils.PxFile.MetadataUtility
{
    /// <summary>
    /// Static class for utility functions to parse metadata from a Px file.
    /// </summary>
    internal static class MetadataParsing
    {
        /// <summary>
        /// Asynchronously reads metadata entries from the provided input stream.
        /// Each metadata entry is a key-value pair, where the key and value are separated by a specified keyword separator,
        /// and each entry is separated by a specified section separator.
        /// The method continues reading entries until it encounters a keyword that matches the 'Data' keyword in the provided symbols configuration,
        /// or until it reaches the end of the stream.
        /// </summary>
        /// <param name="inputStream">The StreamReader from which to read the metadata entries.</param>
        /// <param name="symbolsConf">The configuration that specifies the keyword separator, section separator, and 'Data' keyword.</param>
        /// <param name="readBufferSize">The size of the buffer to use when reading from the stream.</param>
        /// <returns>An IAsyncEnumerable of key-value pairs representing the metadata entries.</returns>
        internal static async IAsyncEnumerable<KeyValuePair<string, string>> GetMetadataEntriesAsync(StreamReader inputStream, PxFileSymbolsConf symbolsConf, int readBufferSize)
        {
            char keywordSeperator = symbolsConf.Tokens.KeywordSeparator;
            char sectionSeparator = symbolsConf.Tokens.SectionSeparator;
            string dataKeyword = symbolsConf.Symbols.KeyWords.Data;

            char[] readBuffer = new char[readBufferSize];
            char[] parsingBuffer = new char[readBufferSize];

            ValueTask<int> readTask = inputStream.ReadAsync(readBuffer.AsMemory());
            
            char nextDelimeter = keywordSeperator;
            bool keyWordMode = true;
            bool endOfMetaSection = false;

            StringBuilder keyWordBldr = new();
            StringBuilder valueStringBldr = new();

            do
            {
                int readChars = await readTask;
                (readBuffer, parsingBuffer) = (parsingBuffer, readBuffer);
                readTask = inputStream.ReadAsync(readBuffer.AsMemory());

                int lastDelimeterIndx = -1;

                for (int i = 0; i < readChars; i++)
                {
                    if (parsingBuffer[i] == nextDelimeter)
                    {
                        Append(parsingBuffer, lastDelimeterIndx + 1, readChars, keyWordMode, keyWordBldr, valueStringBldr);
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
                Append(parsingBuffer, lastDelimeterIndx + 1, readChars, keyWordMode, keyWordBldr, valueStringBldr);

            } while (!endOfMetaSection && !inputStream.EndOfStream);
        }

        /// <summary>
        /// Reads metadata entries from the provided input stream synchronously.
        /// Each metadata entry is a key-value pair, where the key and value are separated by a specified keyword separator,
        /// and each entry is separated by a specified section separator.
        /// The method continues reading entries until it encounters a keyword that matches the 'Data' keyword in the provided symbols configuration,
        /// or until it reaches the end of the stream.
        /// </summary>
        /// <param name="inputStream">The StreamReader from which to read the metadata entries.</param>
        /// <param name="symbolsConf">The configuration that specifies the keyword separator, section separator, and 'Data' keyword.</param>
        /// <param name="readBufferSize">The size of the buffer to use when reading from the stream.</param>
        /// <returns>An IEnumerable of key-value pairs representing the metadata entries.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> GetMetadataEntries(StreamReader inputStream, PxFileSymbolsConf symbolsConf, int readBufferSize)
        {
            char keywordSeperator = symbolsConf.Tokens.KeywordSeparator;
            char sectionSeparator = symbolsConf.Tokens.SectionSeparator;
            string dataKeyword = symbolsConf.Symbols.KeyWords.Data;

            char[] buffer = new char[readBufferSize];
            char nextDelimeter = keywordSeperator;
            bool keyWordMode = true;
            bool endOfMetaSection = false;

            StringBuilder keyWordBldr = new();
            StringBuilder valueStringBldr = new();

            do
            {
                int readChars = inputStream.Read(buffer, 0, readBufferSize);
                int lastDelimeterIndx = -1;

                for (int i = 0; i < readChars; i++)
                {
                    if (buffer[i] == nextDelimeter)
                    {
                        Append(buffer, lastDelimeterIndx + 1, readChars, keyWordMode, keyWordBldr, valueStringBldr);
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
                Append(buffer, lastDelimeterIndx + 1, readChars, keyWordMode, keyWordBldr, valueStringBldr);

            } while (!endOfMetaSection && !inputStream.EndOfStream);
        }

        /// <summary>
        /// Asynchronously determines the encoding of the file managed by the PxFileStreamFactory.
        /// The method first checks for the presence of a Byte Order Mark (BOM) to identify UTF-8, UTF-16, and UTF-32 encodings.
        /// If no BOM is found, it reads the file's metadata to find the 'CODEPAGE' keyword and uses its value to determine the encoding.
        /// If the 'CODEPAGE' keyword is not found or the encoding it specifies is not available, an exception is thrown.
        /// </summary>
        /// <returns>The detected Encoding of the file.</returns>
        /// <exception cref="PxFileStreamException">Thrown when the 'CODEPAGE' keyword is not found in the file's metadata or the specified encoding is not available.</exception>
        internal static async Task<Encoding> GetEncodingAsync(Stream stream, int readBufferSize)
        {
            const int CODEPAGE_SEARCH_TIMEOUT_MS = 2000;

            stream.Position = 0;

            byte[] bom = new byte[3];
            await stream.ReadAsync(bom.AsMemory(0, 3));

            if (GetBom(bom) is Encoding utf) return utf;

            PxFileSymbolsConf symbolsConf = PxFileSymbolsConf.Default;

            using CancellationTokenSource ctSource = new();
            ctSource.CancelAfter(CODEPAGE_SEARCH_TIMEOUT_MS);

            stream.Position = 0;

            // Use ASCII because encoding is still unknown, CODEPAGE keyword is readable as ASCII
            KeyValuePair<string, string> encoding = await GetMetadataEntriesAsync(new StreamReader(stream, Encoding.ASCII), symbolsConf, readBufferSize)
                .FirstOrDefaultAsync(kvp => kvp.Key == symbolsConf.Symbols.KeyWords.CodePage, ctSource.Token);

            return GetEncodingFromValue(encoding.Value, symbolsConf);
        }

        /// <summary>
        /// Determines the encoding of the file managed by the PxFileStreamFactory.
        /// The method first checks for the presence of a Byte Order Mark (BOM) to identify UTF-8, UTF-16, and UTF-32 encodings.
        /// If no BOM is found, it reads the file's metadata to find the 'CODEPAGE' keyword and uses its value to determine the encoding.
        /// If the 'CODEPAGE' keyword is not found or the encoding it specifies is not available, an exception is thrown.
        /// </summary>
        /// <param name="stream">The Stream object representing the file.</param>
        /// <param name="readBufferSize">The size of the buffer to use when reading from the stream.</param>
        /// <returns>The detected Encoding of the file.</returns>
        /// <exception cref="PxFileStreamException">Thrown when the 'CODEPAGE' keyword is not found in the file's metadata or the specified encoding is not available.</exception>
        internal static Encoding GetEncoding(Stream stream, int readBufferSize)
        {
            stream.Position = 0;

            byte[] bom = new byte[3];
            stream.Read(bom);

            if (GetBom(bom) is Encoding utf) return utf;

            PxFileSymbolsConf symbolsConf = PxFileSymbolsConf.Default;

            stream.Position = 0;

            // Use ASCII because encoding is still unknown, CODEPAGE keyword is readable as ASCII
            KeyValuePair<string, string> encoding = GetMetadataEntries(new StreamReader(stream, Encoding.ASCII), symbolsConf, readBufferSize)
                .FirstOrDefault(kvp => kvp.Key == symbolsConf.Symbols.KeyWords.CodePage);

            return GetEncodingFromValue(encoding.Value, symbolsConf);
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
        private static Encoding GetEncodingFromValue(string value, PxFileSymbolsConf symbolsConf)
        {
            if (value is null) throw new PxFileStreamException($"Could not find CODEPAGE keyword in the file.");

            string encodingName = value.Trim(symbolsConf.Tokens.Value.StringDelimeter);

            bool isAvailable = Array.Exists(Encoding.GetEncodings(), e => e.Name == encodingName);
            if (!isAvailable) Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            try
            {
                return Encoding.GetEncoding(encodingName);
            }
            catch (ArgumentException aExp)
            {
                throw new PxFileStreamException($"The encoding {encodingName} provided with the CODEPAGE keyword is not available", aExp);
            }
        }

        #endregion
    }
}
