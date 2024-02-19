namespace PxUtils.ModelBuilders
{
    public static class ValueParserUtilities
    {
        public static List<string> ParseStringList(string input, char listSeparator, char stringDelimeter)
        {
            List<string> list = [];

            if(string.IsNullOrEmpty(input)) return list;

            bool inString = false;
            bool newSection = true;
            int strStartIndex = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == stringDelimeter)
                {
                    if(!newSection && !inString) throw new ArgumentException($"Invalid symbol {c} found in input string {input}");
                    inString = !inString;
                    newSection = false;
                }
                else if (!inString && !char.IsWhiteSpace(c))
                {
                    if (c == listSeparator)
                    {
                        list.Add(input[strStartIndex..i].Trim().Trim(stringDelimeter));
                        strStartIndex = i + 1;
                        newSection = true;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid symbol {c} found in input string {input}");
                    }
                }
            }
            if (inString) throw new ArgumentException($"String not closed in input string {input}");
            if (newSection) throw new ArgumentException($"Invalid input string {input}");
            list.Add(input[strStartIndex..].Trim().Trim(stringDelimeter));

            return list;
        }
    }
}
