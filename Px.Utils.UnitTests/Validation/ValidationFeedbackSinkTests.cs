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

            Assert.HasCount(2, values);
            Assert.Contains("Original information.", values[1].AdditionalInfo);
            Assert.Contains("Feedback limit of 2 instances", values[1].AdditionalInfo);
        }

        [TestMethod]
        public void ReportUnlimitedFeedbackRetainsAllItems()
        {
            ValidationFeedbackSink sink = new(ValidationOptions.Unlimited);
            ValidationFeedbackKey key = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidChar);

            for (int line = 1; line <= 101; line++)
            {
                sink.Report(key, new ValidationFeedbackValue("file.px", line));
            }

            List<ValidationFeedbackValue> values = sink.ToFeedback()[key];
            Assert.HasCount(101, values);
            Assert.DoesNotContain(value => value.AdditionalInfo?.Contains("Feedback limit", StringComparison.Ordinal) == true, values);
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
            Assert.HasCount(1, feedback[warningKey]);
        }

        [TestMethod]
        public void ValidationOptionsNonPositiveLimitThrows()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ValidationFeedbackSink(new ValidationOptions { MaxFeedbackItemsPerSignature = 0 }));
        }
    }
}
