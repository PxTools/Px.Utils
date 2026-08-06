using System.Runtime.CompilerServices;
using System.Text;

namespace Px.Utils.PxFile.Data
{
    /// <summary>
    /// Utility class for working with px file <see cref="Stream"/> instances.
    /// </summary>
    public static class StreamUtilities
    {
        /// <summary>
        /// Finds the absolute raw byte offset of the first occurrence of a keyword at the start of a top-level PX entry.
        /// </summary>
        /// <param name="stream">The PX file stream to search from its current position.</param>
        /// <param name="keyword">The keyword to search for.</param>
        /// <param name="conf">A configuration object that contains PX syntax symbols.</param>
        /// <param name="bufferSize">The size of the buffer to use when reading from the stream. Defaults to 4096.</param>
        /// <returns>The absolute raw byte offset of the keyword, or -1 when the keyword cannot be found.</returns>
        public static long FindKeywordPosition(Stream stream, string keyword, PxFileConfiguration conf, int bufferSize = 4096)
        {
            return FindKeywordPositionImpl(stream, keyword, conf, bufferSize);
        }

        /// <summary>
        /// Asynchronously finds the absolute raw byte offset of the first occurrence of a keyword at the start of a top-level PX entry.
        /// </summary>
        /// <param name="stream">The PX file stream to search from its current position.</param>
        /// <param name="keyword">The keyword to search for.</param>
        /// <param name="conf">A configuration object that contains PX syntax symbols.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
        /// <param name="bufferSize">The size of the buffer to use when reading from the stream. Defaults to 4096.</param>
        /// <returns>The absolute raw byte offset of the keyword, or -1 when the keyword cannot be found.</returns>
        public static Task<long> FindKeywordPositionAsync(Stream stream, string keyword, PxFileConfiguration conf, CancellationToken? cancellationToken = null, int bufferSize = 4096)
        {
            return FindKeywordPositionImplAsync(stream, keyword, conf, bufferSize, cancellationToken ?? CancellationToken.None);
        }

        /// <summary>
        /// Finds the absolute raw byte offset of the first non-whitespace data value after a top-level DATA entry.
        /// The stream position is restored before this method returns. Returns -1 when the DATA entry or its first value cannot be found.
        /// </summary>
        /// <param name="stream">The seekable PX file stream to search from its origin.</param>
        /// <param name="conf">A configuration object that contains the DATA keyword and PX syntax symbols.</param>
        /// <param name="bufferSize">The size of the buffer to use when reading from the stream. Defaults to 4096.</param>
        /// <returns>The absolute raw byte offset of the first data value, or -1.</returns>
        public static long FindDataStartPosition(Stream stream, PxFileConfiguration conf, int bufferSize = 4096)
        {
            long originalPosition = stream.Position;
            try
            {
                stream.Position = 0;
                return FindDataStartPositionImpl(stream, conf, bufferSize);
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        /// <summary>
        /// Asynchronously finds the absolute raw byte offset of the first non-whitespace data value after a top-level DATA entry.
        /// The stream position is restored before this method returns. Returns -1 when the DATA entry or its first value cannot be found.
        /// </summary>
        /// <param name="stream">The seekable PX file stream to search from its origin.</param>
        /// <param name="conf">A configuration object that contains the DATA keyword and PX syntax symbols.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
        /// <param name="bufferSize">The size of the buffer to use when reading from the stream. Defaults to 4096.</param>
        /// <returns>The absolute raw byte offset of the first data value, or -1.</returns>
        public static async Task<long> FindDataStartPositionAsync(Stream stream, PxFileConfiguration conf, int bufferSize = 4096, CancellationToken cancellationToken = default)
        {
            long originalPosition = stream.Position;
            try
            {
                stream.Position = 0;
                return await FindDataStartPositionImplAsync(stream, conf, bufferSize, cancellationToken);
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        private static long FindKeywordPositionImpl(Stream stream, string keyword, PxFileConfiguration conf, int bufferSize)
        {
            byte[] keywordBytes = Encoding.ASCII.GetBytes(keyword + conf.Symbols.KeywordSeparator);
            byte[] buffer = new byte[bufferSize];
            KeywordSearchState state = new(keywordBytes, (byte)conf.Symbols.EntrySeparator);

            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                long bufferStart = stream.Position - bytesRead;
                long keywordPosition = FindKeywordPosition(buffer.AsSpan(0, bytesRead), bufferStart, ref state);
                if (keywordPosition >= 0)
                {
                    return keywordPosition;
                }
            }

            return -1;
        }

        private static async Task<long> FindKeywordPositionImplAsync(Stream stream, string keyword, PxFileConfiguration conf, int bufferSize, CancellationToken cancellationToken)
        {
            byte[] keywordBytes = Encoding.ASCII.GetBytes(keyword + conf.Symbols.KeywordSeparator);
            byte[] buffer = new byte[bufferSize];
            KeywordSearchState state = new(keywordBytes, (byte)conf.Symbols.EntrySeparator);

            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(), cancellationToken)) > 0)
            {
                long bufferStart = stream.Position - bytesRead;
                long keywordPosition = FindKeywordPosition(buffer.AsSpan(0, bytesRead), bufferStart, ref state);
                if (keywordPosition >= 0)
                {
                    return keywordPosition;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long FindKeywordPosition(ReadOnlySpan<byte> buffer, long bufferStart, ref KeywordSearchState state)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (TryProcessKeywordByte(buffer[i], ref state))
                {
                    return bufferStart + i - state.KeywordBytes.Length + 1;
                }
            }

            return -1;
        }

        private static long FindDataStartPositionImpl(Stream stream, PxFileConfiguration conf, int bufferSize)
        {
            byte[] dataKeywordBytes = Encoding.ASCII.GetBytes(conf.Tokens.KeyWords.Data);
            byte[] buffer = new byte[bufferSize];
            DataStartSearchState state = new(
                dataKeywordBytes,
                (byte)conf.Symbols.EntrySeparator,
                (byte)conf.Symbols.KeywordSeparator,
                (byte)conf.Symbols.Key.StringDelimeter);

            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                long bufferStart = stream.Position - bytesRead;
                if (TryFindDataStartPosition(
                    buffer.AsSpan(0, bytesRead), 
                    bufferStart,
                    ref state, 
                    out long dataStartPosition))
                {
                    return dataStartPosition;
                }
            }

            return -1;
        }

        private static async Task<long> FindDataStartPositionImplAsync(Stream stream, PxFileConfiguration conf, int bufferSize, CancellationToken cancellationToken)
        {
            byte[] dataKeywordBytes = Encoding.ASCII.GetBytes(conf.Tokens.KeyWords.Data);
            byte[] buffer = new byte[bufferSize];
            DataStartSearchState state = new(
                dataKeywordBytes,
                (byte)conf.Symbols.EntrySeparator,
                (byte)conf.Symbols.KeywordSeparator,
                (byte)conf.Symbols.Key.StringDelimeter);

            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(), cancellationToken)) > 0)
            {
                long bufferStart = stream.Position - bytesRead;
                if (TryFindDataStartPosition(
                    buffer.AsSpan(0, bytesRead), 
                    bufferStart, 
                    ref state, 
                    out long dataStartPosition))
                {
                    return dataStartPosition;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFindDataStartPosition(
            ReadOnlySpan<byte> buffer, 
            long bufferStart, 
            ref DataStartSearchState state, 
            out long dataStartPosition)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (TryProcessDataStartByte(buffer[i], ref state))
                {
                    dataStartPosition = bufferStart + i;
                    return true;
                }
                if (state.IsDataEntryEmpty)
                {
                    dataStartPosition = -1;
                    return true;
                }
            }

            dataStartPosition = -1;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryProcessDataStartByte(byte currentByte, ref DataStartSearchState state)
        {
            if (state.IsAfterDataKeyword)
            {
                if (currentByte == state.EntrySeparator)
                {
                    state.IsDataEntryEmpty = true;
                    return false;
                }
                return !IsWhitespace(currentByte);
            }
            if (state.IsInString)
            {
                state.IsInString = currentByte != state.StringDelimiter;
                return false;
            }
            if (currentByte == state.StringDelimiter)
            {
                state.IsInString = true;
                return false;
            }
            if (currentByte == state.EntrySeparator)
            {
                state.KeywordSearchState.Reset();
                return false;
            }
            if (TryProcessKeywordByte(currentByte, ref state.KeywordSearchState))
            {
                state.IsAfterDataKeyword = true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryProcessKeywordByte(byte currentByte, ref KeywordSearchState state)
        {
            if (currentByte == state.EntrySeparator)
            {
                state.Reset();
                return false;
            }
            if (!state.IsAtEntryStart || IsWhitespace(currentByte))
            {
                return false;
            }
            if (state.MatchedKeywordBytes < state.KeywordBytes.Length && currentByte == state.KeywordBytes[state.MatchedKeywordBytes])
            {
                state.MatchedKeywordBytes++;
                return state.MatchedKeywordBytes == state.KeywordBytes.Length;
            }

            state.IsAtEntryStart = false;
            state.MatchedKeywordBytes = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsWhitespace(byte value)
        {
            return value is CharacterConstants.SPACE or CharacterConstants.HORIZONTALTAB or CharacterConstants.CARRIAGERETURN or CharacterConstants.LINEFEED;
        }

        private struct DataStartSearchState(byte[] dataKeywordBytes, byte entrySeparator, byte keywordSeparator, byte stringDelimiter)
        {
            public readonly byte EntrySeparator = entrySeparator;
            public readonly byte StringDelimiter = stringDelimiter;
            public KeywordSearchState KeywordSearchState = new([.. dataKeywordBytes, keywordSeparator], entrySeparator);
            public bool IsInString;
            public bool IsAfterDataKeyword;
            public bool IsDataEntryEmpty;
        }

        private struct KeywordSearchState(byte[] keywordBytes, byte entrySeparator)
        {
            public readonly byte[] KeywordBytes = keywordBytes;
            public readonly byte EntrySeparator = entrySeparator;
            public int MatchedKeywordBytes;
            public bool IsAtEntryStart = true;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                MatchedKeywordBytes = 0;
                IsAtEntryStart = true;
            }
        }
    }
}
