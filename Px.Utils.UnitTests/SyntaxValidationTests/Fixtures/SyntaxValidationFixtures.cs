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

        internal static List<StringValidationEntry> MULTIPLE_ENTRIES_IN_SINGLE_LINE => [
            new(0, 0, "foo", "CHARSET=\"ANSI\";", PxFileSyntaxConf.Default, 0),
            new(1, 0, "foo", "AXIS-VERSION=\"2013\";", PxFileSyntaxConf.Default, 1),
            new(2, 0, "foo", "CODEPAGE=\"utf-8\";", PxFileSyntaxConf.Default, 2),
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

        internal static List<KeyValuePairValidationEntry> ENTRY_WITH_MULTIPLE_LANGUAGE_PARAMETERS => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO[fi][en](\"first_specifier\", \"second_specifier\")", "YES\n"), PxFileSyntaxConf.Default)
                ];

        internal static List<KeyValuePairValidationEntry> ENTRY_WITH_MULTIPLE_SPECIFIER_PARAMETER_SECTIONS => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO[fi](\"first_specifier\")(\"second_specifier\")", "YES\n"), PxFileSyntaxConf.Default)
                ];

        internal static List<KeyValuePairValidationEntry> ENTRIES_IN_WRONG_ORDER_AND_MISSING_KEYWORD => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO(\"first_specifier\")[fi]", "YES\n"), PxFileSyntaxConf.Default),
            new(1, 0, "foo", new KeyValuePair<string, string>("[en]BAR(\"first_specifier\")", "NO\n"), PxFileSyntaxConf.Default),
            new(2, 0, "foo", new KeyValuePair<string, string>("[fi](\"first_specifier\")", "baz\n"), PxFileSyntaxConf.Default)
                ];

        internal static List<KeyValuePairValidationEntry> ENTRIES_WITH_INVALID_SPECIFIERS => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO[fi](\"first_specifier\", \"second_specifier\", \"third_specifier\")", "YES\n"), PxFileSyntaxConf.Default),
            new(1, 0, "foo", new KeyValuePair<string, string>("BAR[fi](first_specifier\")", "NO\n"), PxFileSyntaxConf.Default),
            new(2, 0, "foo", new KeyValuePair<string, string>("BAZ[fi](\"first_specifier\" \"second_specifier\")", "NO\n"), PxFileSyntaxConf.Default)
                ];

        internal static List<KeyValuePairValidationEntry> ENTRIES_WITH_ILLEGAL_SYMBOLS_IN_PARAM_SECTIONS => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO[\"fi\"](\"first_specifier\", \"second_specifier\")", "YES\n"), PxFileSyntaxConf.Default),
            new(1, 0, "foo", new KeyValuePair<string, string>("BAR[\"[first_specifier]\", \"second_specifier\"]", "NO\n"), PxFileSyntaxConf.Default),
            new(2, 0, "foo", new KeyValuePair<string, string>("BAZ[fi, en]", "NO\n"), PxFileSyntaxConf.Default)
            ];

        private const string multilineStringValue = "\"dis parturient montes nascetur ridiculus mus\"\n" +
            "\"mauris vitae ultricies leo integer malesuada nunc vel risus commodo viverra maecenas accumsan lacus vel\"\n" +
            "\"facilisis volutpat est velit egestas dui id ornare\"";
        private const string multilineListValue = "\"dis\", \"parturient\", \"montes\", \"nascetur\", \"ridiculus\", \"mus\",\n" +
            "\"mauris\", \"vitae\", \"ultricies\", \"leo\", \"integer\", \"malesuada\", \"nunc\", \"vel\", \"commodo\", \"viverra\",\n" +
            "\"maecenas\", \"accumsan\", \"lacus\", \"vel\", \"facilisis\", \"volutpat\", \"est\", \"velit\", \"egestas\", \"dui\",\n" +
            "\"id\", \"ornare\"";
        internal static List<KeyValuePairValidationEntry> ENTRIES_WITH_CORRECTLY_FORMATTED_LIST_AND_MULTILINE_STRING => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO", multilineStringValue), PxFileSyntaxConf.Default),
            new(1, 0, "foo", new KeyValuePair<string, string>("BAR", multilineListValue), PxFileSyntaxConf.Default),
            ];

        private const string badMultilineStringValue = "\"dis parturient montes nascetur ridiculus mus\"\n" +
            " \"mauris vitae ultricies leo integer malesuada nunc vel risus commodo viverra maecenas accumsan lacus vel\"\n" +
            "\"facilisis volutpat est velit egestas dui id ornare\";\n";
        private const string badMultilineListValue = "\"dis\", \"parturient\", \"montes\", \"nascetur\", \"ridiculus\", \"mus\"\n" +
            "\"mauris\", \"vitae\", \"ultricies\", \"leo\", \"integer\", \"malesuada\", \"nunc\", \"vel\", \"commodo\", \"viverra\",\n" +
            "\"maecenas\", \"accumsan\", \"lacus\", \"vel\", \"facilisis\", \"volutpat\", \"est\", \"velit\", \"egestas\", \"dui\",\n" +
            "\"id\", \"ornare\";\n";
        internal static List<KeyValuePairValidationEntry> ENTRIES_WITH_BAD_VALUES => [
            new(0, 0, "foo", new KeyValuePair<string, string>("CHARSET", "ANSI"), PxFileSyntaxConf.Default),
            new(1, 0, "foo", new KeyValuePair<string, string>("FOO", badMultilineStringValue), PxFileSyntaxConf.Default),
            new(2, 0, "foo", new KeyValuePair<string, string>("BAR", badMultilineListValue), PxFileSyntaxConf.Default),
        ];

        private const string excessWhitespaceStringValue = "\"dis\", \"parturient\", \"montes\", \"nascetur\", \"ridiculus\", \"mus\",\n" +
            "\"mauris\", \"vitae\", \"ultricies\", \"leo\", \"integer\", \"malesuada\",  \"nunc\", \"vel\", \"commodo\", \"viverra\",\n" +
            "\"maecenas\", \"accumsan\", \"lacus\", \"vel\", \"facilisis\", \"volutpat\", \"est\", \"velit\", \"egestas\", \"dui\",\n" +
            "\"id\", \"ornare\"";
        internal static List<KeyValuePairValidationEntry> ENTRY_WITH_EXCESS_LIST_VALUE_WHITESPACE => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO", excessWhitespaceStringValue), PxFileSyntaxConf.Default),
        ];

        private const string shortStringValueWithNewLine = "\"foobar foobar\"\n\"foobar foober\"";
        private const string shortListValueWithNewLine = "\"foo\", \"bar\",\n\"baz\"";
        internal static List<KeyValuePairValidationEntry> ENTRY_WITH_SHORT_MULTILINE_VALUES => [
            new(0, 0, "foo", new KeyValuePair<string, string>("FOO", shortStringValueWithNewLine), PxFileSyntaxConf.Default),
            new(1, 0, "foo", new KeyValuePair<string, string>("BAR", shortListValueWithNewLine), PxFileSyntaxConf.Default)
        ];

        private readonly static ValidationEntryKey numberInKeyword = new("1FOO");
        private readonly static ValidationEntryKey slashInKeyword = new("B/AR");
        private readonly static ValidationEntryKey colonInKeyword = new("B:AZ");
        internal static List<StructuredValidationEntry> ENTRIES_WITH_INVALID_KEYWORDS => [
            new(0, 0, "foo", numberInKeyword, "foo"),
            new(1, 0, "foo", slashInKeyword, "bar"),
            new(2, 0, "foo", colonInKeyword, "baz")
        ];

        private readonly static ValidationEntryKey fi = new("FOO", "fi");
        private readonly static ValidationEntryKey fin = new("BAR", "fin");
        private readonly static ValidationEntryKey fiFi = new("BAZ", "fi-FI");
        internal static List<StructuredValidationEntry> ENTRIES_WITH_VALID_LANGUAGES => [
            new(0, 0, "foo", fi, "foo"),
            new(1, 0, "foo", fin, "bar"),
            new(2, 0, "foo", fiFi, "baz")
        ];

        private readonly static ValidationEntryKey fien = new("FOO", "fi en");
        internal static List<StructuredValidationEntry> ENTRIES_WITH_INVALID_LANGUAGES => [
            new(0, 0, "foo", fien, "foo"),
        ];

        private readonly static ValidationEntryKey illegalSpecifier = new("FOO", "fi", "first\"specifier");
        internal static List<StructuredValidationEntry> ENTRY_WITH_ILLEGAL_CHARACTERS_IN_SPECIFIERS => [
            new(0, 0, "foo", illegalSpecifier, "foo"),
        ];

        internal static List<StringValidationEntry> ENTRY_WITHOUT_VALUE => [
           new(0, 0, "LANGUAGES", "", PxFileSyntaxConf.Default, 0)
        ];

        private readonly static ValidationEntryKey finnish = new("FOO", "finnish");
        private readonly static ValidationEntryKey engl = new("BAR", "engl");
        internal static List<StructuredValidationEntry> ENTRIES_WITH_INCOMPLIANT_LANGUAGES => [
            new(0, 0, "foo", finnish, "foo"),
            new(1, 0, "foo", engl, "foo"),
        ];

        private readonly static ValidationEntryKey longKeyword = new("THISISALONGKEYWORDWHICHISNOTRECOMMENDED");
        internal static List<StructuredValidationEntry> ENTRY_WITH_LONG_KEYWORD => [
            new(0, 0, "foo", longKeyword, "foo"),
        ];

        private readonly static ValidationEntryKey pascalCaseKeyword = new("PascalCase");
        private readonly static ValidationEntryKey screamingSnakeCaseKeyword = new("SCREAMING_SNAKE");
        internal static List<StructuredValidationEntry> ENTRIES_WITH_UNRECOMMENDED_KEYWORD_NAMING => [
            new(0, 0, "foo", pascalCaseKeyword, "foo"),
            new(1, 0, "foo", screamingSnakeCaseKeyword, "foo")
        ];
    }
}
