namespace PxUtils.Exceptions
{
    public class TranslationNotFoundException(string language) : Exception()
    {
        public string Language { get; } = language;
        public override string Message => $"No translation could be found for language {Language}.";
    }
}
