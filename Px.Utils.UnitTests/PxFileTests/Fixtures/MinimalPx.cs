namespace Px.Utils.UnitTests.PxFileTests.Fixtures
{
    internal static class MinimalPx
    {
        internal static string MINIMAL1_UTF8_N => @"
            CHARSET=""ANSI"";
            AXIS-VERSION=""2013"";
            CODEPAGE=""utf-8"";
            LANGUAGES=""aa"",""åå"",""öö"";
            NEXT-UPDATE=""20240131 08:00"";
            SUBJECT-AREA=""test"";
            SUBJECT-AREA[åå]=""test"";
            COPYRIGHT=YES;
            DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;
        ";
    }
}
