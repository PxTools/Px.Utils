using PxUtils.PxFile;

namespace Px.Utils.Validation
{
    /*
     * THIS IS A PLANNING DOCUMENT FOR SHARING DESIGN IDEAS BETWEEN TEAM MEMBERS
     * DO NOT USE THIS CODE IN ACTUAL FEATURE DEVELOPMENT
     * REMOVE DOCUMENT WHEN FEATURE IS IMPLEMENTED
     */


    public struct ValidationFeedbackItem
    {
        public string Message { get; set; }
        public long Line { get; set; }
        public int Character { get; set; }
        public string FileName { get; set; }
    }

    public abstract class ValidationEntry
    {
        public int Line { get; }
        public int Character { get; }
    }

    public class StringValidationEntry : ValidationEntry
    {
        public string EntryString { get; }
    }

    public class KeyValueValidationEntry : ValidationEntry
    {
        public KeyValuePair<string, string> EntryPair { get; }
    }

    public class StructuredValidationEntry : ValidationEntry
    {
        public ValidationFeedbackItem Key { get; }
        public string Value { get; }
    }


    public readonly record struct ValidationEntryKey
    {
        string Keyword { get; }
        string Language { get; }
        string FirstIdentifier { get; }
        string SecondIdentifier { get; }
    }

    public interface IEntryBuilder
    {
        public IEnumerable<string> ReadEntries();
    }

    public interface IEntrySyntaxValidator
    {
        public IEnumerable<ValidationFeedbackItem> ValidateStringEntries(IEnumerable<StringValidationEntry> inputStrings);

        public IEnumerable<KeyValuePair<string, string>> BuildKeyValuePairs(IEnumerable<StringValidationEntry> inputStrings);


        public IEnumerable<ValidationFeedbackItem> ValidateKeyValuePairs(IEnumerable<KeyValueValidationEntry> inputEntries);

        public IEnumerable<ValidationEntry> BuildValidationentries(IEnumerable<KeyValueValidationEntry> inputPairs);


        public IEnumerable<ValidationFeedbackItem> ValidateEntries(IEnumerable<StructuredValidationEntry> inputEntries);
    }

    public interface IDataValidation
    {
        public IEnumerable<ValidationFeedbackItem> Validate(Stream stream, long rowLen, long numOfRows, int startRow, PxFileSyntaxConf? conf = null);
    }



}
