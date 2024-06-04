namespace Px.Utils.Language
{
    internal struct PxFileLanguages(string defaultLanguage, IReadOnlyList<string> availableLanguages)
    {
        public string DefaultLanguage { get; set; } = defaultLanguage;
        public IReadOnlyList<string> AvailableLanguages { get; set; } = availableLanguages;
    }
}
