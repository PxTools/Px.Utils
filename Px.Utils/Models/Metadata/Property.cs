using PxUtils.Language;

namespace PxUtils.Models.Metadata
{
    /// <summary>
    /// Represents a property in a Px-file. A property can have a single string value or a multilanguage string value.
    /// The class provides methods to get the property value as a single string or as a multilanguage string, 
    /// and to check if the property value can be represented in these formats.
    /// </summary>
    public class Property
    {
        /// <summary>
        /// The keyword of the property as it appears in the Px-file
        /// </summary>
        public string KeyWord { get; }

        /// <summary>
        /// True if the property can be represented as a single string
        /// </summary>
        public bool CanGetStringValue { get; }

        /// <summary>
        /// True if the property can be represented as a multilanguage string
        /// </summary>
        public bool CanGetMultilanguageValue { get; }

        private MultilanguageString Entries { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> class with a single string value.
        /// </summary>
        /// <param name="keyWord">The keyword of the property as it appears in the Px-file.</param>
        /// <param name="value">The value of the property.</param>
        public Property(string keyWord, string value)
        {
            Entries = new MultilanguageString("none", value);
            KeyWord = keyWord;
            CanGetStringValue = true;
            CanGetMultilanguageValue = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> class.
        /// </summary>
        /// <param name="keyWord">The keyword of the property as it appears in the Px-file.</param>
        /// <param name="entries">A <see cref="MultilanguageString"/> object that contains the multilingual entries for the property.</param>
        public Property(string keyWord, MultilanguageString entries)
        {
            Entries = entries;
            KeyWord = keyWord;
            CanGetStringValue = entries.Languages
                .Select(l => entries[l])
                .Distinct()
                .Count() == 1;
            CanGetMultilanguageValue = true;
        }

        /// <summary>
        /// Retrieves the raw value of the property as a single string.
        /// This method does not perform any trimming or other modifications to the value.
        /// </summary>
        /// <returns>The value of the property as a single string.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the property value cannot be represented as a single string.</exception>
        public string GetRawValueString()
        {
            if (!CanGetStringValue) throw new InvalidOperationException("Property value can not be represented as a single string");
            else return Entries.UniformValue();
        }

        /// <summary>
        /// Attempts to retrieve the raw value of the property as a single string.
        /// This method does not perform any trimming or other modifications to the value.
        /// </summary>
        /// <param name="value">When this method returns, contains the value of the property as a single string, if the conversion succeeded, or null if the conversion failed.</param>
        /// <returns>true if the property value can be represented as a single string; otherwise, false.</returns>
        public bool TryGetRawValueString(out string? value)
        {
            if (!CanGetStringValue)
            {
                value = null;
                return false;
            }
            else
            {
                value = Entries.UniformValue();
                return true;
            }
        }

        /// <summary>
        /// Retrieves the raw value of the property as a multilanguage string.
        /// This method does not perform any trimming or other modifications to the value.
        /// </summary>
        /// <returns>A <see cref="MultilanguageString"/> object that represents the value of the property.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the property value cannot be represented as a multilanguage string.</exception>
        public MultilanguageString GetRawValueMultiLanguageString()
        {
            if (!CanGetMultilanguageValue) throw new InvalidOperationException("Value can not be represented as a multilanguage string");
            else return new(Entries);
        }

        /// <summary>
        /// Attempts to retrieve the raw value of the property as a multilanguage string.
        /// This method does not perform any trimming or other modifications to the value.
        /// </summary>
        /// <param name="value">When this method returns, contains a <see cref="MultilanguageString"/>
        /// object that represents the value of the property, if the conversion succeeded, or null if the conversion failed.</param>
        /// <returns>true if the property value can be represented as a multilanguage string; otherwise, false.</returns>
        public bool TryGetRawValueMultilanguageString(out MultilanguageString? value)
        {
            if(!CanGetMultilanguageValue)
            {
                value = null;
                return false;
            }
            else
            {
                value = new(Entries);
                return true;
            }
        }
    }
}
