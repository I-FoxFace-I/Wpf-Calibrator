namespace Calibrator.WpfControl.Validation.Validators;

/// <summary>
/// Validator that checks if a string does not exceed maximum length
/// </summary>
public class MaxLengthValidator : IValidator<object>
{
    private readonly int _maxLength;
    private readonly string _errorMessage;

    /// <summary>
    /// Initializes a new instance of the MaxLengthValidator class
    /// </summary>
    /// <param name="maxLength">The maximum allowed length</param>
    /// <param name="errorMessage">The error message to display when validation fails</param>
    public MaxLengthValidator(int maxLength, string errorMessage)
    {
        _maxLength = maxLength;
        _errorMessage = errorMessage ?? $"Must not exceed {maxLength} characters";
    }

    /// <summary>
    /// Validates that the string value does not exceed the maximum length
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>A validation result indicating success or failure</returns>
    public ValidationResult Validate(object value)
    {
        if (value == null)
            return ValidationResult.Success(); // Use RequiredValidator for null checks

        var stringValue = value.ToString();

        if (string.IsNullOrEmpty(stringValue))
            return ValidationResult.Success();

        if (stringValue.Length > _maxLength)
            return ValidationResult.Failure(_errorMessage);

        return ValidationResult.Success();
    }
}