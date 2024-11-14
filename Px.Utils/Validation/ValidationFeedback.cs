using System.Collections.Concurrent;

namespace Px.Utils.Validation
{
    /// <summary>
    /// Represents the result of a validation operation. Associates feedback levels and rules with instances of violations.
    /// </summary>
    public class ValidationFeedback() : ConcurrentDictionary<ValidationFeedbackKey, List<ValidationFeedbackValue>>
    {
        /// <summary>
        /// Adds a feedback item to the feedback dictionary.
        /// </summary>
        public void Add(KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> item)
        {
            if (!ContainsKey(item.Key))
            {
                this[item.Key] = [];
            }
            this[item.Key].Add(item.Value);
        }

        /// <summary>
        /// Adds multiple feedback items to the feedback dictionary.
        /// </summary>
        /// <param name="feedbacks">Feedback key value pairs to add</param>
        public void AddRange(ConcurrentDictionary<ValidationFeedbackKey, List<ValidationFeedbackValue>> feedbacks)
        {
            foreach (KeyValuePair<ValidationFeedbackKey, List<ValidationFeedbackValue>> kvp in feedbacks)
            {
                if (!ContainsKey(kvp.Key))
                {
                    this[kvp.Key] = [];
                }
                this[kvp.Key].AddRange(kvp.Value);
            }
        }

        /// <summary>
        /// Constructor that initializes the feedback dictionary with a single feedback item.
        /// </summary>
        /// <param name="item">Feedback key value pair to add</param>
        public ValidationFeedback(KeyValuePair<ValidationFeedbackKey, ValidationFeedbackValue> item) : this()
        {
            Add(item);
        }
    }

    /// <summary>
    /// Validation feedback contains information about validation errors or warnings that occurred during validation.
    /// </summary>
    public readonly struct ValidationFeedbackKey(ValidationFeedbackLevel level, ValidationFeedbackRule rule)
    {
        /// <summary>
        /// Enum that gets the level of the feedback. This can be used to categorize feedback items by severity.
        /// </summary>
        public ValidationFeedbackLevel Level { get; } = level;

        /// <summary>
        /// Enum that defines the type of validation feedback rule. Can be used to categorize feedback by rule type or for translations.
        /// </summary>
        public ValidationFeedbackRule Rule { get; } = rule;
    }

    /// <summary>
    /// Stores information about a specific instance of a validation feedback rule violation.
    /// </summary>
    public readonly struct ValidationFeedbackValue(string filename, int? line = null, int? character = null, string? additionalInfo = null)
    {
        /// <summary>
        /// Name of the file where the violation occurred.
        /// </summary>
        public string Filename { get; } = filename;
        /// <summary>
        /// Line number where the violation occurred.
        /// </summary>
        public int? Line { get; } = line;
        /// <summary>
        /// Character position where the violation occurred.
        /// </summary>
        public int? Character { get; } = character;
        /// <summary>
        /// Additional information about the violation.
        /// </summary>
        public string? AdditionalInfo { get; } = additionalInfo;
    }
}
