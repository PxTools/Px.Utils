namespace Px.Utils.UnitTests.PxFileTests.Fixtures
{
    internal static class MinimalPx
    {
        internal static string MINIMAL_UTF8_N =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[åå]=\"test\";\nCOPYRIGHT=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string MINIMAL_UTF8_RN =>
            "CHARSET=\"ANSI\";\r\nAXIS-VERSION=\"2013\";\r\nCODEPAGE=\"utf-8\";\r\nLANGUAGES=\"aa\",\"åå\",\"öö\";\r\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\r\nSUBJECT-AREA=\"test\";\r\nSUBJECT-AREA[åå]=\"test\";\r\nCOPYRIGHT=YES;\r\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string MINIMAL_ISO_8859_15_N =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"iso-8859-15\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[åå]=\"test\";\nCOPYRIGHT=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string MINIMAL_ISO_8859_1_RN =>
            "CHARSET=\"ANSI\";\r\nAXIS-VERSION=\"2013\";\r\nCODEPAGE=\"iso-8859-1\";\r\nLANGUAGES=\"aa\",\"åå\",\"öö\";\r\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\r\nSUBJECT-AREA=\"test\";\r\nSUBJECT-AREA[åå]=\"test\";\r\nCOPYRIGHT=YES;\r\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string MINIMAL_UTF8_N_WITH_DELIMETERS_IN_VALUESTRING =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013;2014\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[åå]=\"test=a;\";\nCOPYRIGHT=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string BROKEN_UTF8_N =>
            "\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[åå]=\"test\";\nCOPYRIGHT=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string MINIMAL_UTF8_N_WITH_MULTILINE_VALUES =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\n\"åå\",\n\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[åå]=\"test\";\nCOPYRIGHT=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,\n12,13,14,15,16,17,18,19,20;";
    }
}
