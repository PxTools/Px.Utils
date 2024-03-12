using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.PxFile.Validation
{
    public class ValidationReport
    {
        public ValidationTarget ReportType { get; set; }
        public List<ValidationFeedbackItem>? FeedbackItems { get; set; }
    }
}
