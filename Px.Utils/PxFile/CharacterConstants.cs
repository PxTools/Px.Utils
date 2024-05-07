namespace PxUtils.PxFile
{
    public static class CharacterConstants
    {
        public static char[] WhitespaceCharacters { get; } = [SPACE, HORIZONTALTAB, CARRIAGERETURN, LINEFEED];

        public static string UnixNewLine => "\n";
        public static string WindowsNewLine => "\r\n";

        public const char SPACE = (char)0x20;
        public const char HORIZONTALTAB = (char)0x09;
        public const char CARRIAGERETURN = (char)0x0D;
        public const char LINEFEED = (char)0x0A;

        public readonly static byte[] BOMUTF8 = [0xEF, 0xBB, 0xBF];
        public readonly static byte[] BOMUTF16 = [0xFE, 0xFF];
        public readonly static byte[] BOMUTF32 = [0x00, 0x00, 0xFE];
    }
}
