namespace PxUtils.Validation
{
    public interface IValidationFunction
    {
        bool IsRelevant(IValidationEntry entry);
        ValidationFeedbackItem? Validate(IValidationEntry entry);
    }
}
