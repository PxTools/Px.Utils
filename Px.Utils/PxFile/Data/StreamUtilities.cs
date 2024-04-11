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
            return FindKeywordPostionImpl(stream, keyword, conf, bufferSize);
        }

        /// <summary>
        /// Asynchronously finds the position of the first occurrence of a specified keyword in a given stream.
        /// </summary>
        /// <param name="stream">The stream to search in.</param>
        /// <param name="keyword">The keyword to search for.</param>
        /// <param name="conf">A configuration object that contains symbols used in the px file syntax.</param>
        /// <param name="cToken">A token that can be used to cancel the operation. Defaults to None.</param>
        /// <param name="bufferSize">The size of the buffer to use when reading from the stream. Defaults to 4096.</param>
        /// <returns>The task result contains the position of the keyword in the stream if found, otherwise -1.</returns>
        public async static Task<long> FindKeywordPositionAsync(Stream stream, string keyword, PxFileSyntaxConf conf, CancellationToken? cToken = null, int bufferSize = 4096)
        {
            return await Task.Factory.StartNew(
                () => FindKeywordPostionImpl(stream, keyword, conf, bufferSize, cToken), cToken
                ?? CancellationToken.None
            );
        }

        private static long FindKeywordPostionImpl(Stream stream, string keyword, PxFileSyntaxConf conf, int bufferSize = 4096, CancellationToken? cancellationToken = null)
        {
            char entrySeparator = conf.Symbols.EntrySeparator;

            byte[] keywordBytes = Encoding.ASCII.GetBytes(keyword + conf.Symbols.KeywordSeparator);
            byte[] buffer = new byte[bufferSize];

            long read;
            int keywordIndex = 0;
            int lastKeyIndex = keywordBytes.Length - 1;
            bool searchMode = true;

            do
            {
                cancellationToken?.ThrowIfCancellationRequested();
                read = stream.Read(buffer, 0, bufferSize);

                for (int i = 0; i < read; i++)
                {
                    if (searchMode && !CharacterConstants.WhitespaceCharacters.Contains((char)buffer[i]))
                    {
                        if (buffer[i] == keywordBytes[keywordIndex])
                        {
                            if (keywordIndex == lastKeyIndex) return stream.Position - read + i - keyword.Length;
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
            }
            while (read > 0);

            return -1;
        }
    }
}
