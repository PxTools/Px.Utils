using System.Runtime.CompilerServices;
using System.Text;

namespace PxUtils.PxFile.Data
{
    /// <summary>
    /// Utility class for working with px file <see cref="Stream"/> instances.
    /// </summary>
    public static class StreamUtilities
    {
        /// <summary>
        /// Finds the position of the first occurrence of a specified keyword in a given stream.
        /// </summary>
        /// <param name="stream">The stream to search in.</param>
        /// <param name="keyword">The keyword to search for.</param>
        /// <param name="conf">A configuration object that contains symbols used in the px file syntax.</param>
        /// <param name="bufferSize">The size of the buffer to use when reading from the stream. Defaults to 4096.</param>
        /// <returns>The position of the keyword in the stream if found, otherwise -1.</returns>
        public static long FindKeywordPosition(Stream stream, string keyword, PxFileSyntaxConf conf, int bufferSize = 4096)
        {
            char entrySeparator = conf.Symbols.EntrySeparator;

            byte[] keywordBytes = Encoding.ASCII.GetBytes(keyword + conf.Symbols.KeywordSeparator);
            byte[] buffer = new byte[bufferSize];

            long read;
            int keywordIndex = 0;
            bool searchMode = true;

            do
            {
                read = stream.Read(buffer, 0, bufferSize);
                long indexInBuffer = FindInBuffer(read, buffer, keywordBytes, entrySeparator, ref searchMode, ref keywordIndex);
                if (indexInBuffer >= 0) return stream.Position - read + indexInBuffer;
            }
            while (read > 0);

            return -1;
        }

        /// <summary>
        /// Asynchronously finds the position of the first occurrence of a specified keyword in a given stream.
        /// </summary>
        /// <param name="stream">The stream to search in.</param>
        /// <param name="keyword">The keyword to search for.</param>
        /// <param name="conf">A configuration object that contains symbols used in the px file syntax.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the operation. Defaults to None.</param>
        /// <param name="bufferSize">The size of the buffer to use when reading from the stream. Defaults to 4096.</param>
        /// <returns>The task result contains the position of the keyword in the stream if found, otherwise -1.</returns>
        public async static Task<long> FindKeywordPositionAsync(Stream stream, string keyword, PxFileSyntaxConf conf, CancellationToken? cancellationToken = null, int bufferSize = 4096)
        {
            char entrySeparator = conf.Symbols.EntrySeparator;

            byte[] keywordBytes = Encoding.ASCII.GetBytes(keyword + conf.Symbols.KeywordSeparator);
            byte[] buffer = new byte[bufferSize];

            long read;
            int keywordIndex = 0;
            bool searchMode = true;

            do
            {
                read = await stream.ReadAsync(buffer.AsMemory(0, bufferSize), cancellationToken ?? CancellationToken.None);
                long indexInBuffer = FindInBuffer(read, buffer, keywordBytes, entrySeparator, ref searchMode, ref keywordIndex);
                if(indexInBuffer >= 0) return stream.Position - read + indexInBuffer;
            }
            while (read > 0);

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long FindInBuffer(long read, byte[] buffer, byte[] keywordBytes, char entrySeparator, ref bool searchMode, ref int keywordIndex)
        {
            int lastKeyIndex = keywordBytes.Length - 1;

            for (int i = 0; i < read; i++)
            {
                if (searchMode && !CharacterConstants.WhitespaceCharacters.Contains((char)buffer[i]))
                {
                    if (buffer[i] == keywordBytes[keywordIndex])
                    {
                        if (keywordIndex == lastKeyIndex)
                        {
                            return i - lastKeyIndex;
                        }
                        else keywordIndex++;
                    }
                    else
                    {
                        searchMode = false;
                        keywordIndex = 0;
                    }
                }
                else if (buffer[i] == entrySeparator)
                {
                    searchMode = true;
                }
            }

            return -1;
        }
    }
}
