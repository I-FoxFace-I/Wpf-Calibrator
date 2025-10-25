using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Core.ViewModels;

// ========== STATE MIXINS ==========

/// <summary>
/// Base ViewModel with busy state tracking
/// </summary>
public abstract partial class BusyViewModel : BaseViewModel, IBusyViewModel
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _busyMessage;

    protected BusyViewModel(ILogger logger) : base(logger)
    {
    }

    /// <summary>
    /// Executes async operation with busy indicator
    /// </summary>
    protected async Task ExecuteWithBusyAsync(Func<Task> operation, string? busyMessage = null)
    {
        try
        {
            IsBusy = true;
            BusyMessage = busyMessage;
            await operation();
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }
}
