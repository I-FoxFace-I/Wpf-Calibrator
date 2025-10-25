using WpfEngine.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace WpfEngine.Extensions;

/// <summary>
/// Helper extensions for IServiceScope
/// </summary>
public static class ServiceScopeExtensions
{
    /// <summary>
    /// Creates and initializes ViewModel from scoped service provider
    /// </summary>
    public static async Task<TViewModel> CreateAndInitializeViewModelAsync<TViewModel>(
        this IServiceScope scope)
        where TViewModel : IViewModel
    {
        var viewModel = scope.ServiceProvider.GetRequiredService<TViewModel>();
        await viewModel.InitializeAsync();
        return viewModel;
    }

    /// <summary>
    /// Creates and initializes ViewModel with options from scoped service provider
    /// </summary>
    public static async Task<TViewModel> CreateAndInitializeViewModelAsync<TViewModel, TOptions>(
        this IServiceScope scope,
        TOptions options)
        where TViewModel : IViewModel, IViewModel<TOptions>
        where TOptions : IVmParameters
    {
        var viewModel = scope.ServiceProvider.GetRequiredService<TViewModel>();
        await viewModel.InitializeAsync(options);
        return viewModel;
    }
}

/// <summary>
/// Usage examples:
/// 
/// // In App.xaml.cs
/// var mainViewModel = await _serviceProvider.CreateAndInitializeViewModelAsync<MainViewModel>();
/// mainWindow.DataContext = mainViewModel;
/// 
/// // With options
/// var options = new CustomerDetailOptions(customerId: 5, readOnly: true);
/// var detailViewModel = await _serviceProvider.CreateAndInitializeViewModelAsync<CustomerDetailViewModel, CustomerDetailOptions>(options);
/// 
/// // Set ViewModel on view directly
/// await mainWindow.SetViewModelAsync<MainViewModel>(_serviceProvider);
/// 
/// // With scoped services
/// using var scope = _serviceProvider.CreateScope();
/// var viewModel = await scope.CreateAndInitializeViewModelAsync<CustomerListViewModel>();
/// </summary>
