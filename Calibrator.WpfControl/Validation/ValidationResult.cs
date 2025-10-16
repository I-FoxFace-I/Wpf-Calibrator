namespace Calibrator.WpfControl.Validation;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Indicates whether the validation was successful.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Error message if validation failed.
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true, ErrorMessage = string.Empty };

    /// <summary>
    /// Creates a failed validation result with error message.
    /// </summary>
    /// <param name="message">Error message.</param>
    public static ValidationResult Failure(string message) => new() { IsValid = false, ErrorMessage = message };
}