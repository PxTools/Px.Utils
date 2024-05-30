﻿using Px.Utils.Validation.SyntaxValidation;
using Px.Utils.UnitTests.SyntaxValidationTests.Fixtures;
using System.Text;
using Px.Utils.Validation;
using Px.Utils.PxFile.Metadata;

namespace Px.Utils.UnitTests.SyntaxValidationTests
{
    [TestClass]
    public class StreamSyntaxValidationAsyncTests
    {
        private readonly string filename = "foo";
        private readonly List<ValidationFeedbackItem> feedback = [];

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsyncCalledWithMinumalUTF8ReturnsValidResult()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.MINIMAL_UTF8_N);
            using Stream stream = new MemoryStream(data);
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            SyntaxValidator validator = new(stream, encoding, filename);

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = (SyntaxValidationResult)await validator.ValidateAsync();
            Assert.AreEqual(8, result.Result.Count);
            Assert.AreEqual(0, feedback.Count);
        }

        [TestMethod]
        public async Task ValidatePxFileSyntaxAsyncCalledWithSpecifiersReturnsResultWithRightStructure()
        {
            // Arrange
            byte[] data = Encoding.UTF8.GetBytes(SyntaxValidationFixtures.UTF8_N_WITH_SPECIFIERS);
            using Stream stream = new MemoryStream(data);
            Encoding encoding = await PxFileMetadataReader.GetEncodingAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            SyntaxValidator validator = new(stream, encoding, filename);

            // Assert
            Assert.IsNotNull(encoding, "Encoding should not be null");

            // Act
            SyntaxValidationResult result = (SyntaxValidationResult)await validator.ValidateAsync();
            Assert.AreEqual(10, result.Result.Count);
            Assert.AreEqual("YES", result.Result[8].Value);
            Assert.AreEqual("NO", result.Result[9].Value);
            Assert.AreEqual("fi", result.Result[8].Key.Language);
            Assert.AreEqual("fi", result.Result[9].Key.Language);
            Assert.AreEqual("first_specifier", result.Result[8].Key.FirstSpecifier);
            Assert.AreEqual("first_specifier", result.Result[9].Key.FirstSpecifier);
            Assert.AreEqual("second_specifier", result.Result[8].Key.SecondSpecifier);
        }
    }
}
