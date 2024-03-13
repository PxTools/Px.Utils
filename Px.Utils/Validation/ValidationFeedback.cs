using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation
{
    public abstract class ValidationFeedback
    {
        public abstract ValidationFeedbackLevel Level { get; }
        public abstract string Rule { get; }
    }
}
