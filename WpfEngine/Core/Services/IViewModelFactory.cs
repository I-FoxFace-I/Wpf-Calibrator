using System;
using Autofac;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Services;

/// <summary>
/// Factory for creating ViewModels with DI and parameters
/// </summary>
public interface IViewModelFactory
{
    /// <summary>
    /// Creates ViewModel without parameters
    /// </summary>
    TViewModel Create<TViewModel>() where TViewModel : IViewModel;

    /// <summary>
    /// Creates ViewModel with options
    /// </summary>
    TViewModel Create<TViewModel, TOptions>(TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters;

    ///// <summary>
    ///// Creates ViewModel within specific scope
    ///// </summary>
    //TViewModel CreateScoped<TViewModel>(ILifetimeScope scope)
    //    where TViewModel : class;

    ///// <summary>
    ///// Creates ViewModel with options within specific scope
    ///// </summary>
    //TViewModel CreateScoped<TViewModel, TOptions>(ILifetimeScope scope, TOptions options)
    //    where TViewModel : class
    //    where TOptions : ViewModelOptions;

    /// <summary>
    /// Creates ViewModel with non-generic options (for dynamic scenarios)
    /// </summary>
    object Create(Type viewModelType, ViewModelOptions? options = null);

    ///// <summary>
    ///// Creates ViewModel with non-generic options within scope
    ///// </summary>
    //object CreateScoped(Type viewModelType, ILifetimeScope scope, ViewModelOptions? options = null);
}
