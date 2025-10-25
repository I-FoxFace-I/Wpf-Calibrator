using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Core.ViewModels;

// ========== LIST VIEWMODEL ==========


/// <summary>
/// Base ViewModel for lists with IDisposable support
/// </summary>
public abstract partial class BaseListingViewModel<T> : BaseViewModel, IListingViewModel<T>
{
    [ObservableProperty]
    private T? _selectedItem;

    private bool _disposed;

    protected BaseListingViewModel(ILogger logger) : base(logger)
    {
    }

    public abstract IEnumerable<T> Items { get; }

    public abstract Task RefreshAsync();
    public abstract Task RefreshAsync(object id);

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Logger.LogDebug("[{ViewModelType}] Disposing", GetType().Name);
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
