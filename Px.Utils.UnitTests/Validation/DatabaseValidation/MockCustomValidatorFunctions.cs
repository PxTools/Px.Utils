using Px.Utils.Validation;
using Px.Utils.Validation.DatabaseValidation;

namespace Px.Utils.UnitTests.Validation.DatabaseValidation
{
    internal sealed class MockCustomDatabaseValidator : IDatabaseValidator
    {
        public KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? Validate(DatabaseValidationItem item)
        {
            return new (
                new (ValidationFeedbackLevel.Warning, ValidationFeedbackRule.AliasFileMissing),
                new ("test", 0, 0, "Test error"));
        }
    }
}
