namespace PxUtils.PxFile
{
    public class PxFileSymbolsConf
    {
        public class TokenCollection
        {
            public class KeyTokens
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

                private KeyTokens() { }

                public static KeyTokens DefaultKeyTokens => new();
            }

            public class ValueTokens
            {
                private const char STRING_DELIMETER_DEFAULT = '"';
                private const char TIME_SERIES_INTERVAL_START_DEFAULT = '(';
                private const char TIME_SERIES_INTERVAL_END_DEFAULT = ')';
                private const char TIME_SERIES_LIMITS_SEPARATOR_DEFAULT = '-';

                public char StringDelimeter { get; set; } = STRING_DELIMETER_DEFAULT;
                public char TimeSeriesIntervalStart { get; set; } = TIME_SERIES_INTERVAL_START_DEFAULT;
                public char TimeSeriesIntervalEnd { get; set; } = TIME_SERIES_INTERVAL_END_DEFAULT;
                public char TimeSeriesLimitsSeparator { get; set; } = TIME_SERIES_LIMITS_SEPARATOR_DEFAULT;

                private ValueTokens() { }

                public static ValueTokens DefaultValueTokens => new();
            }

            private const char KEYWORD_SEPARATOR_DEFAULT = '=';
            private const char SECTION_SEPARATOR_DEFAULT = ';';

            public char KeywordSeparator { get; set; } = KEYWORD_SEPARATOR_DEFAULT;
            public char SectionSeparator { get; set; } = SECTION_SEPARATOR_DEFAULT;

            public KeyTokens Key { get; set; }
            public ValueTokens Value { get; set; }

            private TokenCollection()
            {
                Key = KeyTokens.DefaultKeyTokens;
                Value = ValueTokens.DefaultValueTokens;
            }

            public static TokenCollection DefaultTokens => new();
        }

        public class SymbolsCollection
        {
            public class TimeValue
            {
                private const string TIME_INTERVAL_INDICATOR = "TLIST";
                private const string YEAR_INTERVAL = "A1";
                private const string HALF_YEAR_INTERVAL = "H1";
                private const string QUARTER_YEAR_INTERVAL = "Q1";
                private const string MONTH_INTERVAL = "M1";
                private const string WEEK_INTERVAL = "W1";

                public string TimeIntervalIndicator { get; set; } = TIME_INTERVAL_INDICATOR;
                public string YearInterval { get; set; } = YEAR_INTERVAL;
                public string HalfYearInterval { get; set; } = HALF_YEAR_INTERVAL;
                public string QuarterYearInterval { get; set; } = QUARTER_YEAR_INTERVAL;
                public string MonthInterval { get; set; } = MONTH_INTERVAL;
                public string WeekInterval { get; set; } = WEEK_INTERVAL;

                private TimeValue() { }

                public static TimeValue DefaultTimeValue => new();
            }

            public class KeyWordSymbols
            {
                private const string DATA = "DATA";
                private const string CODEPAGE = "CODEPAGE";

                public string Data { get; set; } = DATA;
                public string CodePage { get; set; } = CODEPAGE;

                private KeyWordSymbols() { }

                public static KeyWordSymbols DefaultKeyWordSymbols => new();
            }

            public TimeValue Time { get; set; }

            public KeyWordSymbols KeyWords { get; set; }

            private SymbolsCollection()
            {
                Time = TimeValue.DefaultTimeValue;
                KeyWords = KeyWordSymbols.DefaultKeyWordSymbols;
            }

            public static SymbolsCollection DefaultSymbols => new();
        }

        public TokenCollection Tokens { get; set; }
        public SymbolsCollection Symbols { get; set; }

        private PxFileSymbolsConf()
        {
            Tokens = TokenCollection.DefaultTokens;
            Symbols = SymbolsCollection.DefaultSymbols;
        }

        public static PxFileSymbolsConf Default => new();
    }
}
