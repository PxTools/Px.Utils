using PxUtils.PxFile;
using PxUtils.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation.SyntaxValidation
{
    public class StringValidationEntry(int line, int character, string file, string entry, PxFileSyntaxConf syntaxConf, int entryIndex) : ValidationEntry(line, character, file)
    {
        public string EntryString { get; } = entry;
        public PxFileSyntaxConf SyntaxConf { get; } = syntaxConf;
        public int EntryIndex { get; } = entryIndex;
    }
}
