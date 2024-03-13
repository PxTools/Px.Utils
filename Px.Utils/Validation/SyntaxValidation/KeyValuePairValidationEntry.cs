using PxUtils.PxFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation.SyntaxValidation
{
    public class KeyValuePairValidationEntry(int line, int character, string file, KeyValuePair<string, string> keyValueEntry, PxFileSyntaxConf syntaxConf) : ValidationEntry(line, character, file)
    {
        public KeyValuePair<string, string> KeyValueEntry { get; } = keyValueEntry;
        public PxFileSyntaxConf SyntaxConf { get; } = syntaxConf;
    }
}
