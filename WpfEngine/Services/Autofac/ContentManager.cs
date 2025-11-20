using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using WpfEngine.Abstract;
using WpfEngine.Data.Abstract;
using WpfEngine.Services;
using WpfEngine.ViewModels;

namespace WpfEngine.Services.Autofac;

/// <summary>
/// Lightweight Content Manager - only creation and disposal
/// No tracking, no activation, no caching - just a thin wrapper over DI
/// 
/// Use this when:
/// - You want Navigator to be DI-agnostic
/// - You don't need content tracking/metadata
/// - You manage activation yourself (e.g., in Navigator)
/// - You want minimal overhead
/// </summary>
public class ContentManager : IContentManager, IDisposable
{
    private readonly ILifetimeScope _scope;
    private readonly ILogger<ContentManager> _logger;
    private bool _disposed;

    public ContentManager(
        ILifetimeScope scope, 
        ILogger<ContentManager> logger)
    {
        _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _logger.LogDebug("[LIGHT_CONTENT_MANAGER] Service created");
    }

    public async Task<IViewModel> CreateContentAsync(
        Type viewModelType,
        IViewModelParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        if (viewModelType == null) 
            throw new ArgumentNullException(nameof(viewModelType));
            
        if (!typeof(IViewModel).IsAssignableFrom(viewModelType))
        {
            throw new ArgumentException(
                $"Type {viewModelType.Name} must implement IViewModel", 
                nameof(viewModelType));
        }

        _logger.LogDebug(
            "[LIGHT_CONTENT_MANAGER] Creating {Type}", 
            viewModelType.Name);

        try
        {
            // Resolve from DI container
            IViewModel viewModel;
            if (parameters != null)
            {
                viewModel = (IViewModel)_scope.Resolve(viewModelType, 
                    new TypedParameter(parameters.GetType(), parameters));
            }
            else
            {
                viewModel = (IViewModel)_scope.Resolve(viewModelType);
            }

            // Initialize if supported
            await InitializeAsync(viewModel, parameters, cancellationToken);

            _logger.LogInformation(
                "[LIGHT_CONTENT_MANAGER] Created {Type} with ID {Id}", 
                viewModelType.Name, 
                viewModel.ViewModelId);

            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "[LIGHT_CONTENT_MANAGER] Failed to create {Type}", 
                viewModelType.Name);
            throw;
        }
    }

    public async Task<TViewModel> CreateContentAsync<TViewModel>(
        IViewModelParameters? parameters = null,
        CancellationToken cancellationToken = default) 
        where TViewModel : IViewModel
    {
        var viewModel = await CreateContentAsync(
            typeof(TViewModel), 
            parameters, 
            cancellationToken);
            
        return (TViewModel)viewModel;
    }

    public Task DisposeContentAsync(IViewModel viewModel)
    {
        ThrowIfDisposed();
        
        if (viewModel == null) 
            throw new ArgumentNullException(nameof(viewModel));

        _logger.LogDebug(
            "[LIGHT_CONTENT_MANAGER] Disposing {Type} (ID: {Id})", 
            viewModel.GetType().Name,
            viewModel.ViewModelId);

        try
        {
            // Dispose if IDisposable
            if (viewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
            
            _logger.LogInformation(
                "[LIGHT_CONTENT_MANAGER] Disposed {Type} (ID: {Id})", 
                viewModel.GetType().Name,
                viewModel.ViewModelId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "[LIGHT_CONTENT_MANAGER] Error disposing {Type}", 
                viewModel.GetType().Name);
        }

        return Task.CompletedTask;
    }

    // ========== PRIVATE METHODS ==========

    private async Task InitializeAsync(
        IViewModel viewModel,
        IViewModelParameters? parameters,
        CancellationToken cancellationToken)
    {
        // Try initialize with parameters first
        if (parameters != null && viewModel is IInitializable<IViewModelParameters> initWithParams)
        {
            await initWithParams.InitializeAsync(parameters);
            return;
        }
        
        // Fall back to parameterless initialization
        if (viewModel is IInitializable init)
        {
            await init.InitializeAsync();
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ContentManager));
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _logger.LogDebug("[LIGHT_CONTENT_MANAGER] Disposed");
    }
}

