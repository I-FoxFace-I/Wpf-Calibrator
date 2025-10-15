namespace Calibrator.WpfControl.Validation;

/// <summary>
/// Interface for all validators
/// </summary>
/// <typeparam name="T">Type of value to validate</typeparam>
public interface IValidator<in T>
{
    /// <summary>
    /// Validates the provided value
    /// </summary>
    /// <param name="value">Value to validate</param>
    /// <returns>Validation result</returns>
    ValidationResult Validate(T value);
}
