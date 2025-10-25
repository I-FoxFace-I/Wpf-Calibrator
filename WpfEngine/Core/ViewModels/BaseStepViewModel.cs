using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Core.ViewModels;

// ========== STEP VIEWMODEL ==========

/// <summary>
/// Base ViewModel for workflow steps with IDisposable
/// </summary>
public abstract partial class BaseStepViewModel : BaseViewModel, IStepViewModel
{
    [ObservableProperty]
    private bool _canNavigateNext;

    [ObservableProperty]
    private bool _canNavigateBack = true;

    private bool _disposed;

    protected BaseStepViewModel(ILogger logger) : base(logger)
    {
    }

    /// <summary>
    /// Saves current step data
    /// </summary>
    public abstract Task SaveAsync();

    /// <summary>
    /// Validates current step before navigation
    /// </summary>
    public virtual Task<bool> ValidateStepAsync()
    {
        return Task.FromResult(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Logger.LogDebug("[{ViewModelType}] Disposing step", GetType().Name);
                // Dispose managed resources
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
