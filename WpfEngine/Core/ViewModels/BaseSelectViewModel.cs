using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;

namespace WpfEngine.Core.ViewModels;

// ========== SELECT VIEWMODEL ==========

/// <summary>
/// Base ViewModel for selecting entities
/// </summary>
public abstract partial class BaseSelectViewModel<T> : BaseDetailViewModel<T>, ISelectViewModel<T>
{
    [ObservableProperty]
    private bool _isSelected;

    public event EventHandler<T?>? EntitySelected;

    protected BaseSelectViewModel(ILogger logger, IWindowService? windowService = null) 
        : base(logger, windowService)
    {
    }

    /// <summary>
    /// Performs the selection
    /// </summary>
    public virtual void Select()
    {
        IsSelected = true;
        Logger.LogInformation("[{ViewModelType}] Entity selected", GetType().Name);
        
        EntitySelected?.Invoke(this, Entity);
        Close();
    }
}
