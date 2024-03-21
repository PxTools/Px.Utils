using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Px.Utils.UnitTests.Validation.Fixtures;
using PxUtils.PxFile;
using PxUtils.Validation.DataValidation;

namespace Px.Utils.UnitTests.Validation.DataValidationTests;

[TestClass]
public class DataValidationTest
{

    [TestMethod]
    public void Tokenize()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_VALID_DATA));
        stream.Seek(6,0);
        
        var tokens = DataValidation.Tokenize(stream, PxFileSyntaxConf.Default, Encoding.UTF8);

        var i = 0;
        foreach (var token in tokens)
        {
            Logger.LogMessage($"token: {token.Type}, value: {token.Value}, line: {token.LineNumber}, pos: {token.CharPosition}");
            Assert.AreEqual(DataStreamContents.EXPECTED_SIMPLE_VALID_DATA_TOKENS[i++], token);
        }
    }

    [TestMethod]
    public void TestValidateWithoutErrors()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_VALID_DATA));
        stream.Seek(6, 0);

        var validationFeedbackItems =
            DataValidation.Validate(stream, "filename", 5, 4, 1, Encoding.UTF8, PxFileSyntaxConf.Default);


        Assert.AreEqual(0, validationFeedbackItems.Count());
        foreach (var validationFeedbackItem in validationFeedbackItems)
        {
            Logger.LogMessage($"Line {validationFeedbackItem.Object.Line}, Char {validationFeedbackItem.Object.Character}: " 
                              + $"{validationFeedbackItem.Feedback.Rule} {validationFeedbackItem.Feedback.AdditionalInfo}");
        }
    }

    [TestMethod]
    public void TestValidateWithErrors()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_INVALID_DATA));
        stream.Seek(6, 0);

        var validationFeedbackItems =
            DataValidation.Validate(stream, "filename", 5, 4, 1, Encoding.UTF8, PxFileSyntaxConf.Default);


        Assert.AreEqual(10, validationFeedbackItems.Count());
        foreach (var validationFeedbackItem in validationFeedbackItems)
        {
            Logger.LogMessage($"Line {validationFeedbackItem.Object.Line}, Char {validationFeedbackItem.Object.Character}: " 
                              + $"{validationFeedbackItem.Feedback.Rule} {validationFeedbackItem.Feedback.AdditionalInfo}");
        }
    }
    
    [Ignore] // Used to check the performance of the tokenization
    [TestMethod]
    public void TokenizeBigFile()
    {
        var handle = File.OpenHandle(@"D:\UD\reen1\work\code\Px.Utils\statfin_tyonv_pxt_12ts.px");
        var stream = new FileStream(handle, FileAccess.Read, 4096);

        stream.Seek(77670,0);

        var tokens = DataValidation.Tokenize(stream, PxFileSyntaxConf.Default, Encoding.UTF8);

        var i = 0;
        foreach (var token in tokens)
        {
            i++;
        }
        Logger.LogMessage($"count: {i}");
    }

    [Ignore] // Used to check the performance of the validation 
    [TestMethod]
    public void TestValidatePerformance()
    {
        var handle = File.OpenHandle(@"D:\UD\reen1\work\code\Px.Utils\statfin_tyonv_pxt_12ts.px");
        var stream = new FileStream(handle, FileAccess.Read, 4096);
        var streamEncoding = Encoding.UTF8;
        stream.Seek(77677,0);

        var validationFeedbackItems =
            DataValidation.Validate(stream, "filename", 2821, 44712, 1, streamEncoding, PxFileSyntaxConf.Default);

        foreach (var validationFeedbackItem in validationFeedbackItems)
        {
            Logger.LogMessage($"Line {validationFeedbackItem.Object.Line}, Char {validationFeedbackItem.Object.Character}: " 
                              + $"{validationFeedbackItem.Feedback.Rule} {validationFeedbackItem.Feedback.AdditionalInfo}");
        }
        Assert.AreEqual(0, validationFeedbackItems.Count());
    }
}