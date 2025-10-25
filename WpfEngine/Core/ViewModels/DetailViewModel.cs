using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Core.ViewModels;

// ========== DETAIL VIEWMODEL ==========

/// <summary>
/// Base ViewModel for detail/edit views
/// </summary>
public abstract partial class DetailViewModel<T> : BaseViewModel, IDetailViewModel<T>
{
    [ObservableProperty]
    private T? _entity;

    [ObservableProperty]
    private bool _isReadOnly;

    protected DetailViewModel(ILogger logger) : base(logger)
    {
    }

    public abstract Task SaveAsync();
    public abstract void Close();
}
