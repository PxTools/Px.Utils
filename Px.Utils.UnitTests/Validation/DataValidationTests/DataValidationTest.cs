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
    public async Task TokenizeAsync()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_VALID_DATA));
        stream.Seek(6,0);
        
        var tokens = DataValidation.TokenizeAsync(stream, PxFileSyntaxConf.Default, Encoding.UTF8);

        var i = 0;
        await foreach (var token in tokens)
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

        var validationFeedbacks =
            DataValidation.Validate(stream, 5, 4, 1, Encoding.UTF8, PxFileSyntaxConf.Default);


        Assert.AreEqual(0, validationFeedbacks.Count());
        foreach (var validationFeedback in validationFeedbacks)
        {
            Logger.LogMessage($"Line {validationFeedback.Line}, Char {validationFeedback.Character}: " 
                              + $"{validationFeedback.Rule} {validationFeedback.AdditionalInfo}");
        }
    }

    [TestMethod]
    public async Task TestValidateAsyncWithoutErrors()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_VALID_DATA));
        stream.Seek(6, 0);

        var validationFeedbacks =
            await DataValidation.ValidateAsync(stream, 5, 4, 1, Encoding.UTF8, PxFileSyntaxConf.Default);

        foreach (var validationFeedback in validationFeedbacks)
        {
            Logger.LogMessage($"Line {validationFeedback.Line}, Char {validationFeedback.Character}: " 
                              + $"{validationFeedback.Rule} {validationFeedback.AdditionalInfo}");
        }
        Assert.AreEqual(0, validationFeedbacks.Count());

    }
    [TestMethod]
    public void TestValidateWithErrors()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_INVALID_DATA));
        stream.Seek(6, 0);

        var validationFeedbacks =
            DataValidation.Validate(stream, 5, 4, 1, Encoding.UTF8, PxFileSyntaxConf.Default);


        Assert.AreEqual(10, validationFeedbacks.Count());
        foreach (var validationFeedback in validationFeedbacks)
        {
            Logger.LogMessage($"Line {validationFeedback.Line}, Char {validationFeedback.Character}: " 
                              + $"{validationFeedback.Rule} {validationFeedback.AdditionalInfo}");
        }
    }

    [TestMethod]
    public async Task TestValidateAsyncWithErrors()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(DataStreamContents.SIMPLE_INVALID_DATA));
        stream.Seek(6, 0);

        var validationFeedbacks =
            await DataValidation.ValidateAsync(stream, 5, 4, 1, Encoding.UTF8, PxFileSyntaxConf.Default);

        foreach (var validationFeedback in validationFeedbacks)
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
        Assert.AreEqual(252309823, i);
    }

    [Ignore] // Used to check the performance of the tokenization
    [TestMethod]
    public async Task TokenizeBigFileAsync()
    {
        var handle = File.OpenHandle(@"D:\UD\reen1\work\code\Px.Utils\statfin_tyonv_pxt_12ts.px");
        var stream = new FileStream(handle, FileAccess.Read, 4096);

        stream.Seek(77670,0);

        var tokens = DataValidation.TokenizeAsync(stream, PxFileSyntaxConf.Default, Encoding.UTF8);

        var i = 0;
        await foreach (var token in tokens)
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
        var handle = File.OpenHandle(@"D:\UD\reen1\work\code\Px.Utils\statfin_tyonv_pxt_12ts.px");
        var stream = new FileStream(handle, FileAccess.Read, 4096);
        var streamEncoding = Encoding.UTF8;
        stream.Seek(77677,0);

        var validationFeedbacks =
            DataValidation.Validate(stream, 2821, 44712, 791, streamEncoding, PxFileSyntaxConf.Default);

        foreach (var validationFeedback in validationFeedbacks)
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
        var handle = File.OpenHandle(@"D:\UD\reen1\work\code\Px.Utils\statfin_tyonv_pxt_12ts.px");
        var stream = new FileStream(handle, FileAccess.Read, 4096);
        var streamEncoding = Encoding.UTF8;
        stream.Seek(77677,0);

        var validationFeedbacks =
            await DataValidation.ValidateAsync(stream, 2821, 44712, 791, streamEncoding, PxFileSyntaxConf.Default);

        foreach (var validationFeedback in validationFeedbacks)
        {
            Logger.LogMessage($"Line {validationFeedback.Line}, Char {validationFeedback.Character}: " 
                              + $"{validationFeedback.Rule} {validationFeedback.AdditionalInfo}");
        }
        Assert.AreEqual(0, validationFeedbacks.Count());
    }
}