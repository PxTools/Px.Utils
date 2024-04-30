using PxUtils.Validation.DataValidation;

namespace Px.Utils.UnitTests.Validation.Fixtures
{
    internal static class DataStreamContents
    {
        internal static string  SIMPLE_VALID_DATA =>
            "DATA=\n"+
            "1 2 3 4 5 \r\n" +
            "6 7 8 9 10 \n\r" +
            "\".\" \"..\" \"...\" \"....\" \".....\" \r"+
            "\"......\" \"-\" -1 1.2 -1.3; \r\n";

        internal static string  SIMPLE_INVALID_DATA =>
            "DATA=\n"+
            "1 2. 3 4 5 \r\n" +
            "6 7 +8\t9 10 \n\r" +
            "\".\" \"..\" \"...\" \"....\" \".....\" \r"+
            "\"dots\" \"-\" \"..123\" -1 1.2 -1.3; \n";

        internal static Token[] EXPECTED_SIMPLE_VALID_DATA_TOKENS =
        [
            new Token(TokenType.NumDataItem, "1", 1, 1),
            new Token(TokenType.DataItemSeparator, " ", 1, 2),
            new Token(TokenType.NumDataItem, "2", 1, 3),
            new Token(TokenType.DataItemSeparator, " ", 1, 4),
            new Token(TokenType.NumDataItem, "3", 1, 5),
            new Token(TokenType.DataItemSeparator, " ", 1, 6),
            new Token(TokenType.NumDataItem, "4", 1, 7),
            new Token(TokenType.DataItemSeparator, " ", 1, 8),
            new Token(TokenType.NumDataItem, "5", 1, 9),
            new Token(TokenType.DataItemSeparator, " ", 1, 10),
            new Token(TokenType.LineSeparator, "\r\n", 1, 11),
            new Token(TokenType.NumDataItem, "6", 2, 1),
            new Token(TokenType.DataItemSeparator, " ", 2, 2),
            new Token(TokenType.NumDataItem, "7", 2, 3),
            new Token(TokenType.DataItemSeparator, " ", 2, 4),
            new Token(TokenType.NumDataItem, "8", 2, 5),
            new Token(TokenType.DataItemSeparator, " ", 2, 6),
            new Token(TokenType.NumDataItem, "9", 2, 7),
            new Token(TokenType.DataItemSeparator, " ", 2, 8),
            new Token(TokenType.NumDataItem, "10", 2, 9),
            new Token(TokenType.DataItemSeparator, " ", 2, 11),
            new Token(TokenType.LineSeparator, "\n\r", 2, 12),
            new Token(TokenType.StringDataItem, "\".\"", 3, 1),
            new Token(TokenType.DataItemSeparator, " ", 3, 4),
            new Token(TokenType.StringDataItem, "\"..\"", 3, 5),
            new Token(TokenType.DataItemSeparator, " ", 3, 9),
            new Token(TokenType.StringDataItem, "\"...\"", 3, 10),
            new Token(TokenType.DataItemSeparator, " ", 3, 15),
            new Token(TokenType.StringDataItem, "\"....\"", 3, 16),
            new Token(TokenType.DataItemSeparator, " ", 3, 22),
            new Token(TokenType.StringDataItem, "\".....\"", 3, 23),
            new Token(TokenType.DataItemSeparator, " ", 3, 30),
            new Token(TokenType.LineSeparator, "\r", 3, 31),
            new Token(TokenType.StringDataItem, "\"......\"", 4, 1),
            new Token(TokenType.DataItemSeparator, " ", 4, 9),
            new Token(TokenType.StringDataItem, "\"-\"", 4, 10),
            new Token(TokenType.DataItemSeparator, " ", 4, 13),
            new Token(TokenType.NumDataItem, "-1", 4, 14),
            new Token(TokenType.DataItemSeparator, " ", 4, 16),
            new Token(TokenType.NumDataItem, "1.2", 4, 17),
            new Token(TokenType.DataItemSeparator, " ", 4, 20),
            new Token(TokenType.NumDataItem, "-1.3", 4, 21),
            new Token(TokenType.EndOfData, ";", 4, 25),
            new Token(TokenType.DataItemSeparator, " ", 4, 26),
            new Token(TokenType.LineSeparator, "\r\n", 4, 27),
            new Token(TokenType.EndOfStream, "", 5, 0)    
            
        ];
    }
}