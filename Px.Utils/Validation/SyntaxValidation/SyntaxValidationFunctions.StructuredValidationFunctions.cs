using Px.Utils.PxFile;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Px.Utils.Validation.SyntaxValidation
{
    public delegate KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? StructuredValidationFunction(ValidationStructuredEntry validationObject, PxFileSyntaxConf syntaxConf);

    /// <summary>
    /// Contains the default validation functions for syntax validation structured entries.
    /// </summary>
    public partial class SyntaxValidationFunctions
    {
        public List<StructuredValidationFunction> DefaultStructuredValidationFunctions { get; } =
        [
            KeywordContainsIllegalCharacters,
            KeywordDoesntStartWithALetter,
            IllegalCharactersInLanguageParameter,
            IllegalCharactersInSpecifierParts,
            IncompliantLanguage,
            KeywordContainsUnderscore,
            KeywordIsNotInUpperCase,
            KeywordIsExcessivelyLong
        ];

        private static readonly TimeSpan regexTimeout = TimeSpan.FromMilliseconds(50);
        private static readonly Regex keywordIllegalSymbolsRegex = new(@"[^a-zA-Z0-9_-]", RegexOptions.Compiled, regexTimeout);

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> specifier contains illegal characters, a new feedback is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the specifier contains illegal characters, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? KeywordContainsIllegalCharacters(ValidationStructuredEntry validationStructuredEntry, PxFileSyntaxConf syntaxConf)
        {
            string keyword = validationStructuredEntry.Key.Keyword;
            // Missing specifier is catched earlier
            if (keyword.Length == 0)
            {
                return null;
            }

            // Find all illegal symbols in specifier
            try
            {
                MatchCollection matchesKeyword = keywordIllegalSymbolsRegex.Matches(keyword, 0);
                IEnumerable<string> illegalSymbolsInKeyWord = matchesKeyword.Cast<Match>().Select(m => m.Value).Distinct();
                if (illegalSymbolsInKeyWord.Any())
                {
                    KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                        validationStructuredEntry.KeyStartLineIndex,
                        keyword.IndexOf(illegalSymbolsInKeyWord.First(), StringComparison.Ordinal),
                        validationStructuredEntry.LineChangeIndexes);

                    return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                        new(ValidationFeedbackLevel.Error,
                        ValidationFeedbackRule.IllegalCharactersInKeyword),
                        new(validationStructuredEntry.File,
                        feedbackIndexes.Key,
                        feedbackIndexes.Value,
                        string.Join(", ", illegalSymbolsInKeyWord))
                    );
                }
            }
            catch (RegexMatchTimeoutException)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    0,
                    validationStructuredEntry.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.RegexTimeout),
                    new(validationStructuredEntry.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value)
                );
            }

            return null;
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> keyword doesn't start with a letter, a new feedback is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the specifier doesn't start with a letter, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? KeywordDoesntStartWithALetter(ValidationStructuredEntry validationStructuredEntry, PxFileSyntaxConf syntaxConf)
        {
            string keyword = validationStructuredEntry.Key.Keyword;

            // Check if keyword starts with a letter
            if (!char.IsLetter(keyword[0]))
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    0,
                    validationStructuredEntry.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.KeywordDoesntStartWithALetter),
                    new(validationStructuredEntry.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value)
                );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> language parameter is not following a valid format, a new feedback is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the language parameter is not following a valid format, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? IllegalCharactersInLanguageParameter(
            ValidationStructuredEntry validationStructuredEntry,
            PxFileSyntaxConf syntaxConf)
        {
            // Running this validation is relevant only for objects with a language parameter
            if (validationStructuredEntry.Key.Language is null)
            {
                return null;
            }

            string lang = validationStructuredEntry.Key.Language;

            // Find illegal characters from language parameter string
            char[] illegalCharacters = [
                syntaxConf.Symbols.Key.LangParamStart,
                syntaxConf.Symbols.Key.LangParamEnd,
                syntaxConf.Symbols.Key.StringDelimeter,
                syntaxConf.Symbols.EntrySeparator,
                syntaxConf.Symbols.KeywordSeparator,
                CharacterConstants.CHARSPACE,
                CharacterConstants.CHARCARRIAGERETURN,
                CharacterConstants.CHARLINEFEED,
                CharacterConstants.CHARHORIZONTALTAB
            ];

            IEnumerable<char> foundIllegalCharacters = lang.Where(c => illegalCharacters.Contains(c));
            if (foundIllegalCharacters.Any())
            {
                int indexOfFirstIllegalCharacter = lang.IndexOf(foundIllegalCharacters.First());

                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    validationStructuredEntry.Key.Keyword.Length + indexOfFirstIllegalCharacter + 1,
                    validationStructuredEntry.LineChangeIndexes);

                string foundSymbols = string.Join(", ", foundIllegalCharacters);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.IllegalCharactersInLanguageSection),
                    new(validationStructuredEntry.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value,
                    foundSymbols)
                );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> specifier parameter is not following a valid format, a new feedback is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the specifier parameter is not following a valid format, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? IllegalCharactersInSpecifierParts(ValidationStructuredEntry validationStructuredEntry, PxFileSyntaxConf syntaxConf)
        {
            // Running this validation is relevant only for objects with a specifier
            if (validationStructuredEntry.Key.FirstSpecifier is null)
            {
                return null;
            }

            char[] illegalCharacters = [syntaxConf.Symbols.EntrySeparator, syntaxConf.Symbols.Key.StringDelimeter];
            IEnumerable<char> illegalcharactersInFirstSpecifier = validationStructuredEntry.Key.FirstSpecifier.Where(c => illegalCharacters.Contains(c));
            IEnumerable<char> illegalcharactersInSecondSpecifier = [];
            if (validationStructuredEntry.Key.SecondSpecifier is not null)
            {
                illegalcharactersInSecondSpecifier = validationStructuredEntry.Key.SecondSpecifier.Where(c => illegalCharacters.Contains(c));
            }

            if (illegalcharactersInFirstSpecifier.Any() || illegalcharactersInSecondSpecifier.Any())
            {
                int languageSectionLength = validationStructuredEntry.Key.Language is not null ? validationStructuredEntry.Key.Language.Length + 3 : 1;

                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    validationStructuredEntry.Key.Keyword.Length + languageSectionLength,
                    validationStructuredEntry.LineChangeIndexes);

                char[] characters = [.. illegalcharactersInFirstSpecifier, .. illegalcharactersInSecondSpecifier];

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Error,
                    ValidationFeedbackRule.IllegalCharactersInSpecifierPart),
                    new(validationStructuredEntry.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value,
                    string.Join(", ", characters))
                );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> language parameter is not compliant with ISO 639 or BCP 47, a new feedback is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the language parameter is not compliant with ISO 639 or BCP 47, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? IncompliantLanguage(ValidationStructuredEntry validationStructuredEntry, PxFileSyntaxConf syntaxConf)
        {
            // Running this validation is relevant only for objects with a language
            if (validationStructuredEntry.Key.Language is null)
            {
                return null;
            }

            string lang = validationStructuredEntry.Key.Language;

            bool iso639OrMsLcidCompliant = CultureInfo.GetCultures(CultureTypes.AllCultures).ToList().Exists(c => c.Name == lang || c.ThreeLetterISOLanguageName == lang);
            bool bcp47Compliant = Bcp47Codes.Codes.Contains(lang);

            if (iso639OrMsLcidCompliant || bcp47Compliant)
            {
                return null;
            }
            else
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    validationStructuredEntry.Key.Keyword.Length + 1,
                    validationStructuredEntry.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.IncompliantLanguage),
                    new(validationStructuredEntry.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value)
                );
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> specifier contains unrecommended characters, a new feedback is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> to validate.</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the specifier contains unrecommended characters, null otherwise.</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? KeywordContainsUnderscore(ValidationStructuredEntry validationStructuredEntry, PxFileSyntaxConf syntaxConf)
        {
            int underscoreIndex = validationStructuredEntry.Key.Keyword.IndexOf('_');
            if (underscoreIndex != -1)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    underscoreIndex,
                    validationStructuredEntry.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.KeywordContainsUnderscore),
                    new(validationStructuredEntry.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value)
                );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> specifier is not in upper case, a new feedback is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> to validate</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the specifier is not in upper case, otherwise null</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? KeywordIsNotInUpperCase(ValidationStructuredEntry validationStructuredEntry, PxFileSyntaxConf syntaxConf)
        {
            string keyword = validationStructuredEntry.Key.Keyword;
            string uppercaseKeyword = keyword.ToUpper(CultureInfo.InvariantCulture);

            if (uppercaseKeyword != keyword)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    0,
                    validationStructuredEntry.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.KeywordIsNotInUpperCase),
                    new(validationStructuredEntry.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value,
                    validationStructuredEntry.Key.Keyword)
                );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// If the <see cref="ValidationStructuredEntry"/> specifier is excessively long, a new feedback is returned.
        /// </summary>
        /// <param name="validationStructuredEntry">The <see cref="ValidationStructuredEntry"/> object to validate</param>
        /// <param name="syntaxConf">The syntax configuration for the PX file.</param>
        /// <returns>A validation feedback key value pair if the specifier is excessively long, otherwise null</returns>
        public static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? KeywordIsExcessivelyLong(ValidationStructuredEntry validationStructuredEntry, PxFileSyntaxConf syntaxConf)
        {
            const int keywordLengthRecommendedLimit = 20;
            string keyword = validationStructuredEntry.Key.Keyword;

            if (keyword.Length > keywordLengthRecommendedLimit)
            {
                KeyValuePair<int, int> feedbackIndexes = SyntaxValidationUtilityMethods.GetLineAndCharacterIndex(
                    validationStructuredEntry.KeyStartLineIndex,
                    keyword.Length,
                    validationStructuredEntry.LineChangeIndexes);

                return new KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>(
                    new(ValidationFeedbackLevel.Warning,
                    ValidationFeedbackRule.KeywordExcessivelyLong),
                    new(validationStructuredEntry.File,
                    feedbackIndexes.Key,
                    feedbackIndexes.Value,
                    validationStructuredEntry.Key.Keyword)
                );
            }
            else
            {
                return null;
            }
        }
    }
}
