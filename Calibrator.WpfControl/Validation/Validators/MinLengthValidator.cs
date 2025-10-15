namespace Calibrator.WpfControl.Validation.Validators;

/// <summary>
/// Validator that checks if a string has a minimum length
/// </summary>
public class MinLengthValidator : IValidator<object>
{
    private readonly int _minLength;
    private readonly string _errorMessage;
    
    public MinLengthValidator(int minLength, string errorMessage = null)
    {
        _minLength = minLength;
        _errorMessage = errorMessage ?? $"Must be at least {minLength} characters";
    }
    
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
