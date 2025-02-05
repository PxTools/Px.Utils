namespace PxFileTests.Fixtures
{
    internal static class DataReaderFixtures
    {
        internal static string MINIMAL_UTF8_20DATAVALUES =
            """
            CHARSET=""Unicode"";
            AXIS-VERSION=""2013"";
            CODEPAGE=""utf-8"";
            LANGUAGES=""aa"",""åå"",""öö"";
            NEXT-UPDATE=""20240131 08:00"";
            SUBJECT-AREA=""test"";
            SUBJECT-AREA[åå]=""test"";
            COPYRIGHT=YES;
            DATA=
            0 1 2 3 4 5 6 7 8 9 
            10 11 12 13 14 15 16 17 18 19;
            """;

        internal static string MINIMAL_UTF8_20DATAVALUES_WITH_MISSING =
            """
            CHARSET=""Unicode"";
            AXIS-VERSION=""2013"";
            CODEPAGE=""utf-8"";
            LANGUAGES=""aa"",""åå"",""öö"";
            NEXT-UPDATE=""20240131 08:00"";
            SUBJECT-AREA=""test"";
            SUBJECT-AREA[åå]=""test"";
            COPYRIGHT=YES;
            DATA=
            "." 1 "." 3 "." 5 "." 7 "." 9 
            "..." 11 "..." 13 "..." 15 "..." 17 "..." 19;
            """;

        internal static string MINIMAL_UTF8_20DATAVALUES_WITH_UNENCLOSED_MISSING =
            """
            CHARSET=""Unicode"";
            AXIS-VERSION=""2013"";
            CODEPAGE=""utf-8"";
            LANGUAGES=""aa"",""åå"",""öö"";
            NEXT-UPDATE=""20240131 08:00"";
            SUBJECT-AREA=""test"";
            SUBJECT-AREA[åå]=""test"";
            COPYRIGHT=YES;
            DATA=
            "." 1 "." 3 "." 5 "." 7 "." 9 
            ... 11 ... 13 ... 15 ... 17 ... 19;
            """;

        internal static string MINIMAL_UTF8_20DECIMALVALUES =
            """
            CHARSET=""Unicode"";
            AXIS-VERSION=""2013"";
            CODEPAGE=""utf-8"";
            LANGUAGES=""aa"",""åå"",""öö"";
            NEXT-UPDATE=""20240131 08:00"";
            SUBJECT-AREA=""test"";
            SUBJECT-AREA[åå]=""test"";
            COPYRIGHT=YES;
            DATA=
            0.0 0.01 0.02 0.03 0.04 0.05 0.06 0.07 0.08 0.09 
            0.1 0.11 0.12 0.13 0.14 0.15 0.16 0.17 0.18 0.19; 
            """; // observe the trailing space

        internal static string MINIMAL_UTF8_20ROWS =
            """
            CHARSET=""Unicode"";
            AXIS-VERSION=""2013"";
            CODEPAGE=""utf-8"";
            LANGUAGES=""aa"",""åå"",""öö"";
            NEXT-UPDATE=""20240131 08:00"";
            SUBJECT-AREA=""test"";
            SUBJECT-AREA[åå]=""test"";
            COPYRIGHT=YES;
            DATA=
            0.0 0.01 0.02 0.03 0.04 0.05 0.06 0.07 0.08 0.09 
            0.1 0.11 0.12 0.13 0.14 0.15 0.16 0.17 0.18 0.19; 
            """; // observe the trailing space
        }
}
