using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation
{
    public class ValidationFeedbackItem(ValidationEntry entry, ValidationFeedback feedback)
    {
        public string File { get; set; } = entry.File;
        public long Line { get; set; } = entry.Line;
        public int Character { get; set; } = entry.Character;
        public ValidationFeedback Feedback { get; set; } = feedback;
    }
}
