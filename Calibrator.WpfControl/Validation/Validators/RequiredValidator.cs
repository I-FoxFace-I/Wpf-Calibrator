namespace Calibrator.WpfControl.Validation.Validators;

/// <summary>
/// Validator that checks if a value is provided (not null or empty)
/// </summary>
public class RequiredValidator : IValidator<object>
{
    private readonly string _errorMessage;

    /// <summary>
    /// Initializes a new instance of the RequiredValidator class
    /// </summary>
    /// <param name="errorMessage">The error message to display when validation fails</param>
    public RequiredValidator(string errorMessage = "This field is required")
    {
        _errorMessage = errorMessage;
    }

    /// <summary>
    /// Validates that the provided value is not null or empty
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>A validation result indicating success or failure</returns>
    public ValidationResult Validate(object value)
    {
        if (value == null)
            return ValidationResult.Failure(_errorMessage);

        if (value is string str && string.IsNullOrWhiteSpace(str))
            return ValidationResult.Failure(_errorMessage);

        return ValidationResult.Success();
    }
}