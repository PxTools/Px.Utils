using PxUtils.PxFile;

namespace PxUtils.Validation.SyntaxValidation
{
    public class KeyValuePairValidationEntry(int line, int character, string file, KeyValuePair<string, string> keyValueEntry, PxFileSyntaxConf syntaxConf) : IValidationEntry
    {
        public KeyValuePair<string, string> KeyValueEntry { get; } = keyValueEntry;
        public PxFileSyntaxConf SyntaxConf { get; } = syntaxConf;

        public int Line { get; } = line;
        public int Character { get; } = character;
        public string File { get; } = file;
    }
}
