using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using WpfEngine.ViewModels;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Base ViewModel using CommunityToolkit.Mvvm
/// Provides common functionality for all ViewModels
/// </summary>
public abstract partial class BaseViewModel : WpfEngine.ViewModels.Base.BaseViewModel
{
    //[ObservableProperty]
    //private bool _isBusy;
    
    //[ObservableProperty]
    //private string? _errorMessage;
    
    protected BaseViewModel(ILogger<BaseViewModel> logger) : base(logger)
    {

    }

    ///// <summary>
    ///// Clears error message
    ///// </summary>
    //protected void ClearError()
    //{
    //    ErrorMessage = null;
    //}
    
    ///// <summary>
    ///// Sets error message
    ///// </summary>
    //protected void SetError(string message)
    //{
    //    ErrorMessage = message;
    //    Logger.LogError("ViewModel error: {Message}", message);
    //}
}
