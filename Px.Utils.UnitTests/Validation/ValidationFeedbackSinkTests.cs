using Px.Utils.Validation;

namespace Px.Utils.UnitTests.Validation
{
    [TestClass]
    public class ValidationFeedbackSinkTests
    {
        [TestMethod]
        public void ReportMatchingFeedbackBeyondLimitRetainsLimitAndAnnotatesFinalItem()
        {
            ValidationFeedbackSink sink = new(new ValidationOptions { MaxFeedbackItemsPerSignature = 2 });
            ValidationFeedbackKey key = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidChar);

            sink.Report(key, new ValidationFeedbackValue("file.px", 1));
            sink.Report(key, new ValidationFeedbackValue("file.px", 2, additionalInfo: "Original information."));
            sink.Report(key, new ValidationFeedbackValue("file.px", 3));
            sink.Report(key, new ValidationFeedbackValue("file.px", 4));

            ValidationFeedback feedback = sink.ToFeedback();
            List<ValidationFeedbackValue> values = feedback[key];

            Assert.AreEqual(2, values.Count);
            StringAssert.Contains(values[1].AdditionalInfo, "Original information.");
            StringAssert.Contains(values[1].AdditionalInfo, "Feedback limit of 2 instances");
        }

        [TestMethod]
        public void ReportUnlimitedFeedbackRetainsAllItems()
        {
            ValidationFeedbackSink sink = new(ValidationOptions.Unlimited);
            ValidationFeedbackKey key = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidChar);

            sink.Report(key, new ValidationFeedbackValue("file.px", 1));
            sink.Report(key, new ValidationFeedbackValue("file.px", 2));
            sink.Report(key, new ValidationFeedbackValue("file.px", 3));

            Assert.AreEqual(3, sink.ToFeedback()[key].Count);
        }

        [TestMethod]
        public void ReportDifferentSignaturesRetainsSeparateLimits()
        {
            ValidationFeedbackSink sink = new(new ValidationOptions { MaxFeedbackItemsPerSignature = 1 });
            ValidationFeedbackKey errorKey = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidChar);
            ValidationFeedbackKey warningKey = new(ValidationFeedbackLevel.Warning, ValidationFeedbackRule.DataValidationFeedbackInvalidChar);

            sink.Report(errorKey, new ValidationFeedbackValue("first.px"));
            sink.Report(errorKey, new ValidationFeedbackValue("first.px"));
            sink.Report(errorKey, new ValidationFeedbackValue("second.px"));
            sink.Report(warningKey, new ValidationFeedbackValue("first.px"));

            ValidationFeedback feedback = sink.ToFeedback();

            Assert.AreEqual(2, feedback[errorKey].Select(value => value.Filename).Distinct(StringComparer.Ordinal).Count());
            Assert.AreEqual(1, feedback[warningKey].Count);
        }

        [TestMethod]
        public void ValidationOptionsNonPositiveLimitThrows()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ValidationFeedbackSink(new ValidationOptions { MaxFeedbackItemsPerSignature = 0 }));
        }
    }
}
