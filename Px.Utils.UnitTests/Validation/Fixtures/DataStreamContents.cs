﻿namespace Px.Utils.UnitTests.Validation.Fixtures
{
    internal static class DataStreamContents
    {
        internal static string  SIMPLE_VALID_DATA =>
            "DATA=\n"+
            "1 2 3 4 5 \r\n" +
            "6 7 8 9 10 \n\r" +
            "\".\" \"..\" \"...\" \"....\" \".....\" \r"+
            "\"......\" \"-\" -1 1.2 -1.3; \r\n";

        internal static string SIMPLE_VALID_DATA_WITHOUT_MISISNG_CODE_DELIMETERS =>
            "DATA=\n" +
            "1 2 3 4 5 \r\n" +
            "6 7 8 9 10 \n\r" +
            ". .. ... .... ..... \r" +
            "...... - -1 1.2 -1.3; \r\n";

        internal static string SIMPLE_INVALID_DATA =>
            "DATA=a\n" +
            "\"b\" 1 2. 3 4 5 \r\n" +
            "6 7 +8\t9 10 \n\r" +
            "\".\" \"..\" \"...\" \"....\" \".....\" \r" +
            "\"dots\" \"-\" \"..123\" -1 1.2 -1.3 \r\n" +
            "1 2 \0 4 5 \r\n;";

        internal static string NO_DATA =>
            "DATA=\n";

        internal static string DATA_ON_SINGLE_ROW =>
            "DATA=1 2 3 4 5 6 7 8 9 10;";
    }
}