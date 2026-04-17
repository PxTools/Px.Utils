using System.Linq;
using System.Text;
using Px.Utils.UnitTests.Validation.Fixtures;
using Px.Utils.Validation;
using Px.Utils.Validation.DataValidation;

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
        [DataRow(DataStreamContents.NO_DATA, 2, 6, false)]
        [DataRow(DataStreamContents.DATA_ON_SINGLE_ROW, 2, 6, false)]
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
        [DataRow(DataStreamContents.SIMPLE_VALID_DATA, 0, 0)]
        [DataRow(DataStreamContents.SIMPLE_VALID_DATA_WITH_INCONSISTENT_LINEBREAKS, 0, 0)]
        [DataRow(DataStreamContents.SIMPLE_VALID_DATA_WITHOUT_MISISNG_CODE_DELIMETERS, 0, 0)]
        [DataRow(DataStreamContents.SIMPLE_INVALID_DATA, 7, 12)]
        [DataRow(DataStreamContents.NO_DATA, 2, 6, false)]
        [DataRow(DataStreamContents.DATA_ON_SINGLE_ROW, 2, 6, false)]
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