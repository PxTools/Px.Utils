using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation
{
    public abstract class ValidationFeedback()
    {
        public abstract ValidationTarget Target { get; set; }
        public abstract ValidationFeedbackLevel Level { get; set; }
        public abstract string Rule { get; set; }
    }

    public class ValidationFeedbackMultipleEntriesOnLine : ValidationFeedback
    {
        public override ValidationTarget Target { get; set; } = ValidationTarget.Syntax;
        public override ValidationFeedbackLevel Level { get; set; } = ValidationFeedbackLevel.Warning;
        public override string Rule { get; set; } = "There are multiple entries on this line.";
    }
}
