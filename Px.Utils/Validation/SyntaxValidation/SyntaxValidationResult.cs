using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation.SyntaxValidation
{
    public class SyntaxValidationResult(ValidationReport report, List<StructuredValidationEntry> structuredEntries)
    {
        public ValidationReport Report { get; } = report;
        public List<StructuredValidationEntry> StructuredEntries { get; } = structuredEntries;
    }
}
