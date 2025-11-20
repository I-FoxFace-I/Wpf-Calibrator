using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using WpfEngine.ViewModels;
using WpfEngine.Services;
using WpfEngine.ViewModels.Base;

namespace WpfEngine.ViewModels.Managed;

// ========== STEP VIEWMODEL ==========

/// <summary>
/// Base ViewModel for workflow steps with IDisposable
/// </summary>
public abstract partial class StepViewModel : BaseViewModel, IStepViewModel
{
    private readonly INavigator _navigator;

    private bool _disposed;

    protected INavigator Navigator => _navigator;
    public virtual bool CanNavigateNext { get; }
    public virtual bool CanNavigateBack { get; }

    protected StepViewModel(ILogger<StepViewModel> logger, INavigator navigator) : base(logger)
    {
        _navigator = navigator;
    }

    /// <summary>
    /// Saves current step data
    /// </summary>
    public abstract Task SaveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates current step before navigation
    /// </summary>
    public abstract Task<bool> ValidateStepAsync(CancellationToken cancellationToken = default);
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
