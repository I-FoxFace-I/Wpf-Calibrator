using System;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;
using WpfEngine.Core.Views;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Services.Autofac;

/// <summary>
/// Dialog service using Autofac
/// Manages modal dialogs with typed results
/// </summary>
public class DialogService : IDialogService
{
    private readonly ILifetimeScope _scope;
    private readonly IViewLocatorService _viewLocator;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly ILogger<DialogService> _logger;

    public DialogService(
        ILifetimeScope scope,
        IViewLocatorService viewLocator,
        IViewModelFactory viewModelFactory,
        ILogger<DialogService> logger)
    {
        _scope = scope;
        _viewLocator = viewLocator;
        _viewModelFactory = viewModelFactory;
        _logger = logger;
    }

    // ========== SHOW DIALOG ==========

    public async Task<TResult?> ShowDialogAsync<TViewModel, TResult>()
        where TViewModel : IDialogViewModel
        where TResult : IVmResult
    {
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[DIALOG_SERVICE] Showing dialog {ViewModelType}", viewModelType.Name);

        // Create ViewModel
        var viewModel = _viewModelFactory.Create<TViewModel>();

        // Create View
        var view = _viewLocator.ResolveView<TViewModel>();

        // Show dialog and get result
        return await ShowDialogInternalAsync<TResult>(viewModel, view);
    }

    public async Task<TResult?> ShowDialogAsync<TViewModel, TOptions, TResult>(TOptions options)
        where TViewModel : IViewModel, IDialogViewModel<TOptions, TResult>
        where TOptions : IVmParameters
        where TResult : IVmResult
    {
        var viewModelType = typeof(TViewModel);

        _logger.LogInformation("[DIALOG_SERVICE] Showing dialog {ViewModelType} with options (CorrelationId: {CorrelationId})",
            viewModelType.Name, options.CorrelationId);

        // Create ViewModel with options
        var viewModel = _viewModelFactory.Create<TViewModel, TOptions>(options);

        // Create View
        var view = _viewLocator.ResolveView<TViewModel>();

        // Show dialog and get result
        return await ShowDialogInternalAsync<TResult>(viewModel, view);
    }

    // ========== COMMON DIALOGS ==========

    public Task<Core.Services.MessageBoxResult> ShowMessageBoxAsync(
        string message,
        string? title = null,
        MessageBoxType type = MessageBoxType.Information)
    {
        _logger.LogInformation("[DIALOG_SERVICE] Showing message box: {Message}", message);

        var icon = type switch
        {
            MessageBoxType.Information => MessageBoxImage.Information,
            MessageBoxType.Warning => MessageBoxImage.Warning,
            MessageBoxType.Error => MessageBoxImage.Error,
            MessageBoxType.Question => MessageBoxImage.Question,
            _ => MessageBoxImage.None
        };

        var result = System.Windows.MessageBox.Show(
            message,
            title ?? "Message",
            MessageBoxButton.OK,
            icon);

        return Task.FromResult(Core.Services.MessageBoxResult.OK);
    }

    public Task<bool> ShowConfirmationAsync(
        string message,
        string? title = null,
        string confirmText = "OK",
        string cancelText = "Cancel")
    {
        _logger.LogInformation("[DIALOG_SERVICE] Showing confirmation: {Message}", message);

        var result = System.Windows.MessageBox.Show(
            message,
            title ?? "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        return Task.FromResult(result == System.Windows.MessageBoxResult.Yes);
    }

    public Task ShowErrorAsync(string errorMessage, string? title = null)
    {
        _logger.LogError("[DIALOG_SERVICE] Showing error dialog: {ErrorMessage}", errorMessage);

        System.Windows.MessageBox.Show(
            errorMessage,
            title ?? "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        return Task.CompletedTask;
    }

    public Task<string?> ShowInputAsync(
        string prompt,
        string? title = null,
        string? defaultValue = null)
    {
        _logger.LogInformation("[DIALOG_SERVICE] Showing input dialog: {Prompt}", prompt);

        // For input dialog, you would typically create a custom dialog window
        // For now, returning default value as placeholder
        // TODO: Implement custom input dialog

        _logger.LogWarning("[DIALOG_SERVICE] ShowInputAsync not fully implemented - returning default value");
        return Task.FromResult(defaultValue);
    }

    // ========== INTERNAL METHODS ==========

    private async Task<TResult?> ShowDialogInternalAsync<TResult>(
        object viewModel,
        IView view)
        where TResult : IVmResult
    {
        // Set DataContext
        view.DataContext = viewModel;

        // Get dialog window
        var dialogView = view as IDialogView;
        if (dialogView == null)
        {
            var message = $"View {view.GetType().Name} does not implement IDialogView";
            _logger.LogError("[DIALOG_SERVICE] {Message}", message);
            throw new InvalidOperationException(message);
        }

        if (dialogView is Window window)
        {
            // Set owner to current active window
            if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsActive)
            {
                window.Owner = Application.Current.MainWindow;
            }
            else
            {
                // Find any active window
                foreach (Window w in Application.Current.Windows)
                {
                    if (w.IsActive)
                    {
                        window.Owner = w;
                        break;
                    }
                }
            }

            // Initialize ViewModel if needed
            if (viewModel is IInitializable initializable)
            {
                _logger.LogDebug("[DIALOG_SERVICE] Initializing dialog ViewModel");
                await initializable.InitializeAsync();
            }

            // Show modal dialog
            _logger.LogDebug("[DIALOG_SERVICE] Showing modal dialog");
            var dialogResult = window.ShowDialog();

            _logger.LogInformation("[DIALOG_SERVICE] Dialog closed with result: {DialogResult}", dialogResult);

            // Get typed result from ViewModel
            if (viewModel is IDialogViewModel<IVmParameters, TResult> dialogViewModel)
            {
                var result = dialogViewModel.DialogResult;
                _logger.LogDebug("[DIALOG_SERVICE] Dialog ViewModel result: {Result}", result);

                // Dispose ViewModel if needed
                if (viewModel is IDisposable disposable)
                {
                    _logger.LogDebug("[DIALOG_SERVICE] Disposing dialog ViewModel");
                    disposable.Dispose();
                }

                return result;
            }

            // Dispose ViewModel even if result extraction failed
            if (viewModel is IDisposable disposableVm)
            {
                disposableVm.Dispose();
            }
        }
        return default;
    }
}
