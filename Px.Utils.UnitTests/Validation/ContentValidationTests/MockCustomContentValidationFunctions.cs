using Px.Utils.Validation;
using Px.Utils.Validation.ContentValidation;
using Px.Utils.Validation.SyntaxValidation;

namespace Px.Utils.UnitTests.Validation.ContentValidationTests
{
    internal sealed class MockCustomContentValidationFunctions : CustomContentValidationFunctions
    {
        internal static ValidationFeedback? MockFindKeywordFunction(ValidationStructuredEntry[] entries, ContentValidator validator)
        {
            return null;
        }

        internal static ValidationFeedback? MockEntryFunction(ValidationStructuredEntry entry, ContentValidator validator)
        {
            return null;
        }

        public MockCustomContentValidationFunctions() : base(
            [MockFindKeywordFunction],
            [MockEntryFunction])
        { }
    }
}
