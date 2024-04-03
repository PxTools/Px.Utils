namespace PxUtils.PxFile
{
    public class PxFileSyntaxConf
    {
        public class SymbolCollection
        {
            public class KeySymbols
            {
                private const char LIST_SEPARATOR_DEFAULT = ',';
                private const char LANG_PARAM_START_DEFAULT = '[';
                private const char LANG_PARAM_END_DEFAULT = ']';
                private const char SPECIFIER_PARAM_START_DEFAULT = '(';
                private const char SPECIFIER_PARAM_END_DEFAULT = ')';
                private const char STRING_DELIMETER_DEFAULT = '"';

                public char ListSeparator { get; set; } = LIST_SEPARATOR_DEFAULT;
                public char LangParamStart { get; set; } = LANG_PARAM_START_DEFAULT;
                public char LangParamEnd { get; set; } = LANG_PARAM_END_DEFAULT;
                public char SpecifierParamStart { get; set; } = SPECIFIER_PARAM_START_DEFAULT;
                public char SpecifierParamEnd { get; set; } = SPECIFIER_PARAM_END_DEFAULT;
                public char StringDelimeter { get; set; } = STRING_DELIMETER_DEFAULT;

                private KeySymbols() { }

                public static KeySymbols DefaultKeySymbols => new();
            }

            public class ValueSymbols
            {
                private const char STRING_DELIMETER_DEFAULT = '"';
                private const char LIST_SEPARATOR_DEFAULT = ',';
                private const char TIME_SERIES_INTERVAL_START_DEFAULT = '(';
                private const char TIME_SERIES_INTERVAL_END_DEFAULT = ')';
                private const char TIME_SERIES_LIMITS_SEPARATOR_DEFAULT = '-';
                private const string DATETIME_FORMAT_DEFAULT = "yyyyMMdd HH:mm";

                public char StringDelimeter { get; set; } = STRING_DELIMETER_DEFAULT;
                public char ListSeparator { get; set; } = LIST_SEPARATOR_DEFAULT;
                public char TimeSeriesIntervalStart { get; set; } = TIME_SERIES_INTERVAL_START_DEFAULT;
                public char TimeSeriesIntervalEnd { get; set; } = TIME_SERIES_INTERVAL_END_DEFAULT;
                public char TimeSeriesLimitsSeparator { get; set; } = TIME_SERIES_LIMITS_SEPARATOR_DEFAULT;
                public string DateTimeFormat { get; set; } = DATETIME_FORMAT_DEFAULT;

                private ValueSymbols() { }

                public static ValueSymbols DefaultValueSymbols => new();
            }

            private const char KEYWORD_SEPARATOR_DEFAULT = '=';
            private const char ENTRY_SEPARATOR_DEFAULT = ';';
            private const char LINE_BREAK_DEFAULT = '\n';
            private const char END_OF_STREAM_DEFAULT = '\0';

            public char KeywordSeparator { get; set; } = KEYWORD_SEPARATOR_DEFAULT;
            public char EntrySeparator { get; set; } = ENTRY_SEPARATOR_DEFAULT;
            public char Linebreak { get; set; } = LINE_BREAK_DEFAULT;
            public char EndOfStream { get; set; } = END_OF_STREAM_DEFAULT;

            public KeySymbols Key { get; set; }
            public ValueSymbols Value { get; set; }

            private SymbolCollection()
            {
                Key = KeySymbols.DefaultKeySymbols;
                Value = ValueSymbols.DefaultValueSymbols;
            }

            public static SymbolCollection DefaultSymbols => new();
        }

        public class TokenCollection
        {
            public class TimeValue
            {
                private const string TIME_INTERVAL_INDICATOR = "TLIST";
                private const string YEAR_INTERVAL = "A1";
                private const string HALF_YEAR_INTERVAL = "H1";
                private const string QUARTER_YEAR_INTERVAL = "Q1";
                private const string MONTH_INTERVAL = "M1";
                private const string WEEK_INTERVAL = "W1";

                private const string DATETIME_FORMAT_STRING = "yyyyMMdd HH:mm";

                public string TimeIntervalIndicator { get; set; } = TIME_INTERVAL_INDICATOR;
                public string YearInterval { get; set; } = YEAR_INTERVAL;
                public string HalfYearInterval { get; set; } = HALF_YEAR_INTERVAL;
                public string QuarterYearInterval { get; set; } = QUARTER_YEAR_INTERVAL;
                public string MonthInterval { get; set; } = MONTH_INTERVAL;
                public string WeekInterval { get; set; } = WEEK_INTERVAL;
                public string DateTimeFormatString { get; set; } = DATETIME_FORMAT_STRING;

                private TimeValue() { }

                public static TimeValue DefaultTimeValue => new();
            }

            public class VariableTypeTokens
            {
                private const string CONTENT = "Content";
                private const string TIME = "Time";
                private const string ORDINAL = "Ordinal";
                private const string NOMINAL = "Nominal";
                private const string GEOGRAPHICAL = "Geographical";
                private const string OTHER = "Other";
                private const string UNKNOWN = "Unknown";

                public string Content { get; set; } = CONTENT;
                public string Time { get; set; } = TIME;
                public string Ordinal { get; set; } = ORDINAL;
                public string Nominal { get; set; } = NOMINAL;
                public string Geographical { get; set; } = GEOGRAPHICAL;
                public string Other { get; set; } = OTHER;
                public string Unknown { get; set; } = UNKNOWN;

                private VariableTypeTokens() { }

                public static VariableTypeTokens DefaultVariableTypeTokens => new();
            }

            public class KeyWordTokens
            {
                private const string DATA = "DATA";
                private const string CODEPAGE = "CODEPAGE";
                private const string DEFAULT_LANGUAGE = "LANGUAGE";
                private const string AVAILABLE_LANGUAGES = "LANGUAGES";
                private const string CONTENT_VARIABLE_IDENTIFIER = "CONTVARIABLE";
                private const string VARIABLE_CODE = "VARIABLECODE";
                private const string VARIABLE_VALUE_CODES = "CODES";
                private const string VARIABLE_VALUES = "VALUES";
                private const string UNITS = "UNITS";
                private const string LAST_UPDATED = "LAST-UPDATED";
                private const string PRECISION = "PRECISION";
                private const string DIMENSION_DEFAULT_VALUE = "ELIMINATION";
                private const string TIME_VAL = "TIMEVAL";
                private const string MAP = "MAP";
                private const string DIMENSION_TYPE = "VARIABLE-TYPE";
                private const string STUB_DIMENSIONS = "STUB";
                private const string HEADING_DIMENSIONS = "HEADING";
                private const string CHARSET = "CHARSET";
                private const string TABLEID = "TABLEID";
                private const string DESCRIPTION = "DESCRIPTION";

                public string Data { get; set; } = DATA;
                public string CodePage { get; set; } = CODEPAGE;
                public string DefaultLanguage { get; set; } = DEFAULT_LANGUAGE;
                public string AvailableLanguages { get; set; } = AVAILABLE_LANGUAGES;
                public string ContentVariableIdentifier { get; set; } = CONTENT_VARIABLE_IDENTIFIER;
                public string DimensionCode { get; set; } = VARIABLE_CODE;
                public string VariableValueCodes { get; set; } = VARIABLE_VALUE_CODES;
                public string VariableValues { get; set; } = VARIABLE_VALUES;
                public string Units { get; set; } = UNITS;
                public string LastUpdated { get; set; } = LAST_UPDATED;
                public string Precision { get; set; } = PRECISION;
                public string DimensionDefaultValue { get; set; } = DIMENSION_DEFAULT_VALUE;
                public string TimeVal { get; set; } = TIME_VAL;
                public string Map { get; set; } = MAP;
                public string DimensionType { get; set; } = DIMENSION_TYPE;
                public string StubDimensions { get; set; } = STUB_DIMENSIONS;
                public string HeadingDimensions { get; set; } = HEADING_DIMENSIONS;
                public string Charset { get; set; } = CHARSET;
                public string TableId { get; set; } = TABLEID;
                public string Description { get; set; } = DESCRIPTION;

                private KeyWordTokens() { }

                public static KeyWordTokens DefaultKeyWordTokens => new();
            }

            public class BooleanTokens
            {
                private const string YES = "YES";
                private const string NO = "NO";

                public string Yes { get; set; } = YES;
                public string No { get; set; } = NO;

                public static BooleanTokens DefaultBooleanTokens => new();
            }

            public TimeValue Time { get; set; }

            public KeyWordTokens KeyWords { get; set; }

            public VariableTypeTokens VariableTypes { get; set; }
            public BooleanTokens Booleans { get; set; }

            private TokenCollection()
            {
                Time = TimeValue.DefaultTimeValue;
                KeyWords = KeyWordTokens.DefaultKeyWordTokens;
                VariableTypes = VariableTypeTokens.DefaultVariableTypeTokens;
                Booleans = BooleanTokens.DefaultBooleanTokens;
            }

            public static TokenCollection DefaultTokens => new();
        }

        public SymbolCollection Symbols { get; set; }
        public TokenCollection Tokens { get; set; }

        private PxFileSyntaxConf()
        {
            Symbols = SymbolCollection.DefaultSymbols;
            Tokens = TokenCollection.DefaultTokens;
        }

        public static PxFileSyntaxConf Default => new();
    }
}
