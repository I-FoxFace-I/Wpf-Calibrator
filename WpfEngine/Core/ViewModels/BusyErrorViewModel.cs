using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Core.ViewModels;

/// <summary>
/// Base ViewModel with busy and error state (common combination)
/// </summary>
public abstract partial class BusyErrorViewModel : BaseViewModel, IErrorViewModel
{
    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string? _errorMessage;

    protected BusyErrorViewModel(ILogger logger) : base(logger)
    {
    }

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
    /// Executes operation with busy indicator and error handling
    /// </summary>
    protected async Task ExecuteWithBusyAndErrorAsync(Func<Task> operation, string? busyMessage = null)
    {
        try
        {
            ClearError();
            IsBusy = true;
            BusyMessage = busyMessage;
            await operation();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }
}
