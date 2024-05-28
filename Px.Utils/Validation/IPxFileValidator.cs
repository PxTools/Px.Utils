namespace Px.Utils.Validation
{
    /// <summary>
    /// TODO: Summary
    /// </summary>
    internal interface IPxFileValidator
    {
        internal IValidationResult Validate();

        internal Task<IValidationResult> ValidateAsync(CancellationToken cancellationToken = default);
    }
}
