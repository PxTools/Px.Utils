
namespace PxUtils.Validation.ContentValidation
{
    //TODO: Summary - also for properties
    public class ContentValidationInfo(string filename)
    {
        public string Filename { get; } = filename;
        public string? DefaultLanguage { get; set; }
        public string[]? AvailableLanguages { get; set; }
        public Dictionary<string, string>? ContentDimensionEntry { get; set; }
        public Dictionary<string, string[]>? StubDimensions { get; set; }
        public Dictionary<string, string[]>? HeadingDimensions { get; set; }
        public Dictionary<KeyValuePair<string, string>, string[]>? DimensionValues { get; set; } 
    }
}
