namespace PxUtils.Validation
{
    /// <summary>
    /// A validation feedback item associates a <see cref="ValidationObject"/> with the feedback from validating that object.
    /// </summary>
    /// <param name="validationObject">The <see cref="ValidationObject"/> that this feedback item is associated with.</param>
    /// <param name="feedback">The <see cref="ValidationFeedback"/> from validating the object.</param>
    public readonly struct ValidationFeedbackItem(ValidationObject validationObject, ValidationFeedback feedback)
    {

        /// <summary>
        /// Gets the <see cref="ValidationObject"/> that this feedback item is associated with.
        /// </summary>
        public ValidationObject Object { get; } = validationObject;


        /// <summary>
        /// Gets the <see cref="ValidationFeedback"/> from validating the Object.
        /// </summary>
        public ValidationFeedback Feedback { get; } = feedback;
    }
}
