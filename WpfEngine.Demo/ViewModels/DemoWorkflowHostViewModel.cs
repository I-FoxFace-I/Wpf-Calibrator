using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;
using WpfEngine.Services.WindowTracking;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Workflow Host ViewModel - Shell pattern with Session support
/// Creates a workflow session where all windows share common services
/// Exposes Navigator's CurrentViewModel for content binding
/// Handles window close requests from Navigator
/// </summary>
public partial class DemoWorkflowHostViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly INavigationService _navigator;
    private readonly IWorkflowSessionFactory _sessionFactory;
    private IWorkflowSession? _session;
    private bool _disposed;

    /// <summary>
    /// Current workflow step ViewModel from Navigator
    /// </summary>
    public object? CurrentContent => _navigator.CurrentViewModel;

    /// <summary>
    /// Current workflow session
    /// </summary>
    public IWorkflowSession? Session => _session;

    public DemoWorkflowHostViewModel(
        INavigationService navigator,
        IWorkflowSessionFactory sessionFactory,
        ILogger<DemoWorkflowHostViewModel> logger) : base(logger)
    {
        _navigator = navigator;
        _sessionFactory = sessionFactory;

        // Subscribe to Navigator's PropertyChanged
        _navigator.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
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

    public override async Task InitializeAsync()
    {
        // Create workflow session - all windows opened from this session will share services
        _session = _sessionFactory.CreateSession("order-creation-workflow");
        
        Logger.LogInformation("[WORKFLOW] Session created: {SessionId}", _session.SessionId);
        
        // Navigate to first step
        Logger.LogInformation("[WORKFLOW] Starting workflow - navigating to Step 1");
        await _navigator.NavigateToAsync<DemoWorkflowStep1ViewModel>();
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

        Logger.LogInformation("[WORKFLOW] Closing workflow and session");

        // Close session (closes all session windows)
        _session?.Close();
        
        // Find and close this window
        foreach (Window window in System.Windows.Application.Current.Windows)
        {
            if (window.DataContext == this)
            {
                window.Close();
                break;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        Logger.LogInformation("[WORKFLOW] Host ViewModel disposing - closing session");

        // Unsubscribe from events
        _navigator.WindowCloseRequested -= OnWindowCloseRequested;

        // Clear navigation history
        _navigator.ClearHistory();

        // Close and dispose session
        _session?.Dispose();

        _disposed = true;
    }
}