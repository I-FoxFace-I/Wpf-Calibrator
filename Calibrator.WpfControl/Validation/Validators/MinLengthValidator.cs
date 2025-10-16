namespace Calibrator.WpfControl.Validation.Validators;

/// <summary>
/// Validator that checks if a string has a minimum length
/// </summary>
public class MinLengthValidator : IValidator<object>
{
    private readonly int _minLength;
    private readonly string _errorMessage;

    /// <summary>
    /// Initializes a new instance of the MinLengthValidator class
    /// </summary>
    /// <param name="minLength">The minimum required length</param>
    /// <param name="errorMessage">The error message to display when validation fails</param>
    public MinLengthValidator(int minLength, string errorMessage)
    {
        _minLength = minLength;
        _errorMessage = errorMessage ?? $"Must be at least {minLength} characters";
    }

    /// <summary>
    /// Validates that the string value meets the minimum length requirement
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

        if (stringValue.Length < _minLength)
            return ValidationResult.Failure(_errorMessage);

        return ValidationResult.Success();
    }
}