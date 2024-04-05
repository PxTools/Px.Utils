
using System.Text;

namespace PxUtils.Validation.ContentValidation
{
    /// <summary>
    /// Object that stores info about the metadata content of a Px file during validation process
    /// </summary>
    /// <param name="filename">Name of the Px file</param>
    /// <param name="encoding">Encoding format of the Px file</param>
    public class ContentValidationInfo(string filename, Encoding encoding)
    {
        public string Filename { get; } = filename;
        public Encoding Encoding { get; set; } = encoding;
        public string? DefaultLanguage { get; set; }
        public string[]? AvailableLanguages { get; set; }
        /// <summary>
        /// Dictionary that define content dimension names for available languages. Key represents the language, while the value is the dimension name.
        /// </summary>
        public Dictionary<string, string>? ContentDimensionNames { get; set; }
        /// <summary>
        /// Dictionary that define stub dimension names for available languages. Key represents the language, while the value is the dimension name.
        /// </summary>
        public Dictionary<string, string[]>? StubDimensionNames { get; set; }
        /// <summary>
        /// Dictionary that define heading dimension names for available languages. Key represents the language, while the value is the dimension name.
        /// </summary>
        public Dictionary<string, string[]>? HeadingDimensionNames { get; set; }
        /// <summary>
        /// Dictionary of key value pairs that define names for dimension values. Key is a key value pair of language and dimension name, while the value is an array of dimension value names.
        /// </summary>
        public Dictionary<KeyValuePair<string, string>, string[]>? DimensionValueNames { get; set; }
    }
}
