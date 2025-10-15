namespace Calibrator.WpfControl.Validation.Validators;

/// <summary>
/// Validator that checks if a value is provided (not null or empty)
/// </summary>
public class RequiredValidator : IValidator<object>
{
    private readonly string _errorMessage;
    
    public RequiredValidator(string errorMessage = "This field is required")
    {
        _errorMessage = errorMessage;
    }
    
    public ValidationResult Validate(object value)
    {
        if (value == null)
            return ValidationResult.Failure(_errorMessage);
        
        if (value is string str && string.IsNullOrWhiteSpace(str))
            return ValidationResult.Failure(_errorMessage);
        
        return ValidationResult.Success();
    }
}
