using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Core.ViewModels;

/// <summary>
/// Base ViewModel with error state tracking
/// </summary>
public abstract partial class ErrorViewModel : BaseViewModel, IErrorViewModel
{
    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string? _errorMessage;

    protected ErrorViewModel(ILogger logger) : base(logger)
    {
    }

    /// <summary>
    /// Sets error state
    /// </summary>
    protected void SetError(string errorMessage)
    {
        HasError = true;
        ErrorMessage = errorMessage;
        Logger.LogError("[{ViewModelType}] Error: {ErrorMessage}", GetType().Name, errorMessage);
    }

    public void ClearError()
    {
        HasError = false;
        ErrorMessage = null;
    }

    /// <summary>
    /// Executes operation with error handling
    /// </summary>
    protected async Task ExecuteWithErrorHandlingAsync(Func<Task> operation)
    {
        try
        {
            ClearError();
            await operation();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }
}
