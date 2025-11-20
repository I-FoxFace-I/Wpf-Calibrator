namespace WpfEngine.Abstract;

/// <summary>
/// Entity that supports validation
/// </summary>
public interface IValidatable
{
    /// <summary>
    /// Validates entity state
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    bool Validate();

    /// <summary>
    /// Gets all validation errors
    /// </summary>
    IEnumerable<string> GetValidationErrors();
}
