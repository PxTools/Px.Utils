namespace Px.Utils.UnitTests.Validation.Fixtures
{
    internal static class PxFileFixtures
    {
        internal static string MINIMAL_PX_FILE =
            "CHARSET=\"ANSI\";" +
            "\r\nAXIS-VERSION=\"2013\";" +
            "\r\nCODEPAGE=\"UTF-8\";" +
            "\r\nLANGUAGE=\"fi\";" +
            "\r\nLANGUAGES=\"fi\";" +
            "\r\nNEXT-UPDATE=\"20240201 08:00\";" +
            "\r\nTABLEID=\"table-id\";" +
            "\r\nSUBJECT-AREA=\"subject-area\";" +
            "\r\nCOPYRIGHT=YES;" +
            "\r\nDESCRIPTION=\"lorem ipsum\";" +
            "\r\nSTUB=\"Vuosi\";" +
            "\r\nHEADING=\"Tiedot\";" +
            "\r\nCONTVARIABLE=\"Tiedot\";" +
            "\r\nVALUES(\"Vuosi\")=\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\",\"2023\",\"2024\";" +
            "\r\nVALUES(\"Tiedot\")=\"foo-val-a\",\"foo-val-b\";" +
            "\r\nTIMEVAL(\"Vuosi\")=TLIST(A1),\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\",\"2023\",\"2024\";" +
            "\r\nCODES(\"Vuosi\")=\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\",\"2023\",\"2024\";" +
            "\r\nCODES(\"Tiedot\")=\"code-foo-val-a\",\"code-foo-val-b\";" +
            "\r\nVARIABLE-TYPE(\"Vuosi\")=\"Time\";" +
            "\r\nVARIABLE-TYPE(\"Tiedot\")=\"Contents\";" +
            "\r\nPRECISION(\"Tiedot\",\"foo-val-a\")=1;" +
            "\r\nPRECISION(\"Tiedot\",\"foo-val-b\")=1;" +
            "\r\nLAST-UPDATED(\"Tiedot\",\"foo-val-a\")=\"20231101 08:00\";" +
            "\r\nLAST-UPDATED(\"Tiedot\",\"foo-val-b\")=\"20231101 08:00\";" +
            "\r\nUNITS(\"Tiedot\",\"foo-val-a\")=\"kpl\";" +
            "\r\nUNITS(\"Tiedot\",\"foo-val-b\")=\"%\";" +
            "\r\nVARIABLECODE(\"Tiedot\")=\"code-tiedot\";" +
            "\r\nVARIABLECODE(\"Vuosi\")=\"code-vuosi\";" +
            "\r\nDATA=" +
            "\r\n0 1 " +
            "\r\n2 3 " +
            "\r\n4 5 " +
            "\r\n6 7 " +
            "\r\n8 9 " +
            "\r\n10 11 " +
            "\r\n12 13 " +
            "\r\n14 15 " +
            "\r\n16 17 " +
            "\r\n18 19; " +
            "\r\n";

        internal static readonly string HEADING_ONLY_PX_FILE = MINIMAL_PX_FILE
            .Replace("\r\nSTUB=\"Vuosi\";", string.Empty)
            .Replace("\r\nVALUES(\"Vuosi\")=\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\",\"2023\",\"2024\";", string.Empty)
            .Replace("\r\nTIMEVAL(\"Vuosi\")=TLIST(A1),\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\",\"2023\",\"2024\";", string.Empty)
            .Replace("\r\nCODES(\"Vuosi\")=\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\",\"2023\",\"2024\";", string.Empty)
            .Replace("\r\nVARIABLE-TYPE(\"Vuosi\")=\"Time\";", string.Empty)
            .Replace("\r\nVARIABLECODE(\"Vuosi\")=\"code-vuosi\";", string.Empty)
            .Replace(
                "\r\n0 1 \r\n2 3 \r\n4 5 \r\n6 7 \r\n8 9 \r\n10 11 \r\n12 13 \r\n14 15 \r\n16 17 \r\n18 19; ",
                "\r\n0 1; ");

        internal static readonly string STUB_ONLY_PX_FILE = MINIMAL_PX_FILE
            .Replace("\r\nHEADING=\"Tiedot\";", string.Empty)
            .Replace("\r\nCONTVARIABLE=\"Tiedot\";", string.Empty)
            .Replace("\r\nVALUES(\"Tiedot\")=\"foo-val-a\",\"foo-val-b\";", string.Empty)
            .Replace("\r\nCODES(\"Tiedot\")=\"code-foo-val-a\",\"code-foo-val-b\";", string.Empty)
            .Replace("\r\nVARIABLE-TYPE(\"Tiedot\")=\"Contents\";", string.Empty)
            .Replace("\r\nPRECISION(\"Tiedot\",\"foo-val-a\")=1;", string.Empty)
            .Replace("\r\nPRECISION(\"Tiedot\",\"foo-val-b\")=1;", string.Empty)
            .Replace("\r\nLAST-UPDATED(\"Tiedot\",\"foo-val-a\")=\"20231101 08:00\";", string.Empty)
            .Replace("\r\nLAST-UPDATED(\"Tiedot\",\"foo-val-b\")=\"20231101 08:00\";", string.Empty)
            .Replace("\r\nUNITS(\"Tiedot\",\"foo-val-a\")=\"kpl\";", string.Empty)
            .Replace("\r\nUNITS(\"Tiedot\",\"foo-val-b\")=\"%\";", string.Empty)
            .Replace("\r\nVARIABLECODE(\"Tiedot\")=\"code-tiedot\";", string.Empty)
            .Replace(
                "\r\n0 1 \r\n2 3 \r\n4 5 \r\n6 7 \r\n8 9 \r\n10 11 \r\n12 13 \r\n14 15 \r\n16 17 \r\n18 19; ",
                "\r\n0 \r\n1 \r\n2 \r\n3 \r\n4 \r\n5 \r\n6 \r\n7 \r\n8 \r\n9; ");

        internal static string PX_FILE_WITHOUT_DATA =
            "CHARSET=\"ANSI\";" +
            "\r\nAXIS-VERSION=\"2013\";" +
            "\r\nCODEPAGE=\"UTF-8\";" +
            "\r\nLANGUAGE=\"fi\";" +
            "\r\nLANGUAGES=\"fi\";" +
            "\r\nNEXT-UPDATE=\"20240201 08:00\";" +
            "\r\nTABLEID=\"table-id\";" +
            "\r\nSUBJECT-AREA=\"subject-area\";" +
            "\r\nCOPYRIGHT=YES;" +
            "\r\nDESCRIPTION=\"lorem ipsum\";" +
            "\r\nSTUB=\"Vuosi\";" +
            "\r\nHEADING=\"Tiedot\";" +
            "\r\nCONTVARIABLE=\"Tiedot\";" +
            "\r\nVALUES(\"Vuosi\")=\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\",\"2023\",\"2024\";" +
            "\r\nVALUES(\"Tiedot\")=\"foo-val-a\",\"foo-val-b\";" +
            "\r\nTIMEVAL(\"Vuosi\")=TLIST(A1),\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\",\"2023\",\"2024\";" +
            "\r\nCODES(\"Vuosi\")=\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\",\"2023\",\"2024\";" +
            "\r\nCODES(\"Tiedot\")=\"code-foo-val-a\",\"code-foo-val-b\";" +
            "\r\nVARIABLE-TYPE(\"Vuosi\")=\"Time\";" +
            "\r\nVARIABLE-TYPE(\"Tiedot\")=\"Contents\";" +
            "\r\nPRECISION(\"Tiedot\",\"foo-val-a\")=1;" +
            "\r\nPRECISION(\"Tiedot\",\"foo-val-b\")=1;" +
            "\r\nLAST-UPDATED(\"Tiedot\",\"foo-val-a\")=\"20231101 08:00\";" +
            "\r\nLAST-UPDATED(\"Tiedot\",\"foo-val-b\")=\"20231101 08:00\";" +
            "\r\nUNITS(\"Tiedot\",\"foo-val-a\")=\"kpl\";" +
            "\r\nUNITS(\"Tiedot\",\"foo-val-b\")=\"%\";" +
            "\r\nVARIABLECODE(\"Tiedot\")=\"code-tiedot\";" +
            "\r\nVARIABLECODE(\"Vuosi\")=\"code-vuosi\";";

        internal static string MINIMAL_SINGLE_LANGUAGE_PX_FILE =
            "CHARSET=\"ANSI\";" +
            "\r\nAXIS-VERSION=\"2013\";" +
            "\r\nCODEPAGE=\"UTF-8\";" +
            "\r\nLANGUAGE=\"en\";" +
            "\r\nLANGUAGES=\"en\";" +
            "\r\nNEXT-UPDATE=\"20240201 08:00\";" +
            "\r\nTABLEID=\"table-id\";" +
            "\r\nSUBJECT-AREA=\"subject-area\";" +
            "\r\nCOPYRIGHT=YES;" +
            "\r\nDESCRIPTION=\"lorem ipsum\";" +
            "\r\nSTUB=\"Vuosi\";" +
            "\r\nHEADING=\"Tiedot\";" +
            "\r\nCONTVARIABLE=\"Tiedot\";" +
            "\r\nVALUES(\"Vuosi\")=\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\",\"2023\",\"2024\";" +
            "\r\nVALUES(\"Tiedot\")=\"foo-val-a\",\"foo-val-b\";" +
            "\r\nTIMEVAL(\"Vuosi\")=TLIST(A1),\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\",\"2023\",\"2024\";" +
            "\r\nCODES(\"Vuosi\")=\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\",\"2023\",\"2024\";" +
            "\r\nCODES(\"Tiedot\")=\"code-foo-val-a\",\"code-foo-val-b\";" +
            "\r\nVARIABLE-TYPE(\"Vuosi\")=\"Time\";" +
            "\r\nVARIABLE-TYPE(\"Tiedot\")=\"Contents\";" +
            "\r\nPRECISION(\"Tiedot\",\"foo-val-a\")=1;" +
            "\r\nPRECISION(\"Tiedot\",\"foo-val-b\")=1;" +
            "\r\nLAST-UPDATED(\"Tiedot\",\"foo-val-a\")=\"20231101 08:00\";" +
            "\r\nLAST-UPDATED(\"Tiedot\",\"foo-val-b\")=\"20231101 08:00\";" +
            "\r\nUNITS(\"Tiedot\",\"foo-val-a\")=\"kpl\";" +
            "\r\nUNITS(\"Tiedot\",\"foo-val-b\")=\"%\";" +
            "\r\nVARIABLECODE(\"Tiedot\")=\"code-tiedot\";" +
            "\r\nVARIABLECODE(\"Vuosi\")=\"code-vuosi\";" +
            "\r\nDATA=" +
            "\r\n0 1 " +
            "\r\n2 3 " +
            "\r\n4 5 " +
            "\r\n6 7 " +
            "\r\n8 9 " +
            "\r\n10 11 " +
            "\r\n12 13 " +
            "\r\n14 15 " +
            "\r\n16 17 " +
            "\r\n18 19; " +
            "\r\n";

        internal static string INVALID_PX_FILE =
            "CHARSET=ANSI;" +
            "\r\nAXIS-VERSION=2013;" +
            "\r\nCODEPAGE=\"UTF-8\";" +
            "\nLANGUAGE=\"fi\";" +
            "\r\nLANGUAGES=\"fi\",\"en\";" +
            "\r\nNEXT-UPDATE=\"20240201 08:00\"" +
            "\r\nTABLEID=\"table-id\";" +
            "\r\nCOPYRIGHT=YES;" +
            "\r\nDESCRIPTION=\"lorem ipsum\";" +
            "\r\nSTUB=\"Vuosi\";" +
            "\r\nHEADING=\"Tiedot\";" +
            "\r\nCONTVARIABLE=\"Tiedot\";" +
            "\r\nVALUES(\"Vuosi\")=\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\",\"2023\",\"2024\";" +
            "\r\nVALUES(\"Tiedot\")=\"foo-val-a\",\"foo-val-b\";" +
            "\r\nTIMEVAL(\"Vuosi\")=TLIST(A1),\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\",\"2023\",\"2024\";" +
            "\r\nCODES(\"Vuosi\")=\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\",\"2023\",\"2024\";" +
            "\r\nCODES(\"Tiedot\")=\"code-foo-val-a\",\"code-foo-val-b\";" +
            "\r\nVARIABLE-TYPE(\"Vuosi\")=\"Time\";" +
            "\r\nVARIABLE-TYPE(\"Tiedot\")=\"Contents\";" +
            "\r\nPRECISION(\"Tiedot\",\"foo-val-a\")=1;" +
            "\r\nLAST-UPDATED(\"Tiedot\",\"foo-val-a\")=\"20231101 08:00\";" +
            "\r\nUNITS(\"Tiedot\",\"foo-val-a\")=\"kpl\";" +
            "\r\nVARIABLECODE(\"Tiedot\")=\"code-tiedot\";" +
            "\r\nVARIABLECODE(\"Vuosi\")=\"code-vuosi\";" +
            "\r\nDATA=" +
            "\r\n0 " +
            "\r\n2 3 " +
            "\r\n4 5" +
            "\r\n6 7 " +
            "\r\n10 11 " +
            "\r\n12 13 " +
            "\r\n14 15 " +
            "\r\n16 17 " +
            "\r\n18 19;";
    }
}
