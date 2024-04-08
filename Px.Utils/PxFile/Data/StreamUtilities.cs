using System.Runtime.CompilerServices;
using System.Text;

namespace PxUtils.PxFile.Data
{
    public static class StreamUtilities
    {
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

        public async static Task<long> FindKeywordPositionAsync(Stream stream, string keyword, PxFileSyntaxConf conf, int bufferSize = 4096)
        {
            char entrySeparator = conf.Symbols.EntrySeparator;

            byte[] keywordBytes = Encoding.ASCII.GetBytes(keyword + conf.Symbols.KeywordSeparator);
            byte[] buffer = new byte[bufferSize];

            long read;
            int keywordIndex = 0;
            bool searchMode = true;

            do
            {
                read = await stream.ReadAsync(buffer.AsMemory(0, bufferSize));
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
