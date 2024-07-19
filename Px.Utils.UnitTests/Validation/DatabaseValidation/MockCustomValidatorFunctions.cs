using Px.Utils.Validation;
using Px.Utils.Validation.DatabaseValidation;

namespace Px.Utils.UnitTests.Validation.DatabaseValidation
{
    internal class MockCustomDatabaseValidator : IDatabaseValidator
    {
        public ValidationFeedbackItem? Validate(DatabaseValidationItem item)
        {
            return new ValidationFeedbackItem(
                new ValidationObject("test", 0, []),
                new ValidationFeedback(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.AliasFileMissing, 0, 0, "Test error"));
        }
    }
}
