﻿using Px.Utils.PxFile.Exceptions;
using System.Text;

namespace PxUtils.PxFile.MetadataUtility
{
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

            char[] buffer = new char[readBufferSize];
            char nextDelimeter = keywordSeperator;
            bool keyWordMode = true;
            bool endOfMetaSection = false;

            StringBuilder keyWordBldr = new();
            StringBuilder valueStringBldr = new();

            do
            {
                int readChars = await inputStream.ReadAsync(buffer, 0, readBufferSize);
                int lastDelimeterIndx = -1;

                for (int i = 0; i < readChars; i++)
                {
                    if (buffer[i] == nextDelimeter)
                    {
                        Append(lastDelimeterIndx + 1, i);
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
                Append(lastDelimeterIndx + 1, readChars);

            } while (!endOfMetaSection && !inputStream.EndOfStream);

            void Append(int start, int endIndex)
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

            // UTF8 BOM
            if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
            {
                return Encoding.UTF8;
            }

            // UTF16 BOM
            else if (bom[0] == 0xFE && bom[1] == 0xFF)
            {
                return Encoding.Unicode;
            }

            // UTF32 BOM
            else if (bom[0] == 0x00 && bom[1] == 0x00 && bom[2] == 0xFE)
            {
                return Encoding.UTF32;
            }

            PxFileSymbolsConf symbolsConf = PxFileSymbolsConf.Default;

            using CancellationTokenSource ctSource = new();
            ctSource.CancelAfter(CODEPAGE_SEARCH_TIMEOUT_MS);

            // Use ASCII because encoding is still unknown, CODEPAGE keyword is readable as ASCII
            KeyValuePair<string, string> encoding = await GetMetadataEntriesAsync(new StreamReader(stream, Encoding.ASCII), symbolsConf, readBufferSize)
                .FirstOrDefaultAsync(kvp => kvp.Key == symbolsConf.Symbols.KeyWords.CodePage, ctSource.Token);

            if (encoding.Value is null) throw new PxFileStreamException($"Could not find CODEPAGE keyword in the file.");

            string encodingName = encoding.Value.Trim(symbolsConf.Tokens.Value.StringDelimeter);

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
    }
}
