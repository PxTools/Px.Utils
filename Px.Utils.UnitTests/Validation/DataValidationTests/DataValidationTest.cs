using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Microsoft.Win32.SafeHandles;
using Px.Utils.UnitTests.Validation.Fixtures;
using PxUtils.PxFile;
using PxUtils.Validation;
using PxUtils.Validation.DataValidation;

namespace Px.Utils.UnitTests.Validation.DataValidationTests
{
    [TestClass]
    public class DataValidationTest
    {

        [TestMethod]
        public void Tokenize()
        {
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_VALID_DATA));
            stream.Seek(6,0);
        
            IEnumerable<Token> tokens = DataValidation.Tokenize(stream, PxFileSyntaxConf.Default, Encoding.UTF8);

            int i = 0;
            foreach (Token token in tokens)
            {
                Logger.LogMessage($"token: {token.Type}, value: {token.Value}, line: {token.LineNumber}, pos: {token.CharPosition}");
                Assert.AreEqual(DataStreamContents.EXPECTED_SIMPLE_VALID_DATA_TOKENS[i++], token);
            }
        }

        [TestMethod]
        public async Task TokenizeAsync()
        {
            await using MemoryStream stream = new(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_VALID_DATA));
            stream.Seek(6,0);
        
            IAsyncEnumerable<Token> tokens = DataValidation.TokenizeAsync(stream, PxFileSyntaxConf.Default, Encoding.UTF8);

            int i = 0;
            await foreach (Token token in tokens)
            {
                Logger.LogMessage($"token: {token.Type}, value: {token.Value}, line: {token.LineNumber}, pos: {token.CharPosition}");
                Assert.AreEqual(DataStreamContents.EXPECTED_SIMPLE_VALID_DATA_TOKENS[i++], token);
            }
        }
    
        [TestMethod]
        public void TestValidateWithoutErrors()
        {
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_VALID_DATA));
            stream.Seek(6, 0);

            IEnumerable<ValidationFeedback> validationFeedbacks =
                DataValidation.Validate(stream, 5, 4, 1, Encoding.UTF8, PxFileSyntaxConf.Default);


            Assert.AreEqual(0, validationFeedbacks.Count());
            foreach (ValidationFeedback validationFeedback in validationFeedbacks)
            {
                Logger.LogMessage($"Line {validationFeedback.Line}, Char {validationFeedback.Character}: " 
                                  + $"{validationFeedback.Rule} {validationFeedback.AdditionalInfo}");
            }
        }

        [TestMethod]
        public async Task TestValidateAsyncWithoutErrors()
        {
            await using MemoryStream stream = new(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_VALID_DATA));
            stream.Seek(6, 0);

            IEnumerable<ValidationFeedback> validationFeedbacks =
                await DataValidation.ValidateAsync(stream, 5, 4, 1, Encoding.UTF8, PxFileSyntaxConf.Default);

            foreach (ValidationFeedback validationFeedback in validationFeedbacks)
            {
                Logger.LogMessage($"Line {validationFeedback.Line}, Char {validationFeedback.Character}: " 
                                  + $"{validationFeedback.Rule} {validationFeedback.AdditionalInfo}");
            }
            Assert.AreEqual(0, validationFeedbacks.Count());

        }
        [TestMethod]
        public void TestValidateWithErrors()
        {
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_INVALID_DATA));
            stream.Seek(6, 0);

            IEnumerable<ValidationFeedback> validationFeedbacks =
                DataValidation.Validate(stream, 5, 4, 1, Encoding.UTF8, PxFileSyntaxConf.Default);


            Assert.AreEqual(10, validationFeedbacks.Count());
            foreach (ValidationFeedback validationFeedback in validationFeedbacks)
            {
                Logger.LogMessage($"Line {validationFeedback.Line}, Char {validationFeedback.Character}: " 
                                  + $"{validationFeedback.Rule} {validationFeedback.AdditionalInfo}");
            }
        }

        [TestMethod]
        public async Task TestValidateAsyncWithErrors()
        {
            await using MemoryStream stream = new(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_INVALID_DATA));
            stream.Seek(6, 0);

            IEnumerable<ValidationFeedback> validationFeedbacks =
                await DataValidation.ValidateAsync(stream, 5, 4, 1, Encoding.UTF8, PxFileSyntaxConf.Default);

            foreach (ValidationFeedback validationFeedback in validationFeedbacks)
            {
                Logger.LogMessage($"Line {validationFeedback.Line}, Char {validationFeedback.Character}: " 
                                  + $"{validationFeedback.Rule} {validationFeedback.AdditionalInfo}");
            }

            Assert.AreEqual(10, validationFeedbacks.Count());
        }
    
        [Ignore] // Used to check the performance of the tokenization
        [TestMethod]
        public void TokenizeBigFile()
        {
            SafeFileHandle handle = File.OpenHandle(@"Validation\fixtures\statfin_tyonv_pxt_12ts.px");
            using FileStream stream = new(handle, FileAccess.Read, 4096);

            stream.Seek(77670,0);

            IEnumerable<Token> tokens = DataValidation.Tokenize(stream, PxFileSyntaxConf.Default, Encoding.UTF8);

            int i = 0;
            foreach (Token token in tokens)
            {
                i++;
            }
            Logger.LogMessage($"count: {i}");
            Assert.AreEqual(252309823, i);
        }

        [Ignore] // Used to check the performance of the tokenization
        [TestMethod]
        public async Task TokenizeBigFileAsync()
        {
            SafeFileHandle handle = File.OpenHandle(@"Validation\fixtures\statfin_tyonv_pxt_12ts.px");
            await using FileStream stream = new(handle, FileAccess.Read, 4096);

            stream.Seek(77670,0);

            IAsyncEnumerable<Token> tokens = DataValidation.TokenizeAsync(stream, PxFileSyntaxConf.Default, Encoding.UTF8);

            int i = 0;
            await foreach (Token token in tokens)
            {
                i++;
            }
            Logger.LogMessage($"count: {i}");
            Assert.AreEqual(252309823, i);
        }

        [Ignore] // Used to check the performance of the validation 
        [TestMethod]
        public void TestValidatePerformance()
        {
            SafeFileHandle handle = File.OpenHandle(@"Validation\fixtures\statfin_tyonv_pxt_12ts.px");
            using FileStream stream = new(handle, FileAccess.Read, 4096);
            Encoding streamEncoding = Encoding.UTF8;
            stream.Seek(77677,0);

            IEnumerable<ValidationFeedback> validationFeedbacks =
                DataValidation.Validate(stream, 2821, 44712, 791, streamEncoding, PxFileSyntaxConf.Default);

            foreach (ValidationFeedback validationFeedback in validationFeedbacks)
            {
                Logger.LogMessage($"Line {validationFeedback.Line}, Char {validationFeedback.Character}: " 
                                  + $"{validationFeedback.Rule} {validationFeedback.AdditionalInfo}");
            }
            Assert.AreEqual(0, validationFeedbacks.Count());
        }
    
        [Ignore] // Used to check the performance of the validation 
        [TestMethod]
        public async Task TestValidatePerformanceAsync()
        {
            SafeFileHandle handle = File.OpenHandle(@"Validation\fixtures\statfin_tyonv_pxt_12ts.px");
            await using FileStream stream = new(handle, FileAccess.Read, 4096);
            Encoding streamEncoding = Encoding.UTF8;
            stream.Seek(77677,0);

            IEnumerable<ValidationFeedback> validationFeedbacks =
                await DataValidation.ValidateAsync(stream, 2821, 44712, 791, streamEncoding, PxFileSyntaxConf.Default);

            foreach (ValidationFeedback validationFeedback in validationFeedbacks)
            {
                Logger.LogMessage($"Line {validationFeedback.Line}, Char {validationFeedback.Character}: " 
                                  + $"{validationFeedback.Rule} {validationFeedback.AdditionalInfo}");
            }
            Assert.AreEqual(0, validationFeedbacks.Count());
        }
    }
}