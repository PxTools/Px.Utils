namespace Px.Utils.PxFile
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

        public readonly static byte[] BOMUTF8 = [0xEF, 0xBB, 0xBF];
        public readonly static byte[] BOMUTF16 = [0xFE, 0xFF];
        public readonly static byte[] BOMUTF32 = [0x00, 0x00, 0xFE];

        public const byte Zero = 0x30;
        public const byte Nine = 0x39;
    }
}
