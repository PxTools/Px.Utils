namespace PxUtils.Language
{
    public static class MultilanguageStringExtensions
    {
        /// <summary>
        /// Retrieves the unique value from a multilanguage string.
        /// This method assumes that all language versions of the string are identical.
        /// If the values differ, an exception is thrown.
        /// </summary>
        public static string UniqueValue(this IReadOnlyMultilanguageString multilanguageString)
        {
            string uniqueValue = multilanguageString[multilanguageString.Languages.First()];
            foreach (var language in multilanguageString.Languages)
            {
                if (multilanguageString[language] != uniqueValue)
                {
                    throw new InvalidOperationException("The multilanguage string has different values for different languages");
                }
            }
            return uniqueValue;
        }
    }
}
