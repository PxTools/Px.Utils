using Px.Utils.PxFile;
using System.Text;

namespace PxUtils.PxFile.MetadataUtility
{
    internal static class MetadataParsing
    {
        internal static async IAsyncEnumerable<KeyValuePair<string, string>> GetMetadataEntriesAsync(StreamReader inputStream, PxFileSymbolsConf symbolsConf)
        {
            char keywordSeperator = symbolsConf.Tokens.KeywordSeparator;
            char sectionSeparator = symbolsConf.Tokens.SectionSeparator;
            string dataKeyword = symbolsConf.Symbols.KeyWords.Data;

            const int BUFFER_SIZE = 1024;
            char[] buffer = new char[BUFFER_SIZE];
            char nextDelimeter = keywordSeperator;
            bool keyWordMode = true;
            bool endOfMetaSection = false;
            StringBuilder keyWordBldr = new();
            StringBuilder valueStringBldr = new();

            do
            {
                int readChars = await inputStream.ReadAsync(buffer, 0, BUFFER_SIZE);
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
    }
}
