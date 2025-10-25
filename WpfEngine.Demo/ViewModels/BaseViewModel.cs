using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Base ViewModel using CommunityToolkit.Mvvm
/// Provides common functionality for all ViewModels
/// </summary>
public abstract partial class BaseViewModel : WpfEngine.Core.ViewModels.BaseViewModel
{
    
    
    [ObservableProperty]
    private bool _isBusy;
    
    [ObservableProperty]
    private string? _errorMessage;
    
    protected BaseViewModel(ILogger logger) : base(logger)
    {
    }

    public Guid Id { get; }

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
