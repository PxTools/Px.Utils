using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Px.Utils.UnitTests.PxFileTests.Fixtures
{
    internal static class SyntaxValidationFixtures
    {
        internal static string UTF8_N_WITH_SPECIFIERS =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[åå]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[lang](\"first_specifier\", \"second_specifier\")=YES;\nBAR[lang](\"first_specifier\")=NO;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_ENTRY_WITH_MULTIPLE_LANGUAGE_PARAMETERS =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[åå]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[lang1][lang2](\"first_specifier\", \"second_specifier\")=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_SPECIFIERS_WITH_MULTIPLE_SPECIFIER_PARAMETERS =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[åå]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[lang](\"first_specifier\", \"second_specifier\")=YES;\nBAR[lang](\"first_specifier\")=NO;\n" +
            "FOO[lang1](\"first_specifier\")(\"second_specifier\")=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_ENTRIES_IN_WRONG_ORDER =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[åå]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[lang](\"first_specifier\", \"second_specifier\")=YES;\nBAR[lang](\"first_specifier\")=NO;\n" +
            "FOO(\"first_specifier\")[lang1]=YES;\n[lang2]BAR(\"first_specifier\")=NO;" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_A_MISSING_KEYWORD =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[åå]=\"test\";\nCOPYRIGHT=YES;\n" +
            "[lang](\"first_specifier\", \"second_specifier\")=YES;\nBAR[lang](\"first_specifier\")=NO;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_INVALID_SPECIFIERS =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[åå]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[lang](\"first_specifier\", \"second_specifier\", \"third_specifier\")=YES;\n" +
            "BAR[lang](first_specifier\")=NO;\n" +
            "BAR[lang](\"first_specifier\" \"second_specifier\")=NO;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_ILLEGAL_SYMBOLS_IN_PARAMS =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[åå]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[\"lang\"](\"first_specifier\", \"second_specifier\")=YES;\n" +
            "BAR[\"[first_specifier]\",  \"second_specifier\"]=NO;\n" +
            "BAZ[lang, lang2]=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_CORRECTLY_FORMATTED_LIST_AND_MULTILINE_STRING =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[åå]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[lang](\"first_specifier\", \"second_specifier\")=\"dis parturient montes nascetur ridiculus mus\"\n" +
            "\"mauris vitae ultricies leo integer malesuada nunc vel risus commodo viverra maecenas accumsan lacus vel\"\n" +
            "\"facilisis volutpat est velit egestas dui id ornare\";\n" +
            "BAR[lang](\"first_specifier\")=\"dis\", \"parturient\", \"montes\", \"nascetur\", \"ridiculus\", \"mus\",\n" +
            "\"mauris\", \"vitae\", \"ultricies\", \"leo\", \"integer\", \"malesuada\", \"nunc\", \"vel\", \"commodo\", \"viverra\",\n" +
            "\"maecenas\", \"accumsan\", \"lacus\", \"vel\", \"facilisis\", \"volutpat\", \"est\", \"velit\", \"egestas\", \"dui\",\n" +
            "\"id\", \"ornare\";\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_BAD_VALUES =>
            "CHARSET=ANSI;\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[åå]=\"test\";\nCOPYRIGHT=YES;\n" +
            "FOO[lang](\"first_specifier\", \"second_specifier\")=\"dis parturient montes nascetur ridiculus mus\"\n" +
            " \"mauris vitae ultricies leo integer malesuada nunc vel risus commodo viverra maecenas accumsan lacus vel\"\n" +
            "\"facilisis volutpat est velit egestas dui id ornare\";\n" +
            "BAR[lang](\"first_specifier\")=\"dis\", \"parturient\", \"montes\", \"nascetur\", \"ridiculus\", \"mus\" \n" +
            "\"mauris\", \"vitae\", \"ultricies\", \"leo\", \"integer\", \"malesuada\", \"nunc\", \"vel\", \"commodo\", \"viverra\",\n" +
            "\"maecenas\", \"accumsan\", \"lacus\", \"vel\", \"facilisis\", \"volutpat\", \"est\", \"velit\", \"egestas\", \"dui\",\n" +
            "\"id\", \"ornare\";\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";
    }
}
