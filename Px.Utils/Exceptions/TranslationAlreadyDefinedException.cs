namespace Px.Utils.Exceptions
{
    public class TranslationAlreadyDefinedException(string language) : Exception()
    {
        public string Language { get; } = language;
        public override string Message => $"Translation for language {Language} has already been defined.";
    }
}
