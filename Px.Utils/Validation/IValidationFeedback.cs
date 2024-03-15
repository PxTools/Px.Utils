namespace PxUtils.Validation
{
    public interface IValidationFeedback
    {
        public ValidationFeedbackLevel Level { get; }
        public string Rule { get; }
    }
}
