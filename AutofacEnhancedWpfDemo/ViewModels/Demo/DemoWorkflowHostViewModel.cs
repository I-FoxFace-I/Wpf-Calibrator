using System;
using System.Threading.Tasks;
using System.Windows;
using AutofacEnhancedWpfDemo.Services.Demo;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels.Demo;

/// <summary>
/// Workflow Host ViewModel - Shell pattern
/// Exposes Navigator's CurrentViewModel for content binding
/// Handles window close requests from Navigator
/// </summary>
public partial class DemoWorkflowHostViewModel : BaseViewModel, IAsyncInitializable, IDisposable
{
    private readonly INavigator _navigator;
    private readonly IWindowManager _windowManager;
    private bool _disposed;

    /// <summary>
    /// Current workflow step ViewModel from Navigator
    /// </summary>
    public object? CurrentContent => _navigator.CurrentViewModel;

    public DemoWorkflowHostViewModel(
        INavigator navigator,
        IWindowManager windowManager,
        ILogger<DemoWorkflowHostViewModel> logger) : base(logger)
    {
        _navigator = navigator;
        _windowManager = windowManager;

        // Subscribe to Navigator's PropertyChanged
        _navigator.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(INavigator.CurrentViewModel))
            {
                OnPropertyChanged(nameof(CurrentContent));
                Logger.LogInformation("[WORKFLOW] Content changed to {Type}",
                    _navigator.CurrentViewModel?.GetType().Name ?? "null");
            }
        };

        // Subscribe to window close requests from Navigator
        _navigator.WindowCloseRequested += OnWindowCloseRequested;

        Logger.LogInformation("[WORKFLOW] Host ViewModel created");
    }

    public async Task InitializeAsync()
    {
        Logger.LogInformation("[WORKFLOW] Starting workflow - navigating to Step 1");
        await _navigator.NavigateToAsync<DemoWorkflowStep1ViewModel>();
    }

    private async void OnWindowCloseRequested(object? sender, WindowCloseRequestedEventArgs e)
    {
        Logger.LogInformation("[WORKFLOW] Window close requested (confirmation: {ShowConfirmation})",
            e.ShowConfirmation);

        if (e.ShowConfirmation)
        {
            var message = e.ConfirmationMessage ?? "Are you sure you want to close this window?";
            var result = MessageBox.Show(
                message,
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                Logger.LogInformation("[WORKFLOW] Window close cancelled by user");
                return;
            }
        }

        Logger.LogInformation("[WORKFLOW] Closing workflow window via WindowManager");

        // Close this window via WindowManager
        // WindowManager tracks windows by (Guid, Type), but for this use case
        // we can use a simpler approach - get the window from the scope
        //await Task.Run(() =>
        //{
        //    System.Windows.Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        // Find the window by ViewModel DataContext
        //        foreach (Window window in System.Windows.Application.Current.Windows)
        //        {
        //            if (window.DataContext == this)
        //            {
        //                window.Close();
        //                break;
        //            }
        //        }
        //    });
        //});
        _windowManager.CloseWindow<DemoWorkflowHostViewModel>(this);
    }

    public void Dispose()
    {
        if (_disposed) return;

        Logger.LogInformation("[WORKFLOW] Host ViewModel disposed");

        // Unsubscribe from events
        _navigator.WindowCloseRequested -= OnWindowCloseRequested;

        // Clear navigation history
        _navigator.ClearHistory();

        _disposed = true;
    }
}