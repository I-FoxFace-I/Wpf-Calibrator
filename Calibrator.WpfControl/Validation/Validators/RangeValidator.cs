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
    
    public RangeValidator(double minimum, double maximum, string errorMessage = null)
    {
        _minimum = minimum;
        _maximum = maximum;
        _errorMessage = errorMessage ?? $"Value must be between {minimum} and {maximum}";
    }
    
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
