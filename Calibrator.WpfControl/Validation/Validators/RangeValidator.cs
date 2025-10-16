using System;

namespace Calibrator.WpfControl.Validation.Validators;

/// <summary>
/// Validator that checks if a numeric value is within a specified range
/// </summary>
public class RangeValidator : IValidator<object>
{
    private readonly double _minimum;
    private readonly double _maximum;
    private readonly string _errorMessage;

    /// <summary>
    /// Initializes a new instance of the RangeValidator class
    /// </summary>
    /// <param name="minimum">The minimum allowed value</param>
    /// <param name="maximum">The maximum allowed value</param>
    /// <param name="errorMessage">The error message to display when validation fails</param>
    public RangeValidator(double minimum, double maximum, string errorMessage)
    {
        _minimum = minimum;
        _maximum = maximum;
        _errorMessage = errorMessage ?? $"Value must be between {minimum} and {maximum}";
    }

    /// <summary>
    /// Validates that the numeric value is within the specified range
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>A validation result indicating success or failure</returns>
    public ValidationResult Validate(object value)
    {
        if (value == null)
            return ValidationResult.Success(); // Use RequiredValidator for null checks

        if (!double.TryParse(value.ToString(), out double numericValue))
            return ValidationResult.Failure("Value must be a number");

        if (numericValue < _minimum || numericValue > _maximum)
            return ValidationResult.Failure(_errorMessage);

        return ValidationResult.Success();
    }
}