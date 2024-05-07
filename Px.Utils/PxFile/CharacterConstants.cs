namespace PxUtils.PxFile
{
    public static class CharacterConstants
    {
        public static char[] WhitespaceCharacters { get; } = [CHARSPACE, CHARHORIZONTALTAB, CHARCARRIAGERETURN, CHARLINEFEED];

        public static string UnixNewLine => "\n";
        public static string WindowsNewLine => "\r\n";

        public const byte SPACE = 0x20;
        public const byte HORIZONTALTAB = 0x09;
        public const byte CARRIAGERETURN = 0x0D;
        public const byte LINEFEED = 0x0A;
        public const byte QUOTATIONMARK = 0x22;
        public const byte SEMICOLON = 0x3B;

        public const char CHARSPACE = (char)SPACE;
        public const char CHARHORIZONTALTAB = (char)HORIZONTALTAB;
        public const char CHARCARRIAGERETURN = (char)CARRIAGERETURN;
        public const char CHARLINEFEED = (char)LINEFEED;
    }
}
