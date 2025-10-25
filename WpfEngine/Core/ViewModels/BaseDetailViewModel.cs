using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;
using WpfEngine.Services.WindowTracking;

namespace WpfEngine.Core.ViewModels;

// ========== DETAIL VIEWMODEL ==========

/// <summary>
/// Base ViewModel for detail views (read-only)
/// </summary>
public abstract partial class BaseDetailViewModel<T> : BaseViewModel, IDetailViewModel<T>
{
    private readonly IWindowService? _windowService;

    [ObservableProperty]
    private T? _entity;

    [ObservableProperty]
    private bool _isReadOnly = true;

    protected BaseDetailViewModel(ILogger logger, IWindowService? windowService = null) : base(logger)
    {
        _windowService = windowService;
    }

    /// <summary>
    /// Closes the detail view
    /// </summary>
    public virtual void Close()
    {
        if (_windowService != null)
        {
            _windowService.Close(this.GetVmKey());
        }
        else
        {
            Logger.LogWarning("[{ViewModelType}] Cannot close - IWindowService not available", GetType().Name);
        }
    }
}
