using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using WpfEngine.Abstract;
using WpfEngine.Services.Autofac;
using WpfEngine.Data.Windows.Events;
using WpfEngine.Services;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace WpfEngine.Demo.ViewModels.Obsolete;

/// <summary>
/// Workflow Host ViewModel - Shell pattern
/// Exposes Navigator's CurrentViewModel for content binding
/// Handles window close requests from Navigator
/// </summary>
public partial class WorkflowHostViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly INavigator _navigator;
    private readonly IWindowContext _windowService;
    private bool _disposed;

    /// <summary>
    /// Current workflow step ViewModel from Navigator
    /// </summary>
    public object? CurrentContent => _navigator.CurrentViewModel;

    public WorkflowHostViewModel(
        INavigator navigator,
        IWindowContext windowService,
        ILogger<WorkflowHostViewModel> logger) : base(logger)
    {
        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));

        // Subscribe to Navigator's PropertyChanged
        _navigator.PropertyChanged += OnContentChanged;

        Logger.LogInformation("[WORKFLOW] Host ViewModel created");
    }
    public override async Task InitializeAsync()
    {
        await InitializeAsync(CancellationToken.None);
    }
    public async Task InitializeAsync(CancellationToken cancelationToken = default)
    {
        Logger.LogInformation("[WORKFLOW] Starting workflow - navigating to Step 1");
        await _navigator.NavigateToAsync<WorkflowStep1ViewModel>();
    }

    /// <summary>
    /// Called when content changes
    /// Override to react to content changes
    /// </summary>
    private void OnContentChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(INavigator.CurrentViewModel))
        {
            OnPropertyChanged(nameof(CurrentContent));
            OnContentChangedIntenal(_navigator.CurrentViewModel);

            Logger.LogInformation("[SHELL_VM] Content changed to {Type}",
                _navigator.CurrentViewModel?.GetType().Name ?? "null");
        }
    }

    protected virtual void OnContentChangedIntenal(object? newContent)
    {
        // Override in derived classes if needed
    }

    private void OnWindowCloseRequested(object? sender, WindowCloseRequestedEventArgs e)
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

            if (result != System.Windows.MessageBoxResult.Yes)
            {
                Logger.LogInformation("[WORKFLOW] Window close cancelled by user");
                return;
            }
        }

        Logger.LogInformation("[WORKFLOW] Closing workflow window");

        // Close this window via LocalWindowService
        _windowService.CloseWindow();
    }

    /// <summary>
    /// Handle close requests from child ViewModels
    /// Override in derived classes for custom close logic
    /// </summary>
    //protected virtual Task<bool> HandleCloseRequestAsync(bool showConfirmation, string? message)
    //{
    //    Logger.LogInformation("[{ViewModelType}] Close requested (confirmation: {ShowConfirmation})",
    //        GetType().Name, showConfirmation);

    //    if (showConfirmation)
    //    {
    //        var result = MessageBox.Show(
    //            message,
    //            "Confirmation",
    //            MessageBoxButton.YesNo,
    //            MessageBoxImage.Question);

    //        if (result != System.Windows.MessageBoxResult.Yes)
    //        {
    //            Logger.LogInformation("[SHELL_VM] Shell close cancelled by user");
    //            return Task.FromResult(false);
    //        }
    //    }

    //    bool success;
    //    try
    //    {
    //        Logger.LogInformation("[SHELL_VM] Closing shell via WindowService");
    //        _windowService.CloseWindow();
    //        success = true;
    //    }
    //    catch (Exception ex)
    //    {
    //        success = false;
    //    }
    //    return Task.FromResult(success);
    //}

    public void Dispose()
    {
        if (_disposed) return;

        Logger.LogInformation("[WORKFLOW] Host ViewModel disposed");

        // Unsubscribe from events
        _navigator.PropertyChanged -= OnContentChanged;

        // Clear navigation history
        _navigator.ClearHistory();

        _disposed = true;
    }
}