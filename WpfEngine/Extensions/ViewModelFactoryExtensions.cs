using System;
using System.Threading.Tasks;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Services.MicrosoftDI;

/// <summary>
/// Helper extensions for async initialization
/// </summary>
public static class ViewModelFactoryExtensions
{
    /// <summary>
    /// Creates and initializes ViewModel
    /// </summary>
    public static async Task<TViewModel> CreateAndInitializeAsync<TViewModel>(
        this IViewModelFactory factory)
        where TViewModel : class, IViewModel
    {
        var vm = factory.Create<TViewModel>();
        await vm.InitializeAsync();
        return vm;
    }

    /// <summary>
    /// Creates and initializes ViewModel with options
    /// </summary>
    public static async Task<TViewModel> CreateAndInitializeAsync<TViewModel, TOptions>(
        this IViewModelFactory factory,
        TOptions options)
        where TViewModel : IViewModel, IViewModel<TOptions>
        where TOptions : IVmParameters
    {
        var vm = factory.Create<TViewModel, TOptions>(options);
        await vm.InitializeAsync(options);
        return vm;
    }
}
