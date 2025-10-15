using System;

namespace Calibrator.WpfControl.Validation.Validators;

/// <summary>
/// Validator that uses a custom validation function
/// </summary>
public class CustomValidator : IValidator<object>
{
    private readonly Func<object, bool> _validationFunc;
    private readonly string _errorMessage;
    
    public CustomValidator(Func<object, bool> validationFunc, string errorMessage = "Validation failed")
    {
        _validationFunc = validationFunc ?? throw new ArgumentNullException(nameof(validationFunc));
        _errorMessage = errorMessage;
    }
    
    public ValidationResult Validate(object value)
    {
        try
        {
            if (_validationFunc(value))
                return ValidationResult.Success();
            
            return ValidationResult.Failure(_errorMessage);
        }
        catch (Exception ex)
        {
            return ValidationResult.Failure($"Validation error: {ex.Message}");
        }
    }
}
