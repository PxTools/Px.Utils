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

        internal static string MINIMAL_UTF8_20DECIMALVALUES =>
            "CHARSET=\"Unicode\";\n" +
            "AXIS-VERSION=\"2013\";\n" +
            "CODEPAGE=\"utf-8\";\n" +
            "LANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\n" +
            "SUBJECT-AREA=\"test\";\n" +
            "SUBJECT-AREA[åå]=\"test\";\n" +
            "COPYRIGHT=YES;\n" +
            "DATA=\n" +
            "0.0 0.01 0.02 0.03 0.04 0.05 0.06 0.07 0.08 0.09 \n" +
            "0.1 0.11 0.12 0.13 0.14 0.15 0.16 0.17 0.18 0.19;";
    }
}
