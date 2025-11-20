using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using WpfEngine.Abstract;
using WpfEngine.ViewModels;
using WpfEngine.Data.Content;
using WpfEngine.Services;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using WpfEngine.ViewModels.Base;

namespace WpfEngine.ViewModels.Managed;

/// <summary>
/// Base ViewModel for shell windows
/// Shell windows contain content managed by IContentManager
/// Handles shell lifecycle and content change notifications
/// </summary>
public abstract partial class ShellViewModel : BaseViewModel, IDisposable
{
    private readonly INavigator _navigator;
    private readonly IWindowContext _windowContext;
    private bool _disposed;

    protected ShellViewModel(
        INavigator navigator,
        IWindowContext windowService,
        ILogger<ShellViewModel> logger) : base(logger)
    {
        _navigator = navigator;
        _windowContext = windowService;

        // Subscribe to content manager's PropertyChanged
        _navigator.PropertyChanged += OnContentChanged;
        _navigator.NavigatorCloseRequest += OnCloseRequestAsync;
        // Subscribe to shell close requests from content
        //_navigator.SetCloseHandler(HandleCloseRequestAsync);

        Logger.LogInformation("[SHELL_VM] {ShellType} created", GetType().Name);
    }

    /// <summary>
    /// Current content ViewModel from ContentManager
    /// Bind to this property in shell's ContentControl
    /// </summary>
    public object? CurrentContent => _navigator.CurrentViewModel;

    /// <summary>
    /// Expose Navigator for XAML binding
    /// Bind ContentControl.Content to this property
    /// </summary>
    protected INavigator Navigator => _navigator;

    /// <summary>
    /// Window service for closing this shell
    /// </summary>
    protected IWindowContext WindowContext => _windowContext;

    /// <summary>
    /// Initialize shell - override to set initial content
    /// </summary>
    public abstract Task InitializeAsync(CancellationToken cancelationToken=default);

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

    protected virtual void OnCloseRequestAsync(object? sender, NavigatorCloseRequestedEventArgs e)
    {
        (var showConfirmation, var message) = (e.ShowConfirmation, e.ConfirmationMessage);
        
        Logger.LogInformation("[{ViewModelType}] Close requested (confirmation: {ShowConfirmation})",
            GetType().Name, showConfirmation);

        if (showConfirmation)
        {
            var result = MessageBox.Show(
                message,
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                Logger.LogInformation("[SHELL_VM] Shell close cancelled by user");
                
                return;
            }
        }

        try
        {
            Logger.LogInformation("[SHELL_VM] Closing shell via WindowService");
            
            WindowContext.CloseWindow();
        }
        catch (Exception ex)
        {
            Logger.LogError("[SHELL_VM] Error on closing shell via WindowService");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_disposed) return;

            Logger.LogInformation("[SHELL_VM] {ShellType} disposing", GetType().Name);

            // Unsubscribe from events
            _navigator.PropertyChanged -= OnContentChanged;
            _navigator.NavigatorCloseRequest -= OnCloseRequestAsync;
            // Clear content history
            _navigator.ClearHistory();

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

