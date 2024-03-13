using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation
{
    public class ValidationEntry(int line, int character, string file)
    {
        public int Line { get; } = line;
        public int Character { get; } = character;
        public string File { get; } = file;
    }
}
