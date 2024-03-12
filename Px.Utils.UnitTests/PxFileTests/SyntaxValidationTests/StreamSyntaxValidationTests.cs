using PxUtils.PxFile.Validation;
using PxUtils.UnitTests.PxFileTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.UnitTests.PxFileTests.SyntaxValidationTests
{
    [TestClass]
    public class StreamSyntaxValidationTests
    {
        [TestMethod]
        public void ValidateStreamSyntax_CalledWith_MINIMAL_UTF8_Returns_Valid_Report()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            ValidationReport result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(ValidationTarget.Syntax, result.ReportType);
            Assert.AreEqual(0, result.FeedbackItems?.Count);
        }

        [TestMethod]
        public void ValidateStreamSyntax_CalledWith_MINIMAL_UTF8_N_WITH_MULTIPLE_ENTRIES_IN_SINGLE_LINE_Returns_Report_With_Warnings()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(MinimalPx.MINIMAL_UTF8_N_WITH_MULTIPLE_ENTRIES_IN_SINGLE_LINE);
            using Stream stream = new MemoryStream(data);
            stream.Seek(0, SeekOrigin.Begin);
            string filename = "foo";

            // Act
            ValidationReport result = SyntaxValidation.ValidatePxFileSyntax(stream, filename);

            Assert.AreEqual(2, result.FeedbackItems?.Count);
        }
    }
}
