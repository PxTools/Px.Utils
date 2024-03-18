namespace PxUtils.UnitTests.SyntaxValidationTests.Fixtures
{
    internal static class SyntaxValidationFixtures
    {
        internal static string MINIMAL_UTF8_N =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[fi]=\"test\";\nCOPYRIGHT=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string MINIMAL_UTF8_N_WITH_MULTIPLE_ENTRIES_IN_SINGLE_LINE =>
            "CHARSET=\"ANSI\";AXIS-VERSION=\"2013\";CODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[fi]=\"test\";\nCOPYRIGHT=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_SPECIFIERS =>
            "CHARSET=\"ANSI\";\n" +
            "AXIS-VERSION=\"2013\";\n" +
            "CODEPAGE=\"utf-8\";\n" +
            "LANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\n" +
            "SUBJECT-AREA=\"test\";\n" +
            "SUBJECT-AREA[en]=\"test\";\n" +
            "COPYRIGHT=YES;\n" +
            "FOO[fi](\"first_specifier\", \"second_specifier\")=YES;\n" +
            "BAR[fi](\"first_specifier\")=NO;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_ENTRY_WITH_MULTIPLE_LANGUAGE_PARAMETERS =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[en]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[fi][en](\"first_specifier\", \"second_specifier\")=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_SPECIFIERS_WITH_MULTIPLE_SPECIFIER_PARAMETERS =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[en]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[fi](\"first_specifier\", \"second_specifier\")=YES;\nBAR[fi](\"first_specifier\")=NO;\n" +
            "FOO[fi](\"first_specifier\")(\"second_specifier\")=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_ENTRIES_IN_WRONG_ORDER =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[en]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[fi](\"first_specifier\", \"second_specifier\")=YES;\nBAR[fi](\"first_specifier\")=NO;\n" +
            "FOO(\"first_specifier\")[fi]=YES;\n[en]BAR(\"first_specifier\")=NO;" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_A_MISSING_KEYWORD =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[en]=\"test\";\nCOPYRIGHT=YES;\n" +
            "[fi](\"first_specifier\", \"second_specifier\")=YES;\nBAR[fi](\"first_specifier\")=NO;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_INVALID_SPECIFIERS =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[en]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[fi](\"first_specifier\", \"second_specifier\", \"third_specifier\")=YES;\n" +
            "BAR[fi](first_specifier\")=NO;\n" +
            "BAR[fi](\"first_specifier\" \"second_specifier\")=NO;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_ILLEGAL_SYMBOLS_IN_PARAMS =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[en]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[\"fi\"](\"first_specifier\", \"second_specifier\")=YES;\n" +
            "BAR[\"[first_specifier]\", \"second_specifier\"]=NO;\n" +
            "BAZ[fi, en]=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_CORRECTLY_FORMATTED_LIST_AND_MULTILINE_STRING =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[en]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[fi](\"first_specifier\", \"second_specifier\")=\"dis parturient montes nascetur ridiculus mus\"\n" +
            "\"mauris vitae ultricies leo integer malesuada nunc vel risus commodo viverra maecenas accumsan lacus vel\"\n" +
            "\"facilisis volutpat est velit egestas dui id ornare\";\n" +
            "BAR[fi](\"first_specifier\")=\"dis\", \"parturient\", \"montes\", \"nascetur\", \"ridiculus\", \"mus\",\n" +
            "\"mauris\", \"vitae\", \"ultricies\", \"leo\", \"integer\", \"malesuada\", \"nunc\", \"vel\", \"commodo\", \"viverra\",\n" +
            "\"maecenas\", \"accumsan\", \"lacus\", \"vel\", \"facilisis\", \"volutpat\", \"est\", \"velit\", \"egestas\", \"dui\",\n" +
            "\"id\", \"ornare\";\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_BAD_VALUES =>
            "CHARSET=ANSI;\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[en]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[fi](\"first_specifier\", \"second_specifier\")=\"dis parturient montes nascetur ridiculus mus\"\n" +
            " \"mauris vitae ultricies leo integer malesuada nunc vel risus commodo viverra maecenas accumsan lacus vel\"\n" +
            "\"facilisis volutpat est velit egestas dui id ornare\";\n" +
            "BAR[fi](\"first_specifier\")=\"dis\", \"parturient\", \"montes\", \"nascetur\", \"ridiculus\", \"mus\"\n" +
            "\"mauris\", \"vitae\", \"ultricies\", \"leo\", \"integer\", \"malesuada\", \"nunc\", \"vel\", \"commodo\", \"viverra\",\n" +
            "\"maecenas\", \"accumsan\", \"lacus\", \"vel\", \"facilisis\", \"volutpat\", \"est\", \"velit\", \"egestas\", \"dui\",\n" +
            "\"id\", \"ornare\";\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_EXCESS_WHITESPACE_IN_LIST =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[en]=\"test\";\nCOPYRIGHT=YES;\n" +
            "BAR[fi](\"first_specifier\")=\"dis\", \"parturient\", \"montes\", \"nascetur\", \"ridiculus\", \"mus\",\n" +
            "\"mauris\", \"vitae\", \"ultricies\", \"leo\", \"integer\", \"malesuada\",  \"nunc\", \"vel\", \"commodo\", \"viverra\",\n" +
            "\"maecenas\", \"accumsan\", \"lacus\", \"vel\", \"facilisis\", \"volutpat\", \"est\", \"velit\", \"egestas\", \"dui\",\n" +
            "\"id\", \"ornare\";\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_SHORT_MULTILINE_VALUES =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[en]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[fi](\"first_specifier\", \"second_specifier\")=\"foobar foobar\"\n\"foobar foober\";\nBAR[fi](\"first_specifier\")=\"foo\", \"bar\",\n\"baz\";\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_INVALID_KEYWORDS =>
            "CHARSET=\"ANSI\";\n" +
            "AXIS/VERSION=\"2013\";\n" +
            "CODEPAGE=\"utf-8\";\n" +
            "LANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\n" +
            "SUBJECT:AREA=\"test\";\n" +
            "SUBJECT-AREA[en]=\"test\";\nCOPYRIGHT=YES;\n" +
            "1FOO[fi](\"first_specifier\", \"second_specifier\")=YES;\n" +
            "BAR[fi](\"first_specifier\")=NO;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_VALID_LANGUAGES =>
            "CHARSET=\"ANSI\";\n" +
            "AXIS-VERSION=\"2013\";\n" +
            "CODEPAGE=\"utf-8\";\n" +
            "LANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\n" +
            "SUBJECT-AREA=\"test\";\n" +
            "SUBJECT-AREA[gle]=\"test\";\n" +
            "COPYRIGHT=YES;\n" +
            "FOO[fi-FI](\"first_specifier\", \"second_specifier\")=YES;\n" +
            "BAR[fi](\"first_specifier\")=NO;\n" +
            "BAZ[fin]=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_INVALID_LANGUAGES =>
            "CHARSET=\"ANSI\";\n" +
            "AXIS-VERSION=\"2013\";\n" +
            "CODEPAGE=\"utf-8\";\n" +
            "LANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\n" +
            "SUBJECT-AREA=\"test\";\n" +
            "SUBJECT-AREA[fi en]=\"test\";\n" +
            "COPYRIGHT=YES;\n" +
            "FOO[fi](\"first_specifier\", \"second_specifier\")=YES;\n" +
            "BAR[fi](\"first_specifier\")=NO;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIERS =>
            "CHARSET=\"ANSI\";\n" +
            "AXIS-VERSION=\"2013\";\n" +
            "CODEPAGE=\"utf-8\";\n" +
            "LANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\n" +
            "SUBJECT-AREA=\"test\";\n" +
            "SUBJECT-AREA[en]=\"test\";\n" +
            "COPYRIGHT=YES;\n" +
            "FOO[fi](\"first,specifier\", \"second_specifier\")=YES;\n" +
            "BAR[fi](\"first\"specifier\")=NO;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_VALUELESS_ENTRY =>
            "CHARSET=\"ANSI\";\n" +
            "AXIS-VERSION=\"2013\";\n" +
            "CODEPAGE=\"utf-8\";\n" +
            "LANGUAGES(\"aa\",\"åå\");\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\n" +
            "SUBJECT-AREA=\"test\";\n" +
            "SUBJECT-AREA[en]=\"test\";\n" +
            "COPYRIGHT=YES;\n" +
            "FOO[fi](\"first_specifier\", \"second_specifier\")=YES;\n" +
            "BAR[fi](\"first_specifier\")=NO;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_INCOMPLIANT_LANGUAGES =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA[engl]=\"test\";\nSUBJECT-AREA[finnish]=\"test\";\nCOPYRIGHT=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_LONG_KEYWORD =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[fi]=\"test\";\nCOPYRIGHT=YES;\n" +
            "THISISALONGKEYWORDWHICHISNOTRECOMMENDED[fi]=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_UNRECOMMENDED_KEYWORD_NAMING =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[fi]=\"test\";\nCOPYRIGHT=YES;\n" +
            "PascalCase[fi]=YES;\nSCREAMING_SNAKE[en]=NO;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";
    }
}
