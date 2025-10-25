using System;
using System.Threading.Tasks;
using WpfEngine.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace WpfEngine.Extensions;

/// <summary>
/// Helper extensions for working with ViewModels in Microsoft DI
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Creates and initializes ViewModel from service provider
    /// </summary>
    public static async Task<TViewModel> CreateAndInitializeViewModelAsync<TViewModel>(
        this IServiceProvider serviceProvider)
        where TViewModel : class, IViewModel
    {
        var viewModel = serviceProvider.GetRequiredService<TViewModel>();
        await viewModel.InitializeAsync();
        return viewModel;
    }

    /// <summary>
    /// Creates and initializes ViewModel with options from service provider
    /// </summary>
    public static async Task<TViewModel> CreateAndInitializeViewModelAsync<TViewModel, TOptions>(
        this IServiceProvider serviceProvider,
        TOptions options)
        where TViewModel : IViewModel, IViewModel<TOptions>
        where TOptions : IVmParameters
    {
        var viewModel = serviceProvider.GetRequiredService<TViewModel>();
        await viewModel.InitializeAsync(options);
        return viewModel;
    }

    /// <summary>
    /// Creates ViewModel and sets it as DataContext for view, then initializes
    /// </summary>
    public static async Task SetViewModelAsync<TViewModel>(
        this object view,
        IServiceProvider serviceProvider)
        where TViewModel : class, IViewModel
    {
        var viewModel = await serviceProvider.CreateAndInitializeViewModelAsync<TViewModel>();
        
        var viewType = view.GetType();
        var dataContextProperty = viewType.GetProperty("DataContext");
        dataContextProperty?.SetValue(view, viewModel);
    }

    /// <summary>
    /// Creates ViewModel with options and sets it as DataContext for view, then initializes
    /// </summary>
    public static async Task SetViewModelAsync<TViewModel, TOptions>(
        this object view,
        IServiceProvider serviceProvider,
        TOptions options)
        where TViewModel : class, IViewModel<TOptions>
        where TOptions : IVmParameters
    {
        var viewModel = await serviceProvider.CreateAndInitializeViewModelAsync<TViewModel, TOptions>(options);
        
        var viewType = view.GetType();
        var dataContextProperty = viewType.GetProperty("DataContext");
        dataContextProperty?.SetValue(view, viewModel);
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
