using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using WpfEngine.ViewModels;
using WpfEngine.Data.Parameters;
using WpfEngine.ViewModels.Base;

namespace WpfEngine.ViewModels.Elements;

// ========== DETAIL VIEWMODEL ==========

/// <summary>
/// Base ViewModel for detail/edit views
/// </summary>
public abstract partial class DetailViewModel<T> : BaseViewModel, IElementViewModel<T>
{
    public virtual T? Entity { get; protected set; }
    public virtual bool IsReadOnly { get; protected set; }

    protected DetailViewModel(ILogger<DetailViewModel<T>> logger, BaseItemParameters? parameters) : base(logger)
    {
        IsReadOnly = parameters?.ReadOnly ?? true;
    }

    public abstract Task SaveAsync();
    public abstract void Close();
}
