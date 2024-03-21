using PxUtils.PxFile;
using PxUtils.Validation.SyntaxValidation;

namespace PxUtils.UnitTests.SyntaxValidationTests.Fixtures
{
    internal static class SyntaxValidationFixtures
    {
        internal static string MINIMAL_UTF8_N =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[fi]=\"test\";\nCOPYRIGHT=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UNKNOWN_ENCODING =>
            "CHARSET=\"foo\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"bar\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[fi]=\"test\";\nCOPYRIGHT=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static List<ValidationEntry> MULTIPLE_ENTRIES_IN_SINGLE_LINE => [
            new(0, 0, "foo", "CHARSET=\"ANSI\";", 0),
            new(1, 0, "foo", "AXIS-VERSION=\"2013\";", 1),
            new(2, 0, "foo", "CODEPAGE=\"utf-8\";", 2),
        ];

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

        internal static List<ValidationKeyValuePair> KEYVALUEPAIR_WITH_MULTIPLE_LANGUAGE_PARAMETERS => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO[fi][en](\"first_specifier\", \"second_specifier\")", "YES\n"))
                ];

        internal static List<ValidationKeyValuePair> KEYVALUEPAIR_WITH_MULTIPLE_SPECIFIER_PARAMETER_SECTIONS => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO[fi](\"first_specifier\")(\"second_specifier\")", "YES\n"))
                ];

        internal static List<ValidationKeyValuePair> KEYVALUEPAIRS_IN_WRONG_ORDER_AND_MISSING_KEYWORD => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO(\"first_specifier\")[fi]", "YES\n")),
            new(1, 0, "foo", new KeyValuePair<string, string>("[en]BAR(\"first_specifier\")", "NO\n")),
            new(2, 0, "foo", new KeyValuePair<string, string>("[fi](\"first_specifier\")", "baz\n"))
                ];

        internal static List<ValidationKeyValuePair> KEYVALUEPAIRS_WITH_INVALID_SPECIFIERS => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO[fi](\"first_specifier\", \"second_specifier\", \"third_specifier\")", "YES\n")),
            new(1, 0, "foo", new KeyValuePair<string, string>("BAR[fi](first_specifier\")", "NO\n")),
            new(2, 0, "foo", new KeyValuePair<string, string>("BAZ[fi](\"first_specifier\" \"second_specifier\")", "NO\n"))
                ];

        internal static List<ValidationKeyValuePair> KEYVALUEPAIRS_WITH_ILLEGAL_SYMBOLS_IN_LANGUAGE_SECTIONS => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO[\"fi\"](\"first_specifier\", \"second_specifier\")", "YES\n")),
            new(1, 0, "foo", new KeyValuePair<string, string>("BAR[\"[first_specifier]\", \"second_specifier\"]", "NO\n")),
            new(2, 0, "foo", new KeyValuePair<string, string>("BAZ[fi, en]", "NO\n"))
            ];

        internal static List<ValidationKeyValuePair> KEYVALUEPAIR_WITH_ILLEGAL_SYMBOLS_IN_SPECIFIER_SECTIONS => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO([\"first_specifier\"], [\"second_specifier\"])", "YES\n")),
            ];

        private const string multilineStringValue = "\"dis parturient montes nascetur ridiculus mus\"\n" +
            "\"mauris vitae ultricies leo integer malesuada nunc vel risus commodo viverra maecenas accumsan lacus vel\"\n" +
            "\"facilisis volutpat est velit egestas dui id ornare\"";
        private const string multilineListValue = "\"dis\", \"parturient\", \"montes\", \"nascetur\", \"ridiculus\", \"mus\",\n" +
            "\"mauris\", \"vitae\", \"ultricies\", \"leo\", \"integer\", \"malesuada\", \"nunc\", \"vel\", \"commodo\", \"viverra\",\n" +
            "\"maecenas\", \"accumsan\", \"lacus\", \"vel\", \"facilisis\", \"volutpat\", \"est\", \"velit\", \"egestas\", \"dui\",\n" +
            "\"id\", \"ornare\"";
        internal static List<ValidationKeyValuePair> KEYVALUEPAIRS_WITH_CORRECTLY_FORMATTED_LIST_AND_MULTILINE_STRING => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO", multilineStringValue)),
            new(1, 0, "foo", new KeyValuePair<string, string>("BAR", multilineListValue)),
            ];

        private const string badMultilineStringValue = "\"dis parturient montes nascetur ridiculus mus\"\n" +
            " \"mauris vitae ultricies leo integer malesuada nunc vel risus commodo viverra maecenas accumsan lacus vel\"\n" +
            "\"facilisis volutpat est velit egestas dui id ornare\";\n";
        private const string badMultilineListValue = "\"dis\", \"parturient\", \"montes\", \"nascetur\", \"ridiculus\", \"mus\"\n" +
            "\"mauris\", \"vitae\", \"ultricies\", \"leo\", \"integer\", \"malesuada\", \"nunc\", \"vel\", \"commodo\", \"viverra\",\n" +
            "\"maecenas\", \"accumsan\", \"lacus\", \"vel\", \"facilisis\", \"volutpat\", \"est\", \"velit\", \"egestas\", \"dui\",\n" +
            "\"id\", \"ornare\";\n";
        internal static List<ValidationKeyValuePair> KEYVALUEPAIRS_WITH_BAD_VALUES => [
            new(0, 0, "foo", new KeyValuePair<string, string>("CHARSET", "ANSI")),
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO", "1 2 3")),
            new(1, 0, "foo", new KeyValuePair<string, string>("FOO", badMultilineStringValue)),
            new(2, 0, "foo", new KeyValuePair<string, string>("BAR", badMultilineListValue)),
        ];

        private const string excessWhitespaceStringValue = "\"dis\", \"parturient\", \"montes\", \"nascetur\", \"ridiculus\", \"mus\",\n" +
            "\"mauris\", \"vitae\", \"ultricies\", \"leo\", \"integer\", \"malesuada\",  \"nunc\", \"vel\", \"commodo\", \"viverra\",\n" +
            "\"maecenas\", \"accumsan\", \"lacus\", \"vel\", \"facilisis\", \"volutpat\", \"est\", \"velit\", \"egestas\", \"dui\",\n" +
            "\"id\", \"ornare\"";
        internal static List<ValidationKeyValuePair> KEYVALUEPAIR_WITH_EXCESS_LIST_VALUE_WHITESPACE => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO", excessWhitespaceStringValue)),
        ];

        private const string keyWithExcessWhitespace = "KEYWORD (fi) (\"first_specifier\", \"second_specifier\")";
        internal static List<ValidationKeyValuePair> KEYVALUEPAIR_WITH_EXCESS_KEY_WHITESPACE => [
            new(0, 0, "foo", new KeyValuePair<string, string>(keyWithExcessWhitespace, "foo")),
        ];

        private const string shortStringValueWithNewLine = "\"foobar foobar\"\n\"foobar foober\"";
        private const string shortListValueWithNewLine = "\"foo\", \"bar\",\n\"baz\"";
        internal static List<ValidationKeyValuePair> KEYVALUEPAIRS_WITH_SHORT_MULTILINE_VALUES => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO", shortStringValueWithNewLine)),
            new(1, 0, "foo", new KeyValuePair<string, string>("BAR", shortListValueWithNewLine))
        ];

        private readonly static ValidationStructKey numberInKeyword = new("1FOO");
        private readonly static ValidationStructKey slashInKeyword = new("B/AR");
        private readonly static ValidationStructKey colonInKeyword = new("B:AZ");
        internal static List<ValidationStruct> STRUCTS_WITH_INVALID_KEYWORDS => [
            new(0, 0, "foo", numberInKeyword, "foo"),
            new(1, 0, "foo", slashInKeyword, "bar"),
            new(2, 0, "foo", colonInKeyword, "baz")
        ];

        private readonly static ValidationStructKey fi = new("FOO", "fi");
        private readonly static ValidationStructKey fin = new("BAR", "fin");
        private readonly static ValidationStructKey fiFi = new("BAZ", "fi-FI");
        internal static List<ValidationStruct> STRUCTS_WITH_VALID_LANGUAGES => [
            new(0, 0, "foo", fi, "foo"),
            new(1, 0, "foo", fin, "bar"),
            new(2, 0, "foo", fiFi, "baz")
        ];

        private readonly static ValidationStructKey fien = new("FOO", "fi en");
        internal static List<ValidationStruct> STRUCTS_WITH_INVALID_LANGUAGES => [
            new(0, 0, "foo", fien, "foo"),
        ];

        private readonly static ValidationStructKey illegalSpecifier = new("FOO", "fi", "first\"specifier");
        internal static List<ValidationStruct> STRUCT_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIERS => [
            new(0, 0, "foo", illegalSpecifier, "foo"),
        ];

        internal static List<ValidationEntry> ENTRY_WITHOUT_VALUE => [
           new(0, 0, "LANGUAGES", "", 0)
        ];

        private readonly static ValidationStructKey finnish = new("FOO", "finnish");
        private readonly static ValidationStructKey engl = new("BAR", "engl");
        internal static List<ValidationStruct> STRUCTS_WITH_INCOMPLIANT_LANGUAGES => [
            new(0, 0, "foo", finnish, "foo"),
            new(1, 0, "foo", engl, "foo"),
        ];

        private readonly static ValidationStructKey longKeyword = new("THISISALONGKEYWORDWHICHISNOTRECOMMENDED");
        internal static List<ValidationStruct> STRUCT_WITH_LONG_KEYWORD => [
            new(0, 0, "foo", longKeyword, "foo"),
        ];

        private readonly static ValidationStructKey pascalCaseKeyword = new("PascalCase");
        private readonly static ValidationStructKey screamingSnakeCaseKeyword = new("SCREAMING_SNAKE");
        internal static List<ValidationStruct> STRUCTS_WITH_UNRECOMMENDED_KEYWORD_NAMING => [
            new(0, 0, "foo", pascalCaseKeyword, "foo"),
            new(1, 0, "foo", screamingSnakeCaseKeyword, "foo")
        ];
    }
}
