using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels;

/// <summary>
/// Base ViewModel using CommunityToolkit.Mvvm
/// Provides common functionality for all ViewModels
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    protected ILogger Logger { get; }
    
    [ObservableProperty]
    private bool _isBusy;
    
    [ObservableProperty]
    private string? _errorMessage;
    
    protected BaseViewModel(ILogger logger)
    {
        Logger = logger;
    }
    
    /// <summary>
    /// Clears error message
    /// </summary>
    protected void ClearError()
    {
        ErrorMessage = null;
    }
    
    /// <summary>
    /// Sets error message
    /// </summary>
    protected void SetError(string message)
    {
        ErrorMessage = message;
        Logger.LogError("ViewModel error: {Message}", message);
    }
}
