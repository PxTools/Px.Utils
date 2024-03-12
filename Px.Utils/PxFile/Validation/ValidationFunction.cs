using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.PxFile.Validation
{
    public interface IValidationFunction
    {
        ValidationFeedbackLevel Level { get; }
    }
}
