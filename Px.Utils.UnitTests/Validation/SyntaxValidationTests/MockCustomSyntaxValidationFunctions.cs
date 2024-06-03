using Px.Utils.Validation.SyntaxValidation;
using Px.Utils.Validation;
using Px.Utils.PxFile;

namespace Px.Utils.UnitTests.Validation.SyntaxValidationTests
{
    internal class MockCustomSyntaxValidationFunctions : CustomSyntaxValidationFunctions
    {
        internal static ValidationFeedbackItem? MockEntryValidationFunction(ValidationEntry validationEntry, PxFileSyntaxConf syntaxConf)
        {
            return null;
        }

        internal static ValidationFeedbackItem? MockKeyValuePairValidationFunction(ValidationKeyValuePair validationEntry, PxFileSyntaxConf syntaxConf)
        {
            return null;
        }

        internal static ValidationFeedbackItem? MockStructuredValidationFunction(ValidationStructuredEntry validationEntry, PxFileSyntaxConf syntaxConf)
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
