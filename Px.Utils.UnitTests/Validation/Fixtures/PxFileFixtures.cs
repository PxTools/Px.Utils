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
            "\r\nVARIABLE-TYPE(\"Tiedot\")=\"Content\";" +
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
            "\r\nVARIABLE-TYPE(\"Tiedot\")=\"Content\";" +
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
            "\r\nVARIABLE-TYPE(\"Tiedot\")=\"Content\";" +
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
            "\r\nVARIABLE-TYPE(\"Tiedot\")=\"Content\";" +
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
