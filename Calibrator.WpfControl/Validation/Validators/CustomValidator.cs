using System;

namespace Calibrator.WpfControl.Validation.Validators;

/// <summary>
/// Validator that uses a custom validation function
/// </summary>
public class CustomValidator : IValidator<object>
{
    private readonly Func<object, bool> _validationFunc;
    private readonly string _errorMessage;

    /// <summary>
    /// Initializes a new instance of the CustomValidator class
    /// </summary>
    /// <param name="validationFunc">The function to use for validation</param>
    /// <param name="errorMessage">The error message to display when validation fails</param>
    /// <exception cref="ArgumentNullException">Thrown when validationFunc is null</exception>
    public CustomValidator(Func<object, bool> validationFunc, string errorMessage = "Validation failed")
    {
        _validationFunc = validationFunc ?? throw new ArgumentNullException(nameof(validationFunc));
        _errorMessage = errorMessage;
    }

    /// <summary>
    /// Validates the provided value using the custom validation function
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>A validation result indicating success or failure</returns>
    public ValidationResult Validate(object value)
    {
        try
        {
            if (_validationFunc(value))
                return ValidationResult.Success();

            return ValidationResult.Failure(_errorMessage);
        }
        catch (InvalidOperationException ex)
        {
            return ValidationResult.Failure($"Validation error: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            return ValidationResult.Failure($"Validation error: {ex.Message}");
        }
    }
}