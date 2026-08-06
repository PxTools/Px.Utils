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
                state.ResetEntry();
                return false;
            }
            if (state.IsAtEntryStart && IsWhitespace(currentByte)) return false;
            if (state.IsAtEntryStart && state.MatchedKeywordBytes < state.DataKeywordBytes.Length && currentByte == state.DataKeywordBytes[state.MatchedKeywordBytes])
            {
                state.MatchedKeywordBytes++;
                return false;
            }
            if (state.MatchedKeywordBytes == state.DataKeywordBytes.Length && currentByte == state.KeywordSeparator)
            {
                state.IsAfterDataKeyword = true;
                return false;
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
            public readonly byte[] DataKeywordBytes = dataKeywordBytes;
            public readonly byte EntrySeparator = entrySeparator;
            public readonly byte KeywordSeparator = keywordSeparator;
            public readonly byte StringDelimiter = stringDelimiter;
            public int MatchedKeywordBytes;
            public bool IsAtEntryStart = true;
            public bool IsInString;
            public bool IsAfterDataKeyword;
            public bool IsDataEntryEmpty;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ResetEntry()
            {
                MatchedKeywordBytes = 0;
                IsAtEntryStart = true;
            }
        }
    }
}
