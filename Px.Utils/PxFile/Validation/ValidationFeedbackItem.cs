using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.PxFile.Validation
{
    public class ValidationFeedbackItem(string file, long line, int character, ValidationFeedback feedback)
    {
        public string File { get; set; } = file;
        public long Line { get; set; } = line;
        public int Character { get; set; } = character;
        public ValidationFeedback Feedback { get; set; } = feedback;
    }
}
