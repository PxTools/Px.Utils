using System.Text;

namespace PxUtils.PxFile.Data
{
    public static class StreamUtilities
    {
        public static long FindKeywordPosition(Stream stream, string keyword, PxFileSyntaxConf conf, int bufferSize = 4096)
        {
            char entrySeparator = conf.Symbols.SectionSeparator;

            char[] whitespaces = ['\r', '\n', '\t', ' '];

            byte[] keywordBytes = Encoding.ASCII.GetBytes(keyword + conf.Symbols.KeywordSeparator);
            int keywordIndex = 0;
            int lastKeyIndex = keywordBytes.Length - 1;

            bool searchMode = true;

            byte[] buffer = new byte[bufferSize];
            long read;

            do
            {
                read = stream.Read(buffer, 0, bufferSize);
                for(int i = 0; i < read; i++)
                {
                    if (searchMode && !whitespaces.Contains((char)buffer[i]))
                    {
                        if (buffer[i] == keywordBytes[keywordIndex])
                        {
                            if(keywordIndex == lastKeyIndex)
                            {
                                return stream.Position - read + i - lastKeyIndex;
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
            }
            while (read > 0);

            return -1;
        }
    }
}
