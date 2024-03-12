using PxUtils.PxFile;
using PxUtils.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Px.Utils.Validation.SyntaxValidation
{
    internal class StreamValidationEntry : ValidationEntry
    {
        public char CurrentCharacter { get; }
        public char NextCharacter { get; }
        public PxFileSyntaxConf SyntaxConf { get; }

        public StreamValidationEntry(int line, int character, string file, char currentCharacter, char nextCharacter, PxFileSyntaxConf syntaxConf) : base(line, character, file)
        {
            CurrentCharacter = currentCharacter;
            NextCharacter = nextCharacter;
            SyntaxConf = syntaxConf;
        }
    }
}
