using System;
using System.Threading;
using System.Threading.Tasks;
using WpfEngine.Data.Abstract;
using WpfEngine.ViewModels;

namespace WpfEngine.Services;

/// <summary>
/// Lightweight factory for creating and disposing ViewModels
/// Abstracts DI container from Navigator - minimal interface
/// 
/// Purpose:
/// - Decouple Navigator from specific DI implementation (Autofac)
/// - Simple create/dispose operations only
/// - No tracking, no caching, no events
/// </summary>
public interface IContentManager
{
    /// <summary>
    /// Create and initialize ViewModel by type
    /// </summary>
    /// <param name="viewModelType">Type of ViewModel to create</param>
    /// <param name="parameters">Optional initialization parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created and initialized ViewModel</returns>
    Task<IViewModel> CreateContentAsync(
        Type viewModelType, 
        IViewModelParameters? parameters = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create and initialize typed ViewModel
    /// </summary>
    /// <typeparam name="TViewModel">Type of ViewModel to create</typeparam>
    /// <param name="parameters">Optional initialization parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created and initialized ViewModel</returns>
    Task<TViewModel> CreateContentAsync<TViewModel>(
        IViewModelParameters? parameters = null,
        CancellationToken cancellationToken = default) 
        where TViewModel : IViewModel;
    
    /// <summary>
    /// Dispose ViewModel properly
    /// </summary>
    /// <param name="viewModel">ViewModel to dispose</param>
    Task DisposeContentAsync(IViewModel viewModel);
}

