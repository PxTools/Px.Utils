using Px.Utils.Validation.SyntaxValidation;
using Px.Utils.Validation;
using Px.Utils.PxFile;

namespace Px.Utils.UnitTests.Validation.SyntaxValidationTests
{
    internal sealed class MockCustomSyntaxValidationFunctions : CustomSyntaxValidationFunctions
    {
        internal static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? MockEntryValidationFunction(ValidationEntry validationEntry, PxFileConfiguration conf)
        {
            return null;
        }

        internal static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? MockKeyValuePairValidationFunction(ValidationKeyValuePair validationEntry, PxFileConfiguration conf)
        {
            return null;
        }

        internal static KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue>? MockStructuredValidationFunction(ValidationStructuredEntry validationEntry, PxFileConfiguration conf)
        {
            return null;
        }

        public MockCustomSyntaxValidationFunctions() : base(
            [MockEntryValidationFunction],
            [MockKeyValuePairValidationFunction],
            [MockStructuredValidationFunction])
        { }
    }
}
