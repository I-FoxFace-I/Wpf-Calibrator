using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;
using WpfEngine.Services.WindowTracking;

namespace WpfEngine.Core.ViewModels;

/// <summary>
/// Base ViewModel for shell windows
/// Shell windows contain content managed by IContentManager
/// Handles shell lifecycle and content change notifications
/// </summary>
public abstract partial class ShellViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly IContentManager _contentManager;
    private readonly IWindowService _windowService;
    private bool _disposed;

    protected ShellViewModel(
        IContentManager contentManager,
        IWindowService windowService,
        ILogger logger) : base(logger)
    {
        _contentManager = contentManager;
        _windowService = windowService;

        // Subscribe to content manager's PropertyChanged
        _contentManager.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IContentManager.CurrentContent))
            {
                OnPropertyChanged(nameof(CurrentContent));
                OnContentChanged(_contentManager.CurrentContent);
                
                Logger.LogInformation("[SHELL_VM] Content changed to {Type}",
                    _contentManager.CurrentContent?.GetType().Name ?? "null");
            }
        };

        // Subscribe to shell close requests from content
        _contentManager.ShellCloseRequested += OnShellCloseRequested;

        Logger.LogInformation("[SHELL_VM] {ShellType} created", GetType().Name);
    }

    /// <summary>
    /// Current content ViewModel from ContentManager
    /// Bind to this property in shell's ContentControl
    /// </summary>
    public object? CurrentContent => _contentManager.CurrentContent;

    /// <summary>
    /// Content manager for this shell
    /// Exposed for advanced scenarios
    /// </summary>
    protected IContentManager ContentManager => _contentManager;

    /// <summary>
    /// Window service for closing this shell
    /// </summary>
    protected IWindowService WindowService => _windowService;

    /// <summary>
    /// Initialize shell - override to set initial content
    /// </summary>
    public abstract override Task InitializeAsync();

    /// <summary>
    /// Called when content changes
    /// Override to react to content changes
    /// </summary>
    protected virtual void OnContentChanged(object? newContent)
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Handles shell close request from content
    /// Override to customize close behavior
    /// </summary>
    protected virtual void OnShellCloseRequested(object? sender, ShellCloseRequestedEventArgs e)
    {
        Logger.LogInformation("[SHELL_VM] Shell close requested (confirmation: {ShowConfirmation})",
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
                Logger.LogInformation("[SHELL_VM] Shell close cancelled by user");
                return;
            }
        }

        CloseShell();
    }

    /// <summary>
    /// Closes the shell window
    /// </summary>
    protected virtual void CloseShell()
    {
        Logger.LogInformation("[SHELL_VM] Closing shell via WindowService");
        _windowService.Close(this.GetVmKey());
    }

    public virtual void Dispose()
    {
        if (_disposed) return;

        Logger.LogInformation("[SHELL_VM] {ShellType} disposing", GetType().Name);

        // Unsubscribe from events
        _contentManager.ShellCloseRequested -= OnShellCloseRequested;

        // Clear content history
        _contentManager.ClearHistory();

        _disposed = true;
    }
}

