namespace PxFileTests.Fixtures
{
    internal static class DataReaderFixtures
    {
        internal static string MINIMAL_UTF8_20DATAVALUES =>
            "CHARSET=\"Unicode\";\n" +
            "AXIS-VERSION=\"2013\";\n" +
            "CODEPAGE=\"utf-8\";\n" +
            "LANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\n" +
            "SUBJECT-AREA=\"test\";\n" +
            "SUBJECT-AREA[åå]=\"test\";\n" +
            "COPYRIGHT=YES;\n" +
            "DATA=\n" +
            "0 1 2 3 4 5 6 7 8 9 \n" +
            "10 11 12 13 14 15 16 17 18 19;";
    }
}
