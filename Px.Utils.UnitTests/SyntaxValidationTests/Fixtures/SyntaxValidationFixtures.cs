using PxUtils.Validation.SyntaxValidation;
using PxUtils.Validation;

namespace PxUtils.UnitTests.SyntaxValidationTests.Fixtures
{
    internal static class SyntaxValidationFixtures
    {
        internal static string MINIMAL_UTF8_N =>
            "CHARSET=\"ANSI\";\nAXIS-VERSION=\"2013\";\nCODEPAGE=\"utf-8\";\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\nSUBJECT-AREA=\"test\";\nSUBJECT-AREA[fi]=\"test\";\nCOPYRIGHT=YES;\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static List<ValidationEntry> MULTIPLE_ENTRIES_IN_SINGLE_LINE => [
            new("foo", "CHARSET=\"ANSI\";", 0, 0, []),
            new("foo", "AXIS-VERSION=\"2013\";", 1, 0, []),
            new("foo", "CODEPAGE=\"utf-8\";", 2, 0, []),
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

        internal static string UTF8_N_WITH_FEEDBACKS =>
            "CHARSET=\"ANSI\";\n" +
            "AXIS-VERSION=\"2013\";\n" +
            "CODEPAGE=\"utf-8\";\n" +
            "LANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";\n" +
            "SUBJECT-AREA=\"test\";\n" +
            "SUBJECT-AREA[en]=\"test\";\n" +
            "COPYRIGHT=YES;\n" +
            "FOO[fi](\"spec1\",  \"spec2\")=YES;\n" + // Excess whitespace
            "BAR[fi]=\"dis\", \"parturient\", \"montes\", \"nascetur\", \"ridiculus\", \"mus\",\n" +
            "\"mauris\", \"vitae\", \"ultricies\", \"leo\", \"integer\", \"malesuada\", \"nunc\", \"vel\", \"commodo\", \"viverra\",\n" +
            "\"maecenas\", \"accumsan\", \"lacus\", \"vel\",  \"facilisis\", \"volutpat\", \"est\", \"velit\", \"egestas\", \"dui\",\n" + // Excess whitespace
            "\"id\", \"ornare\";\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_TIMEVALS =>
            "CHARSET=\"ANSI\";" +
            "\nAXIS-VERSION=\"2013\";" +
            "\nCODEPAGE=\"utf-8\";" +
            "\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";" +
            "\nSUBJECT-AREA=\"test\";" +
            "\nSUBJECT-AREA[fi]=\"test\";" +
            "\nCOPYRIGHT=YES;\n" +
            "TIMEVAL(\"foo\")=TLIST(A1),\"2000\",\"2001\",\"2003\";\n" +
            "TIMEVAL(\"bar\")=TLIST(H1, \"20001-20202\");\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static string UTF8_N_WITH_BAD_TIMEVALS =>
            "CHARSET=\"ANSI\";" +
            "\nAXIS-VERSION=\"2013\";" +
            "\nCODEPAGE=\"utf-8\";" +
            "\nLANGUAGES=\"aa\",\"åå\",\"öö\";\n" +
            "NEXT-UPDATE=\"20240131 08:00\";" +
            "\nSUBJECT-AREA=\"test\";" +
            "\nSUBJECT-AREA[fi]=\"test\";" +
            "\nCOPYRIGHT=YES;\n" +
            "TIMEVAL(\"foo\")=TLIST(A1),2000,2001,2003;\n" +
            "TIMEVAL(\"bar\")=TLIST(H1 20001-20202);\n" +
            "DATA=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20;";

        internal static List<ValidationKeyValuePair> KEYVALUEPAIR_WITH_MULTIPLE_LANGUAGE_PARAMETERS => [
            new("foo", new KeyValuePair<string, string>("FOO[fi][en](\"first_specifier\", \"second_specifier\")", "YES\n"), 0, [], 0)
                ];

        internal static List<ValidationKeyValuePair> KEYVALUEPAIR_WITH_MULTIPLE_SPECIFIER_PARAMETER_SECTIONS => [
            new("foo", new KeyValuePair<string, string>("FOO[fi](\"first_specifier\")(\"second_specifier\")", "YES\n"), 0, [], 0)
                ];

        internal static List<ValidationKeyValuePair> KEYVALUEPAIRS_IN_WRONG_ORDER_AND_MISSING_KEYWORD => [
            new("foo", new KeyValuePair<string, string>("FOO(\"first_specifier\")[fi]", "YES\n"), 0, [], 0),
            new("foo", new KeyValuePair<string, string>("[en]BAR(\"first_specifier\")", "NO\n"), 0, [], 0),
            new("foo", new KeyValuePair<string, string>("[fi](\"first_specifier\")", "baz\n"), 0, [], 0)
                ];

        internal static List<ValidationKeyValuePair> KEYVALUEPAIRS_WITH_INVALID_SPECIFIERS => [
            new("foo", new KeyValuePair<string, string>("FOO[fi](\"first_specifier\", \"second_specifier\", \"third_specifier\")", "YES\n"), 0, [], 0),
            new("foo", new KeyValuePair<string, string>("BAR[fi](first_specifier)", "NO\n"), 0, [], 0),
            new("foo", new KeyValuePair<string, string>("BAR[fi](first_specifier, second_specifier)", "NO\n"), 0, [], 0),
            new("foo", new KeyValuePair<string, string>("BAZ[fi](\"first_specifier\" \"second_specifier\")", "NO\n"), 0, [], 0)
                ];

        internal static List<ValidationKeyValuePair> KEYVALUEPAIRS_WITH_ILLEGAL_SYMBOLS_IN_LANGUAGE_SECTIONS => [
            new("foo", new KeyValuePair<string, string>("FOO[\"fi\"](\"first_specifier\", \"second_specifier\")", "YES\n"), 0, [], 0),
            new("foo", new KeyValuePair<string, string>("BAR[\"[first_specifier]\", \"second_specifier\"]", "NO\n"), 0, [], 0),
            new("foo", new KeyValuePair<string, string>("BAZ[fi, en]", "NO\n"), 0, [], 0)
            ];

        internal static List<ValidationKeyValuePair> KEYVALUEPAIR_WITH_ILLEGAL_SYMBOLS_IN_SPECIFIER_SECTIONS => [
            new("foo", new KeyValuePair<string, string>("FOO([\"first_specifier\"], [\"second_specifier\"])", "YES\n"), 0, [], 0)
            ];

        private const string multilineStringValue = "\"dis parturient montes nascetur ridiculus mus\"\n" +
            "\"mauris vitae ultricies leo integer malesuada nunc vel risus commodo viverra maecenas accumsan lacus vel\"\n" +
            "\"facilisis volutpat est velit egestas dui id ornare\"";
        private const string multilineListValue = "\"dis\", \"parturient\", \"montes\", \"nascetur\", \"ridiculus\", \"mus\",\n" +
            "\"mauris\", \"vitae\", \"ultricies\", \"leo\", \"integer\", \"malesuada\", \"nunc\", \"vel\", \"commodo\", \"viverra\",\n" +
            "\"maecenas\", \"accumsan\", \"lacus\", \"vel\", \"facilisis\", \"volutpat\", \"est\", \"velit\", \"egestas\", \"dui\",\n" +
            "\"id\", \"ornare\"";
        internal static List<ValidationKeyValuePair> KEYVALUEPAIRS_WITH_CORRECTLY_FORMATTED_LIST_AND_MULTILINE_STRING => [
            new("foo", new KeyValuePair<string, string>("FOO", multilineStringValue), 0, [], 0),
            new("foo", new KeyValuePair<string, string>("BAR", multilineListValue), 0, [], 0)
            ];

        private const string badMultilineStringValue = "\"dis parturient montes nascetur ridiculus mus\"\n" +
            " \"mauris vitae ultricies leo integer malesuada nunc vel risus commodo viverra maecenas accumsan lacus vel\"\n" +
            "\"facilisis volutpat est velit egestas dui id ornare\";\n";
        private const string badMultilineListValue = "\"dis\", \"parturient\", \"montes\", \"nascetur\", \"ridiculus\", \"mus\"\n" +
            "\"mauris\", \"vitae\", \"ultricies\", \"leo\", \"integer\", \"malesuada\", \"nunc\", \"vel\", \"commodo\", \"viverra\",\n" +
            "\"maecenas\", \"accumsan\", \"lacus\", \"vel\", \"facilisis\", \"volutpat\", \"est\", \"velit\", \"egestas\", \"dui\",\n" +
            "\"id\", \"ornare\";\n";
        internal static List<ValidationKeyValuePair> KEYVALUEPAIRS_WITH_BAD_VALUES => [
            new("foo", new KeyValuePair<string, string>("CHARSET", "ANSI"), 0, [], 0),
            new("foo", new KeyValuePair<string, string>("FOO", "1 2 3"), 0, [], 0),
            new("foo", new KeyValuePair<string, string>("FOO", badMultilineStringValue), 0, [], 0),
            new("foo", new KeyValuePair<string, string>("BAR", badMultilineListValue), 0, [], 0)
        ];

        private const string excessWhitespaceStringValue = "\"dis\", \"parturient\", \"montes\", \"nascetur\", \"ridiculus\", \"mus\",\n" +
            "\"mauris\", \"vitae\", \"ultricies\", \"leo\", \"integer\", \"malesuada\",  \"nunc\", \"vel\", \"commodo\", \"viverra\",\n" +
            "\"maecenas\", \"accumsan\", \"lacus\", \"vel\", \"facilisis\", \"volutpat\", \"est\", \"velit\", \"egestas\", \"dui\",\n" +
            "\"id\", \"ornare\"";
        internal static List<ValidationKeyValuePair> KEYVALUEPAIR_WITH_EXCESS_LIST_VALUE_WHITESPACE => [
            new("foo", new KeyValuePair<string, string>("FOO", excessWhitespaceStringValue), 0, [], 0)
        ];

        private const string keyWithExcessWhitespace = "KEYWORD (fi) (\"first_specifier\", \"second_specifier\")";
        internal static List<ValidationKeyValuePair> KEYVALUEPAIR_WITH_EXCESS_KEY_WHITESPACE => [
            new("foo", new KeyValuePair<string, string>(keyWithExcessWhitespace, "foo"), 0, [], 0)
        ];

        private const string shortStringValueWithNewLine = "\"foobar foobar\"\n\"foobar foober\"";
        private const string shortListValueWithNewLine = "\"foo\", \"bar\",\n\"baz\"";
        internal static List<ValidationKeyValuePair> KEYVALUEPAIRS_WITH_SHORT_MULTILINE_VALUES => [
            new("foo", new KeyValuePair<string, string>("FOO", shortStringValueWithNewLine), 0, [], 0),
            new("foo", new KeyValuePair<string, string>("BAR", shortListValueWithNewLine), 0, [], 0)
        ];

        private readonly static ValidationStructuredEntryKey numberInKeyword = new("1FOO");
        private readonly static ValidationStructuredEntryKey slashInKeyword = new("B/AR");
        private readonly static ValidationStructuredEntryKey colonInKeyword = new("B:AZ");
        internal static List<ValidationStructuredEntry> STRUCTURED_ENTRIES_WITH_INVALID_KEYWORDS => [
            new("foo", numberInKeyword, "foo", 0, [], 0, Validation.ValueType.String),
            new("foo", slashInKeyword, "bar", 0, [], 0, Validation.ValueType.String),
            new("foo", colonInKeyword, "baz", 0, [], 0, Validation.ValueType.String)
        ];

        private readonly static ValidationStructuredEntryKey fi = new("FOO", "fi");
        private readonly static ValidationStructuredEntryKey fin = new("BAR", "fin");
        private readonly static ValidationStructuredEntryKey fiFi = new("BAZ", "fi-FI");
        internal static List<ValidationStructuredEntry> STRUCTURED_ENTRIES_WITH_VALID_LANGUAGES => [
            new("foo", fi, "foo", 0, [], 0, Validation.ValueType.String),
            new("foo", fin, "bar", 0, [], 0, Validation.ValueType.String),
            new("foo", fiFi, "baz", 0, [], 0, Validation.ValueType.String)
        ];

        private readonly static ValidationStructuredEntryKey fien = new("FOO", "fi en");
        internal static List<ValidationStructuredEntry> STRUCTURED_ENTRIES_WITH_INVALID_LANGUAGES => [
            new("foo", fien, "foo", 0, [], 0, Validation.ValueType.String)
        ];

        private readonly static ValidationStructuredEntryKey illegalSpecifier = new("FOO", "fi", "first\"specifier");
        internal static List<ValidationStructuredEntry> STRUCTIRED_ENTRIES_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIERS => [
            new("foo", illegalSpecifier, "foo", 0, [], 0, Validation.ValueType.String)
        ];

        internal static List<ValidationEntry> ENTRY_WITHOUT_VALUE => [
           new("foo", "LANGUAGES", 0, 0, [])
        ];

        private readonly static ValidationStructuredEntryKey finnish = new("FOO", "finnish");
        private readonly static ValidationStructuredEntryKey engl = new("BAR", "engl");
        internal static List<ValidationStructuredEntry> STRUCTURED_ENTRIES_WITH_INCOMPLIANT_LANGUAGES => [
            new("foo", finnish, "foo", 0, [], 0, Validation.ValueType.String),
            new("foo", engl, "foo", 0, [], 0, Validation.ValueType.String)
        ];

        private readonly static ValidationStructuredEntryKey longKeyword = new("THISISALONGKEYWORDWHICHISNOTRECOMMENDED");
        internal static List<ValidationStructuredEntry> STRUCTS_WITH_LONG_KEYWORD => [
            new("foo", longKeyword, "foo", 0, [], 0, Validation.ValueType.String)
        ];

        private readonly static ValidationStructuredEntryKey pascalCaseKeyword = new("PascalCase");
        private readonly static ValidationStructuredEntryKey screamingSnakeCaseKeyword = new("SCREAMING_SNAKE");
        internal static List<ValidationStructuredEntry> STRUCTURED_ENTRIES_WITH_UNRECOMMENDED_KEYWORD_NAMING => [
            new("foo", pascalCaseKeyword, "foo", 0, [], 0, Validation.ValueType.String),
            new("foo", screamingSnakeCaseKeyword, "foo", 0, [], 0, Validation.ValueType.String)
        ];
    }
}
