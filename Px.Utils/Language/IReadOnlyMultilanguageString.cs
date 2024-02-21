namespace PxUtils.Language
{
    public interface IReadOnlyMultilanguageString : IEquatable<IReadOnlyMultilanguageString>
    {
        public string this[string key] { get; }

        public IEnumerable<string> Languages { get; }
    }
}
