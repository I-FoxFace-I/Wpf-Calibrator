namespace Calibrator.WpfControl.Validation.Validators;

/// <summary>
/// Validator that checks if a string does not exceed maximum length
/// </summary>
public class MaxLengthValidator : IValidator<object>
{
    private readonly int _maxLength;
    private readonly string _errorMessage;
    
    public MaxLengthValidator(int maxLength, string errorMessage = null)
    {
        _maxLength = maxLength;
        _errorMessage = errorMessage ?? $"Must not exceed {maxLength} characters";
    }
    
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
