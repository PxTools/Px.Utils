namespace PxUtils.PxFile
{
    public static class CharacterConstants
    {
        public static char[] WhitespaceCharacters { get; } = [SPACE, HORIZONTAL_TAB, CARRIAGE_RETURN, LINE_FEED];

        public static string UnixNewLine => "\n";
        public static string WindowsNewLine => "\r\n";

        public const char SPACE = (char)0x20;
        public const char HORIZONTAL_TAB = (char)0x09;
        public const char CARRIAGE_RETURN = (char)0x0D;
        public const char LINE_FEED = (char)0x0A;

        public readonly static byte[] BOM_UTF_8 = [0xEF, 0xBB, 0xBF];
        public readonly static byte[] BOM_UTF_16 = [0xFE, 0xFF];
        public readonly static byte[] BOM_UTF_32 = [0x00, 0x00, 0xFE];
    }
}
