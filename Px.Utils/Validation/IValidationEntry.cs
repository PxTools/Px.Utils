namespace PxUtils.Validation
{
    public interface IValidationEntry
    {
        public int Line { get; }
        public int Character { get; }
        public string File { get; }
    }
}
