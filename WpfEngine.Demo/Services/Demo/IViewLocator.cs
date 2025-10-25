using System;
using System.Linq;
using System.Windows;

namespace WpfEngine.Demo.Services;

/// <summary>
/// Locates Views (Windows) for ViewModels using convention-based mapping
/// </summary>
public interface IViewLocator
{
    /// <summary>
    /// Finds and creates Window for given ViewModel type
    /// Convention: ProductsViewModel -> ProductsWindow
    /// </summary>
    Window CreateWindowForViewModel<TViewModel>() where TViewModel : class;

    /// <summary>
    /// Finds and creates Window for given ViewModel type
    /// </summary>
    Window CreateWindowForViewModel(Type viewModelType);
}
