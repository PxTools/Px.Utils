namespace Px.Utils.Language
{
    public static class MultilanguageStringExtensions
    {
        /// <summary>
        /// Retrieves the uniform value from a multilanguage string.
        /// This method assumes that all language versions of the string are identical.
        /// If the values differ, an exception is thrown.
        /// </summary>
        public static string UniformValue(this MultilanguageString multilanguageString)
        {
            string uniformValue = multilanguageString[multilanguageString.Languages.First()];
            foreach (var language in multilanguageString.Languages.Skip(1))
            {
                if (multilanguageString[language] != uniformValue)
                {
                    throw new InvalidOperationException("The multilanguage string has different values for different languages");
                }
            }
            return uniformValue;
        }
    }
}
