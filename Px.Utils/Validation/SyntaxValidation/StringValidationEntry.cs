using PxUtils.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Px.Utils.Validation.SyntaxValidation
{
    public class StringValidationEntry : ValidationEntry
    {
        public string EntryString { get; set; }

        public StringValidationEntry(int line, int character, string file, string entry) : base(line, character, file)
        {
            EntryString = entry;
        }
    }
}
