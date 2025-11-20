using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using WpfEngine.ViewModels;
using WpfEngine.ViewModels.Base;

namespace WpfEngine.ViewModels.Elements;

// ========== LIST VIEWMODEL ==========


/// <summary>
/// Base ViewModel for lists with IDisposable support
/// </summary>
public abstract partial class ListingViewModel<T> : BaseViewModel, IListingViewModel<T>
{
    [ObservableProperty]
    private T? _selectedItem;

    private bool _disposed;

    protected ListingViewModel(ILogger<ListingViewModel<T>> logger) : base(logger)
    {
    }

    public abstract IEnumerable<T> Items { get; }

    public abstract Task RefreshAsync(CancellationToken cancellationToken = default);
    public abstract Task RefreshAsync(object id, CancellationToken cancellationToken = default);

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
