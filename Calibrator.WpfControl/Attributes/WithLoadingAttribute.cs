namespace Calibrator.WpfControl.Attributes;

/// <summary>
/// Marks a partial method to automatically generate its body with IsLoading state management.
/// The method must be declared as partial without a body.
/// The generator will create the method body that calls a corresponding Core method with IsLoading wrapping.
/// 
/// Usage:
/// 1. Auto-detection by name pattern:
///    [WithLoading] private partial Task LoadDataAsync(); 
///    // Finds and calls: LoadDataCoreAsync() or LoadDataCore()
/// 
/// 2. Explicit core method name:
///    [WithLoading("LoadFromContext")] private partial Task ReadDataAsync();
///    // Calls: LoadFromContextAsync() or LoadFromContext()
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class WithLoadingAttribute : Attribute
{
    /// <summary>
    /// Creates a WithLoading attribute with automatic core method detection.
    /// Looks for method with "Core" or "CoreAsync" suffix added to the partial method name.
    /// </summary>
    public WithLoadingAttribute()
    {
    }

    /// <summary>
    /// Creates a WithLoading attribute with explicit core method name.
    /// </summary>
    /// <param name="coreMethodName">The name of the core method to call (without "Async" or "Core" suffix).</param>
    public WithLoadingAttribute(string coreMethodName)
    {
        CoreMethodName = coreMethodName;
    }

    /// <summary>
    /// Gets the explicitly specified name of the core method to call.
    /// If null, the core method name is auto-detected by adding "Core" or "CoreAsync" suffix.
    /// </summary>
    public string? CoreMethodName { get; }
}

