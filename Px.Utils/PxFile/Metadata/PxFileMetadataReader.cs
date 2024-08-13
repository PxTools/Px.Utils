﻿using Px.Utils.Exceptions;
using System.Runtime.CompilerServices;
using System.Text;

namespace Px.Utils.PxFile.Metadata
{
    /// <summary>
    /// Provides methods to read metadata and get the encoding from a stream of a Px file.
    /// </summary>
    public class PxFileMetadataReader : IPxFileMetadataReader
    {
        private const int DEFAULT_READ_BUFFER_SIZE = 4096;

        /// <summary>
        /// Processes a provided stream to extract metadata, returning the results as an IEnumerable of key-value pairs.
        /// The method uses the specified encoding and symbols configuration (or defaults if not provided) to interpret the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the metadata.</param>
        /// <param name="encoding">The encoding to use when reading the stream.</param>
        /// <param name="syntaxConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <param name="readBufferSize">The size of the buffer to use when reading the stream. If not specified, the default buffer size is used.</param>
        /// <returns>An IEnumerable of key-value pairs representing the metadata entries in the file.</returns>
        public IEnumerable<KeyValuePair<string, string>> ReadMetadata(Stream stream, Encoding encoding, PxFileSyntaxConf? syntaxConf = null, int readBufferSize = DEFAULT_READ_BUFFER_SIZE)
        {
            syntaxConf ??= PxFileSyntaxConf.Default;

            char keywordSeperator = syntaxConf.Symbols.KeywordSeparator;
            char sectionSeparator = syntaxConf.Symbols.EntrySeparator;
            char stringDelimeter = syntaxConf.Symbols.Value.StringDelimeter;
            string dataKeyword = syntaxConf.Tokens.KeyWords.Data;

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
                        if (keyWordMode && buffer[i] == sectionSeparator || !keyWordMode && buffer[i] == keywordSeperator)
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
        /// <param name="syntaxConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <param name="readBufferSize">The size of the buffer to use when reading the stream. If not specified, the default buffer size is used.</param>
        /// <param name="cancellationToken">Can be used to cancel the operation.</param>
        /// <returns>An IAsyncEnumerable of key-value pairs representing the metadata entries in the file. This can be asynchronously iterated over as the data is read.</returns>
        public async IAsyncEnumerable<KeyValuePair<string, string>> ReadMetadataAsync(
            Stream stream,
            Encoding encoding,
            PxFileSyntaxConf? syntaxConf = null,
            int readBufferSize = DEFAULT_READ_BUFFER_SIZE,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            syntaxConf ??= PxFileSyntaxConf.Default;

            char keywordSeperator = syntaxConf.Symbols.KeywordSeparator;
            char sectionSeparator = syntaxConf.Symbols.EntrySeparator;
            char stringDelimeter = syntaxConf.Symbols.Value.StringDelimeter;
            string dataKeyword = syntaxConf.Tokens.KeyWords.Data;

            char[] readBuffer = new char[readBufferSize];
            char[] parsingBuffer = new char[readBufferSize];

            StreamReader reader = new(stream, encoding);

            int readChars;
            bool keyWordMode = true;
            bool readingValueString = false;
            bool endOfMetaSection = false;

            StringBuilder keyWordBldr = new();
            StringBuilder valueStringBldr = new();

            while ((readChars = await reader.ReadAsync(readBuffer, 0, readBufferSize)) > 0)
            {
                (readBuffer, parsingBuffer) = (parsingBuffer, readBuffer);

                int lastDelimeterIndx = -1;

                for (int i = 0; i < readChars; i++)
                {
                    if (parsingBuffer[i] == stringDelimeter) readingValueString = !readingValueString;
                    if (!readingValueString)
                    {
                        if (keyWordMode && parsingBuffer[i] == sectionSeparator || !keyWordMode && parsingBuffer[i] == keywordSeperator)
                        {
                            throw new InvalidPxFileMetadataException($"Unexpected character '{parsingBuffer[i]}' found at position {i}.");
                        }

                        if (parsingBuffer[i] == keywordSeperator || parsingBuffer[i] == sectionSeparator)
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
                        }
                    }
                }
                Append(parsingBuffer, lastDelimeterIndx + 1, readChars, keyWordMode, keyWordBldr, valueStringBldr);

                if (endOfMetaSection || cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Reads the metadata from the provided stream and returns it as a dictionary of key-value pairs.
        /// The method uses the specified encoding and symbols configuration (or defaults if not provided) to interpret the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the metadata.</param>
        /// <param name="encoding">The encoding to use when reading the stream.</param>
        /// <param name="syntaxConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <param name="readBufferSize">The size of the buffer to use when reading the stream. If not specified, the default buffer size is used.</param>
        /// <returns>A dictionary containing the metadata entries in the file.</returns>
        public Dictionary<string, string> ReadMetadataToDictionary(
            Stream stream,
            Encoding encoding,
            PxFileSyntaxConf? syntaxConf = null,
            int readBufferSize = DEFAULT_READ_BUFFER_SIZE)
            => new(ReadMetadata(stream, encoding, syntaxConf, readBufferSize));

        /// <summary>
        /// Asynchronously reads the metadata from the provided stream and returns it as a dictionary of key-value pairs.
        /// The method uses the specified encoding and symbols configuration (or defaults if not provided) to interpret the stream.
        /// </summary>
        /// <param name="stream">The stream from which to read the metadata.</param>
        /// <param name="encoding">The encoding to use when reading the stream.</param>
        /// <param name="syntaxConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <param name="readBufferSize">The size of the buffer to use when reading the stream. If not specified, the default buffer size is used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task that represents the asynchronous operation. The Task result contains a dictionary of the metadata entries in the file.</returns>
        public async Task<Dictionary<string, string>> ReadMetadataToDictionaryAsync(
            Stream stream,
            Encoding encoding,
            PxFileSyntaxConf? syntaxConf = null,
            int readBufferSize = DEFAULT_READ_BUFFER_SIZE,
            CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> metaDict = [];
            ConfiguredCancelableAsyncEnumerable<KeyValuePair<string, string>> metaEnumerable =
                ReadMetadataAsync(stream, encoding, syntaxConf, readBufferSize, cancellationToken)
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
        /// <param name="syntaxConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <returns>The determined encoding of the stream.</returns>
        public Encoding GetEncoding(Stream stream, PxFileSyntaxConf? syntaxConf = null)
        {
            syntaxConf ??= PxFileSyntaxConf.Default;
            long position = stream.Position;

            byte[] bom = new byte[3];
            stream.Read(bom);

            stream.Position = position;

            if (GetBom(bom) is Encoding utf) return utf;

            // Use ASCII because encoding is still unknown, CODEPAGE keyword is readable as ASCII
            KeyValuePair<string, string> encoding = ReadMetadata(stream, Encoding.ASCII, syntaxConf, 512)
                .FirstOrDefault(kvp => kvp.Key == syntaxConf.Tokens.KeyWords.CodePage);

            return GetEncodingFromValue(encoding.Value, syntaxConf);
        }

        /// <summary>
        /// Asynchronously determines the encoding of the provided stream based on the Byte Order Mark (BOM) or the CODEPAGE keyword in the metadata.
        /// If no BOM or CODEPAGE keyword is found, an exception is thrown. If the encoding specified by the CODEPAGE keyword is not available,
        /// it attempts to register it using the CodePagesEncodingProvider.
        /// </summary>
        /// <param name="stream">The stream from which to determine the encoding.</param>
        /// <param name="syntaxConf">The symbols configuration to use when reading the metadata. If not specified the default configuration is used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A Task that represents the asynchronous operation. The Task result contains the determined encoding of the stream.</returns>
        public async Task<Encoding> GetEncodingAsync(Stream stream, PxFileSyntaxConf? syntaxConf = null, CancellationToken cancellationToken = default)
        {
            const int bomLength = 3;
            syntaxConf ??= PxFileSyntaxConf.Default;
            long position = stream.Position;

            byte[] bom = new byte[bomLength];
            await stream.ReadAsync(bom.AsMemory(0, bomLength), cancellationToken);

            stream.Position = position;

            if (GetBom(bom) is Encoding utf) return utf;

            // Use ASCII because encoding is still unknown, CODEPAGE keyword is readable as ASCII
            KeyValuePair<string, string> encoding = await ReadMetadataAsync(stream, Encoding.ASCII, syntaxConf, 512, cancellationToken)
                .FirstOrDefaultAsync(kvp => kvp.Key == syntaxConf.Tokens.KeyWords.CodePage, cancellationToken);

            return GetEncodingFromValue(encoding.Value, syntaxConf);
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
            if (buffer.Take(CharacterConstants.BOMUTF8.Length).SequenceEqual(CharacterConstants.BOMUTF8))
            {
                return Encoding.UTF8;
            }
            else if (buffer.Take(CharacterConstants.BOMUTF16.Length).SequenceEqual(CharacterConstants.BOMUTF16))
            {
                return Encoding.Unicode;
            }
            else if (buffer.Take(CharacterConstants.BOMUTF32.Length).SequenceEqual(CharacterConstants.BOMUTF32))
            {
                return Encoding.UTF32;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Encoding GetEncodingFromValue(string value, PxFileSyntaxConf syntaxConf)
        {
            if (value is null) throw new InvalidPxFileMetadataException($"Could not find CODEPAGE keyword in the file.");

            string encodingName = value.Trim(syntaxConf.Symbols.Value.StringDelimeter);

            bool isAvailable = Array.Exists(Encoding.GetEncodings(), e => e.Name == encodingName);
            if (!isAvailable) Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            try
            {
                return Encoding.GetEncoding(encodingName);
            }
            catch (ArgumentException aExp)
            {
                throw new InvalidPxFileMetadataException($"The encoding {encodingName} provided with the CODEPAGE keyword is not available", aExp);
            }
        }

        #endregion
    }
}
