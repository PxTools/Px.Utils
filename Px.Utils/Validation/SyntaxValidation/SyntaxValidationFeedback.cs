using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation.SyntaxValidation
{
    public class ValidationFeedbackMultipleEntriesOnLine : ValidationFeedback
    {
        public override ValidationFeedbackLevel Level { get; set; } = ValidationFeedbackLevel.Warning;
        public override string Rule { get; set; } = "There are multiple entries on this line.";
    }
}
