using System.Text.RegularExpressions;

namespace Calibrator.WpfControl.Validation.Validators;

/// <summary>
/// Validator that checks if a value matches a regular expression pattern
/// </summary>
public class RegexValidator : IValidator<object>
{
    private readonly Regex _regex;
    private readonly string _errorMessage;

    /// <summary>
    /// Initializes a new instance of the RegexValidator class
    /// </summary>
    /// <param name="pattern">The regular expression pattern to match against</param>
    /// <param name="errorMessage">The error message to display when validation fails</param>
    public RegexValidator(string pattern, string errorMessage = "Invalid format")
    {
        _regex = new Regex(pattern, RegexOptions.Compiled);
        _errorMessage = errorMessage;
    }

    /// <summary>
    /// Validates that the value matches the regular expression pattern
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <returns>A validation result indicating success or failure</returns>
    public ValidationResult Validate(object value)
    {
        if (value == null)
            return ValidationResult.Success(); // Use RequiredValidator for null checks

        var stringValue = value.ToString();

        if (string.IsNullOrWhiteSpace(stringValue))
            return ValidationResult.Success();

        if (!_regex.IsMatch(stringValue))
            return ValidationResult.Failure(_errorMessage);

        return ValidationResult.Success();
    }
}