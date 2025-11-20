namespace WpfEngine.Abstract;

// ========== LIFECYCLE INTERFACES ==========

/// <summary>
/// Entity that requires async initialization
/// </summary>
public interface IInitializable
{
    /// <summary>
    /// Performs async initialization
    /// </summary>
    Task InitializeAsync();
}

/// <summary>
/// Entity that requires async initialization with parameters
/// </summary>
public interface IInitializable<in TParam>
{
    /// <summary>
    /// Performs async initialization with parameters
    /// </summary>
    Task InitializeAsync(TParam parameter);
}