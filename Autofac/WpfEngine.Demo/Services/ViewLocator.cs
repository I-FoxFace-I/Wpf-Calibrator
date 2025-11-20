using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using Autofac;

namespace WpfEngine.Services;

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

/// <summary>
/// Convention-based View locator
/// Maps ViewModels to Windows using naming convention
/// </summary>
public class ViewLocator : IViewLocator
{
    private readonly ILifetimeScope _scope;
    
    public ViewLocator(ILifetimeScope scope)
    {
        _scope = scope;
    }
    
    public Window CreateWindowForViewModel<TViewModel>() where TViewModel : class
    {
        return CreateWindowForViewModel(typeof(TViewModel));
    }
    
    public Window CreateWindowForViewModel(Type viewModelType)
    {
        // Convention: ProductsViewModel -> ProductsWindow
        var vmTypeName = viewModelType.Name;
        
        if (!vmTypeName.EndsWith("ViewModel"))
            throw new InvalidOperationException($"Type {vmTypeName} doesn't follow ViewModel naming convention");
        
        var windowTypeName = vmTypeName.Replace("ViewModel", "Window");
        
        // Find window type in assembly
        var windowType = Assembly.GetExecutingAssembly()
            .GetTypes()
            .FirstOrDefault(t => t.Name == windowTypeName && typeof(Window).IsAssignableFrom(t));
        
        if (windowType == null)
            throw new InvalidOperationException($"Window '{windowTypeName}' not found for ViewModel '{vmTypeName}'");
        
        // Resolve window from container (ScopedWindow with its own scope)
        var window = (Window)_scope.Resolve(windowType);
        
        return window;
    }
}
