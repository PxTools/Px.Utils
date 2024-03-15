namespace PxUtils.Validation
{
    public readonly struct ValidationFeedbackItem(IValidationEntry entry, IValidationFeedback feedback)
    {
        public string File { get; } = entry.File;
        public long Line { get; } = entry.Line;
        public int Character { get; } = entry.Character;
        public IValidationFeedback Feedback { get; } = feedback;
    }
}
