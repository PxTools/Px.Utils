using System.Linq;
using System.Text;
using Px.Utils.UnitTests.Validation.Fixtures;
using Px.Utils.Validation;
using Px.Utils.Validation.DataValidation;
using Px.Utils.PxFile;

namespace Px.Utils.UnitTests.Validation.DataValidationTests
{
    [TestClass]
    public class DataValidationTest
    {
        public TestContext TestContext { get; set; }

        private const int DataStartOffset = 6;

        [TestMethod]
        [DataRow(DataStreamContents.SIMPLE_VALID_DATA, 0, 0)]
        [DataRow(DataStreamContents.SIMPLE_VALID_DATA_WITH_INCONSISTENT_LINEBREAKS, 0, 0)]
        [DataRow(DataStreamContents.SIMPLE_VALID_DATA_WITHOUT_MISISNG_CODE_DELIMETERS, 0, 0)]
        [DataRow(DataStreamContents.SIMPLE_INVALID_DATA, 7, 12)]
        [DataRow(DataStreamContents.NO_DATA, 1, 1, false)]
        [DataRow(DataStreamContents.DATA_ON_SINGLE_ROW, 1, 1, false)]
        [DataRow(DataStreamContents.DATA_STARTING_WITH_ENCLOSED_MISSING_VALUE, 0, 0)]
        [DataRow(DataStreamContents.DATA_STARTING_WITH_UNENCLOSED_MISSING_VALUE, 0, 0)]
        [DataRow(DataStreamContents.DATA_STARTING_WITH_NIL_VALUE, 0, 0)]
        [DataRow(DataStreamContents.DATA_STARTING_WITH_UNENCLOSED_NIL_VALUE, 0, 0)]
        public void ValidateDataReturnsExpectedErrorCount(
            string dataContents,
            int expectedUniqueErrorCount,
            int expectedTotalErrorCount,
            bool shouldSeekDataStart = true)
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(dataContents));
            if (shouldSeekDataStart)
            {
                stream.Seek(DataStartOffset, SeekOrigin.Begin);
            }

            DataValidator validator = new(5, 4, 1);

            ValidationFeedback validationFeedbacks = validator.Validate(stream, "foo", Encoding.UTF8).FeedbackItems;
            int actualErrorCount = validationFeedbacks.Values.SelectMany(f => f).Count();

            Assert.HasCount(expectedUniqueErrorCount, validationFeedbacks);
            Assert.AreEqual(expectedTotalErrorCount, actualErrorCount);
        }

        [TestMethod]
        public async Task ValidateAndValidateAsyncCustomDataKeywordAtStreamOriginFindDataSection()
        {
            PxFileConfiguration configuration = PxFileConfiguration.Default;
            configuration.Tokens.KeyWords.Data = "VALUES";
            byte[] data = Encoding.UTF8.GetBytes("TITLE=\"test\";VALUES=1;");
            DataValidator validator = new(1, 0, 0, configuration);

            using Stream synchronousStream = new MemoryStream(data);
            using Stream asynchronousStream = new MemoryStream(data);
            ValidationResult synchronousResult = validator.Validate(synchronousStream, "custom.px", Encoding.UTF8);
            ValidationResult asynchronousResult = await validator.ValidateAsync(asynchronousStream, "custom.px", Encoding.UTF8, cancellationToken: TestContext.CancellationToken);

            ValidationFeedbackKey key = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.StartOfDataSectionNotFound);
            Assert.IsFalse(synchronousResult.FeedbackItems.ContainsKey(key));
            Assert.IsFalse(asynchronousResult.FeedbackItems.ContainsKey(key));
        }

        [TestMethod]
        public void ValidateWithLimitLargeInvalidDataRetainsOnlyConfiguredFeedbackCount()
        {
            const int limit = 3;
            string invalidData = string.Concat(Enumerable.Repeat("! ", 200)) + ";";
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes("DATA=" + invalidData));
            DataValidator validator = new(0, 0, 0);

            ValidationResult result = validator.Validate(stream, "invalid.px", Encoding.UTF8, null, new ValidationOptions { MaxFeedbackItemsPerSignature = limit });

            ValidationFeedbackKey key = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidChar);
            Assert.HasCount(limit, result.FeedbackItems[key]);
            Assert.Contains("Feedback limit of 3 instances", result.FeedbackItems[key][^1].AdditionalInfo!);
        }

        [TestMethod]
        public async Task ValidateDefaultOverloadsLargeInvalidDataRetainDefaultFeedbackCount()
        {
            string invalidData = string.Concat(Enumerable.Repeat("! ", 200)) + ";";
            byte[] data = Encoding.UTF8.GetBytes("DATA=" + invalidData);
            using Stream synchronousStream = new MemoryStream(data);
            using Stream asynchronousStream = new MemoryStream(data);
            DataValidator synchronousValidator = new(0, 0, 0);
            DataValidator asynchronousValidator = new(0, 0, 0);

            ValidationResult synchronousResult = synchronousValidator.Validate(synchronousStream, "invalid.px", Encoding.UTF8);
            ValidationResult asynchronousResult = await asynchronousValidator.ValidateAsync(asynchronousStream, "invalid.px", Encoding.UTF8, cancellationToken: TestContext.CancellationToken);

            ValidationFeedbackKey key = new(ValidationFeedbackLevel.Error, ValidationFeedbackRule.DataValidationFeedbackInvalidChar);
            Assert.HasCount(100, synchronousResult.FeedbackItems[key]);
            Assert.Contains("Feedback limit of 100 instances", synchronousResult.FeedbackItems[key][^1].AdditionalInfo!);
            Assert.HasCount(100, asynchronousResult.FeedbackItems[key]);
            Assert.Contains("Feedback limit of 100 instances", asynchronousResult.FeedbackItems[key][^1].AdditionalInfo!);
        }

        [TestMethod]
        [DataRow(DataStreamContents.SIMPLE_VALID_DATA, 0, 0)]
        [DataRow(DataStreamContents.SIMPLE_VALID_DATA_WITH_INCONSISTENT_LINEBREAKS, 0, 0)]
        [DataRow(DataStreamContents.SIMPLE_VALID_DATA_WITHOUT_MISISNG_CODE_DELIMETERS, 0, 0)]
        [DataRow(DataStreamContents.SIMPLE_INVALID_DATA, 7, 12)]
        [DataRow(DataStreamContents.NO_DATA, 1, 1, false)]
        [DataRow(DataStreamContents.DATA_ON_SINGLE_ROW, 1, 1, false)]
        [DataRow(DataStreamContents.DATA_STARTING_WITH_ENCLOSED_MISSING_VALUE, 0, 0)]
        [DataRow(DataStreamContents.DATA_STARTING_WITH_UNENCLOSED_MISSING_VALUE, 0, 0)]
        [DataRow(DataStreamContents.DATA_STARTING_WITH_NIL_VALUE, 0, 0)]
        [DataRow(DataStreamContents.DATA_STARTING_WITH_UNENCLOSED_NIL_VALUE, 0, 0)]
        public async Task ValidateAsyncDataReturnsExpectedErrorCount(
            string dataContents,
            int expectedUniqueErrorCount,
            int expectedTotalErrorCount,
            bool shouldSeekToDataStart = true)
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(dataContents));
            if (shouldSeekToDataStart)
            {
                stream.Seek(DataStartOffset, SeekOrigin.Begin);
            }

            DataValidator validator = new(5, 4, 1);

            ValidationResult result = await validator.ValidateAsync(stream, "foo", Encoding.UTF8, cancellationToken: TestContext.CancellationToken);
            ValidationFeedback validationFeedbacks = result.FeedbackItems;
            int actualErrorCount = validationFeedbacks.Values.SelectMany(f => f).Count();

            Assert.HasCount(expectedUniqueErrorCount, validationFeedbacks);
            Assert.AreEqual(expectedTotalErrorCount, actualErrorCount);
        }
    }
}