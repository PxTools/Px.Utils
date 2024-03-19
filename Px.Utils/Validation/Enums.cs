namespace PxUtils.Validation
{
    /// <summary>
    /// Defines the levels of validation feedback. These levels can be used to categorize feedback items by severity.
    /// </summary>
    public enum ValidationFeedbackLevel
    {
        Warning = 0,
        Error = 1
    }

    /// <summary>
    /// Defines the types of values that can be validated. These types correspond to the types of values that can be found in a PX file.
    /// </summary>
    public enum ValueType
    {
        String,
        ListOfStrings,
        Number,
        Boolean,
        DateTime,
        Timeval
    }
}
