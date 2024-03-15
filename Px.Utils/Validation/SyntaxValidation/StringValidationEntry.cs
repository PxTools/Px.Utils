using PxUtils.PxFile;

namespace PxUtils.Validation.SyntaxValidation
{
    public class StringValidationEntry(int line, int character, string file, string entry, PxFileSyntaxConf syntaxConf, int entryIndex) : IValidationEntry
    {
        public string EntryString { get; } = entry;
        public PxFileSyntaxConf SyntaxConf { get; } = syntaxConf;
        public int EntryIndex { get; } = entryIndex;

        public int Line { get; } = line;
        public int Character { get; } = character;
        public string File { get; } = file;
    }
}
