using PxUtils.Validation.SyntaxValidation;

namespace PxUtils.UnitTests.ContentValidationTests.Fixtures
{
    internal static class ContentValidationFixtures
    {
        private const string filename = "foo";

        private static readonly ValidationStructuredEntry languagesEntry =
            new(filename,
                new ValidationStructuredEntryKey("LANGUAGES"),
                "fi,en",
                4,
                [],
                11,
                Validation.ValueType.ListOfStrings);

        private static readonly ValidationStructuredEntry charsetEntry =
            new(filename,
                new ValidationStructuredEntryKey("CHARSET"),
                "ANSI",
                1,
                [],
                8,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry axisVersionEntry =
            new(filename,
                new ValidationStructuredEntryKey("AXIS-VERSION"),
                "2013",
                1,
                [],
                14,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry codepageEntry =
            new(filename,
                new ValidationStructuredEntryKey("CODEPAGE"),
                "UTF-8",
                2,
                [],
                10,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry languageEntry =
            new(filename,
                new ValidationStructuredEntryKey("LANGUAGE"),
                "fi",
                3,
                [],
                8,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry nextUpdateEntry =
            new(filename,
                new ValidationStructuredEntryKey("NEXT-UPDATE"),
                "20240131 08:00",
                5,
                [],
                13,
                Validation.ValueType.DateTime);

        private static readonly ValidationStructuredEntry subjectAreaEntry =
            new(filename,
                new ValidationStructuredEntryKey("SUBJECT-AREA"),
                "test",
                6,
                [],
                14,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry copyrightEntry =
            new(filename,
                new ValidationStructuredEntryKey("COPYRIGHT"),
                "YES",
                7,
                [],
                11,
                Validation.ValueType.Boolean);

        private static readonly ValidationStructuredEntry stubEntry =
            new(filename,
                new ValidationStructuredEntryKey("STUB"),
                "bar,bar-time",
                8,
                [],
                5,
                Validation.ValueType.ListOfStrings);

        private static readonly ValidationStructuredEntry stubEnEntry =
            new(filename,
                new ValidationStructuredEntryKey("STUB", "en"),
                "bar-en,bar-time-en",
                9,
                [],
                9,
                Validation.ValueType.ListOfStrings);

        private static readonly ValidationStructuredEntry contVariableEntry =
            new(filename,
                new ValidationStructuredEntryKey("CONTVARIABLE"),
                "bar",
                10,
                [],
                8,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry contVariableEnEntry =
            new(filename,
                new ValidationStructuredEntryKey("CONTVARIABLE", "en"),
                "bar-en",
                11,
                [],
                12,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry tableIdEntry =
            new(filename,
                new ValidationStructuredEntryKey("TABLEID"),
                "baz",
                12,
                [],
                9,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry descriptionEntry =
            new(filename,
                new ValidationStructuredEntryKey("DESCRIPTION"),
                "foobar",
                13,
                [],
                12,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry descriptionEnEntry =
            new(filename,
                new ValidationStructuredEntryKey("DESCRIPTION", "en"),
                "foobar-en",
                14,
                [],
                16,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry valuesBarEntry =
            new(filename,
                new ValidationStructuredEntryKey("VALUES", null, "bar"),
                "foo",
                15,
                [],
                7,
                Validation.ValueType.ListOfStrings);

        private static readonly ValidationStructuredEntry valuesBarEnEntry =
            new(filename,
                new ValidationStructuredEntryKey("VALUES", "en", "bar-en"),
                "foo-en",
                16,
                [],
                11,
                Validation.ValueType.ListOfStrings);

        private static readonly ValidationStructuredEntry lastUpdatedBarFooEntry =
            new(filename,
                new ValidationStructuredEntryKey("LAST-UPDATED", null, "bar", "foo"),
                "20230131 08:00",
                17,
                [],
                13,
                Validation.ValueType.DateTime);

        private static readonly ValidationStructuredEntry unitsBarFooEntry =
            new(filename,
                new ValidationStructuredEntryKey("UNITS", null, "bar", "foo"),
                "unit",
                19,
                [],
                13,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry unitsBarFooEnEntry =
            new(filename,
                new ValidationStructuredEntryKey("UNITS", "en", "bar-en", "foo-en"),
                "unit-en",
                20,
                [],
                17,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry precisionBarFooEntry =
            new(filename,
                new ValidationStructuredEntryKey("PRECISION", null, "bar", "foo"),
                "0",
                21,
                [],
                13,
                Validation.ValueType.Number);

        private static readonly ValidationStructuredEntry variableTypeBarEntry =
            new(filename,
                new ValidationStructuredEntryKey("VARIABLE-TYPE", null, "bar"),
                "Content",
                22,
                [],
                13,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry timevalEntry =
            new(filename,
                new ValidationStructuredEntryKey("TIMEVAL", null, "bar-time"),
                "TLIST(A1),2000,2001,2002,2003",
                23,
                [],
                17,
                Validation.ValueType.TimeValSeries);

        private static readonly ValidationStructuredEntry timeValEnEntry =
            new(filename,
                new ValidationStructuredEntryKey("TIMEVAL", "en", "bar-time-en"),
                "TLIST(A1),2000,2001,2002,2003",
                24,
                [],
                17,
                Validation.ValueType.TimeValSeries);

        private static readonly ValidationStructuredEntry valuesBarTimeEntry =
            new(filename,
                new ValidationStructuredEntryKey("VALUES", null, "bar-time"),
                "2000,2001,2002,2003",
                25,
                [],
                17,
                Validation.ValueType.ListOfStrings);

        private static readonly ValidationStructuredEntry valuesBarTimeEnEntry =
            new(filename,
                new ValidationStructuredEntryKey("VALUES", "en", "bar-time-en"),
                "2000,2001,2002,2003",
                26,
                [],
                17,
                Validation.ValueType.ListOfStrings);

        private static readonly ValidationStructuredEntry variableCodeBarEntry =
            new(filename,
                new ValidationStructuredEntryKey("VARIABLECODE", null, "bar"),
                "bar-code",
                27,
                [],
                5,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry variableCodeBarEnEntry =
            new(filename,
                new ValidationStructuredEntryKey("VARIABLECODE", "en", "bar-en"),
                "bar-en-code",
                28,
                [],
                9,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry variableTypeBarTimeEntry =
            new(filename,
                new ValidationStructuredEntryKey("VARIABLE-TYPE", null, "bar-time"),
                "Time",
                29,
                [],
                13,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry codesBarEntry =
            new(filename,
                new ValidationStructuredEntryKey("CODES", null, "bar"),
                "code-foo",
                30,
                [],
                13,
                Validation.ValueType.ListOfStrings);

        private static readonly ValidationStructuredEntry codesBarEnEntry =
            new(filename,
                new ValidationStructuredEntryKey("CODES", "en", "bar-en"),
                "code-en-foo",
                31,
                [],
                13,
                Validation.ValueType.ListOfStrings);

        private static readonly ValidationStructuredEntry codesBarTimeEntry =
            new(filename,
                new ValidationStructuredEntryKey("CODES", null, "bar-time"),
                "2000,2001,2002,2003",
                32,
                [],
                17,
                Validation.ValueType.ListOfStrings);

        private static readonly ValidationStructuredEntry codesBarTimeEnEntry =
            new(filename,
                new ValidationStructuredEntryKey("CODES", "en", "bar-time-en"),
                "2000,2001,2002,2003",
                33,
                [],
                17,
                Validation.ValueType.ListOfStrings);

        private static readonly ValidationStructuredEntry variableCodeBarTimeEntry =
            new(filename,
                new ValidationStructuredEntryKey("VARIABLECODE", null, "bar-time"),
                "bar-time-code",
                34,
                [],
                17,
                Validation.ValueType.String);

        private static readonly ValidationStructuredEntry variableCodeBarTimeEnEntry =
            new(filename,
                new ValidationStructuredEntryKey("VARIABLECODE", "en", "bar-time-en"),
                "bar-time-en-code",
                35,
                [],
                17,
                Validation.ValueType.String);

        internal static ValidationStructuredEntry[] MINIMAL_STRUCTURED_ENTRY_ARRAY =>
        new[]
        {
            charsetEntry,
            axisVersionEntry,
            codepageEntry,
            languageEntry,
            languagesEntry,
            nextUpdateEntry,
            subjectAreaEntry,
            copyrightEntry,
            stubEntry,
            stubEnEntry,
            contVariableEntry,
            contVariableEnEntry,
            tableIdEntry,
            descriptionEntry,
            descriptionEnEntry,
            valuesBarEntry,
            valuesBarEnEntry,
            lastUpdatedBarFooEntry,
            unitsBarFooEntry,
            unitsBarFooEnEntry,
            precisionBarFooEntry,
            variableTypeBarEntry,
            timevalEntry,
            timeValEnEntry,
            valuesBarTimeEntry,
            valuesBarTimeEnEntry,
            variableCodeBarEntry,
            variableCodeBarEnEntry,
            variableTypeBarTimeEntry,
            codesBarEntry,
            codesBarEnEntry,
            codesBarTimeEntry,
            codesBarTimeEnEntry,
            variableCodeBarTimeEntry,
            variableCodeBarTimeEnEntry
        };

        internal static ValidationStructuredEntry[] EMPTY_STRUCTURED_ENTRY_ARRAY => [];

        internal static ValidationStructuredEntry[] STRUCTURED_ENTRY_ARRAY_WITH_DEFAULT_LANGUAGE =>
            new[] {
                new ValidationStructuredEntry(
                    languageEntry.File,
                    languageEntry.Key,
                    "foo",
                    languageEntry.KeyStartLineIndex,
                    languageEntry.LineChangeIndexes,
                    languageEntry.ValueStartIndex,
                    languageEntry.ValueType),
            };

        internal static ValidationStructuredEntry[] STRUCTURED_ENTRY_ARRAY_WITH_MISSING_CONTVARIABLE =>
            new[]
            {
                contVariableEntry,
            };

        internal static ValidationStructuredEntry[] STRUCTURED_ENTRY_ARRAY_WITH_STUB =>
            new[]
            {
                stubEntry,
            };

        internal static ValidationStructuredEntry[] STRUCTURED_ENTRY_ARRAY_WITH_DESCRIPTION =>
            new[]
            {
                descriptionEntry,
            };

        internal static ValidationStructuredEntry[] STRUCTURED_ENTRY_ARRAY_WITH_DIMENSIONVALUES =>
            new[]
            {
                valuesBarEntry,
            };

        internal static ValidationStructuredEntry[] STRUCTURED_ENTRY_ARRAY_WITH_INVALID_CONTENT_VALUE_KEY_ENTRIES =>
            new[]
            {
                unitsBarFooEntry,
                precisionBarFooEntry,
            };

        internal static ValidationStructuredEntry[] STRUCTURED_ENTRY_ARRAY_WITH_INCOMPLETE_VARIABLE_RECOMMENDED_KEYS =>
            new[]
            {
                timevalEntry,
                variableCodeBarEntry,
                variableCodeBarEnEntry,
                variableCodeBarTimeEnEntry,
                codesBarEntry,
                codesBarEnEntry,
                codesBarTimeEnEntry,
                variableTypeBarEntry,
            };

        internal static ValidationStructuredEntry StructuredEntryWithIllegalSpecifiers =>
            new(filename,
                new ValidationStructuredEntryKey("LANGUAGES", null, "foo", "bar"),
                "fi,en",
                16,
                [],
                11,
                Validation.ValueType.ListOfStrings);

        internal static ValidationStructuredEntry StructuredEntryWithIllegalLanguageParameter =>
            new(filename,
                new ValidationStructuredEntryKey("LANGUAGES", "en"),
                "fi,en",
                16,
                [],
                11,
                Validation.ValueType.ListOfStrings);

        internal static ValidationStructuredEntry StructuredEntryWithUndefinedLanguage =>
            new(filename,
                new ValidationStructuredEntryKey("VALUES", "sv", "bar"),
                "foo,baz",
                16,
                [],
                11,
                Validation.ValueType.ListOfStrings);

        internal static ValidationStructuredEntry StructuredEntryWithUndefinedFirstSpecifier =>
           new(filename,
                new ValidationStructuredEntryKey("KEYWORD", "en", "unknown", "foo-en"),
                "unit-en",
                20,
                [],
                17,
                Validation.ValueType.String);

        internal static ValidationStructuredEntry StructuredEntryWithUndefinedSecondSpecifier =>
            new(filename,
                new ValidationStructuredEntryKey("KEYWORD", "en", "bar-en", "unknown"),
                "unit-en",
                20,
                [],
                17,
                Validation.ValueType.String);

        internal static ValidationStructuredEntry[] STRUCTURED_ENTRY_ARRAY_WITH_INVALID_VALUE_TYPES =>
            new[]
            {
                new ValidationStructuredEntry(filename,
                    charsetEntry.Key,
                    "1",
                    charsetEntry.ValueStartIndex,
                    charsetEntry.LineChangeIndexes,
                    charsetEntry.ValueStartIndex,
                    Validation.ValueType.Number),
                new ValidationStructuredEntry(filename,
                    languagesEntry.Key,
                    "fi",
                    languagesEntry.ValueStartIndex,
                    languagesEntry.LineChangeIndexes,
                    languagesEntry.ValueStartIndex,
                    Validation.ValueType.String),
                new ValidationStructuredEntry(filename,
                    lastUpdatedBarFooEntry.Key,
                    "1/1/2023",
                    lastUpdatedBarFooEntry.ValueStartIndex,
                    lastUpdatedBarFooEntry.LineChangeIndexes,
                    lastUpdatedBarFooEntry.ValueStartIndex,
                    Validation.ValueType.String),
                new ValidationStructuredEntry(filename,
                    precisionBarFooEntry.Key,
                    "100%",
                    precisionBarFooEntry.ValueStartIndex,
                    precisionBarFooEntry.LineChangeIndexes,
                    precisionBarFooEntry.ValueStartIndex,
                    Validation.ValueType.String),
                new ValidationStructuredEntry(filename,
                    timevalEntry.Key,
                    lastUpdatedBarFooEntry.Value,
                    timevalEntry.ValueStartIndex,
                    timevalEntry.LineChangeIndexes,
                    timevalEntry.ValueStartIndex,
                    Validation.ValueType.DateTime)
            };

        internal static ValidationStructuredEntry[] STRUCTURED_ENTRY_ARRAY_WITH_WRONG_VALUES =>
            new[]
            {
                new ValidationStructuredEntry(filename,
                    charsetEntry.Key,
                    "foo",
                    charsetEntry.ValueStartIndex,
                    charsetEntry.LineChangeIndexes,
                    charsetEntry.ValueStartIndex,
                    Validation.ValueType.String),
                new ValidationStructuredEntry(filename,
                    codepageEntry.Key,
                    "bar",
                    codepageEntry.ValueStartIndex,
                    codepageEntry.LineChangeIndexes,
                    codepageEntry.ValueStartIndex,
                    Validation.ValueType.String),
                new ValidationStructuredEntry(filename,
                    variableTypeBarEntry.Key,
                    "foo",
                    variableTypeBarEntry.ValueStartIndex,
                    variableTypeBarEntry.LineChangeIndexes,
                    variableTypeBarEntry.ValueStartIndex,
                    Validation.ValueType.String),
            };

        internal static ValidationStructuredEntry StructuredEntryWithUnmatchingAmountOfElements =>
            new(filename,
                codesBarEntry.Key,
                "foo,bar,baz",
                codesBarEntry.ValueStartIndex,
                codesBarEntry.LineChangeIndexes,
                codesBarEntry.ValueStartIndex,
                Validation.ValueType.ListOfStrings);

        internal static ValidationStructuredEntry StructuredEntryWithLowerCaseValue =>
            new(filename,
            codepageEntry.Key,
            "utf-8",
            codepageEntry.ValueStartIndex,
            codepageEntry.LineChangeIndexes,
            codepageEntry.ValueStartIndex,
            Validation.ValueType.String);
    }
}
