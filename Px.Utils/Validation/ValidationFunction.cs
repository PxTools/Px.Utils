using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation
{
    public interface IValidationFunction
    {
        public bool IsRelevant(ValidationEntry entry);
        public ValidationFeedbackItem? Validate(ValidationEntry entry);
    }
}
