using System.Reflection;
using System.Windows;
using Autofac;

namespace WpfEngine.Demo.Services;

/// <summary>
/// Convention-based View locator
/// Maps ViewModels to Windows using naming convention
/// Supports both main app and demo namespaces
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
        // Also supports: DemoProductsViewModel -> DemoProductsWindow (in Demo namespace)
        var vmTypeName = viewModelType.Name;

        if (!vmTypeName.EndsWith("ViewModel"))
            throw new InvalidOperationException($"Type {vmTypeName} doesn't follow ViewModel naming convention");

        var windowTypeName = vmTypeName.Replace("ViewModel", "Window");

        // Check if this is a Demo ViewModel
        var isDemo = viewModelType.Namespace?.Contains(".Demo") ?? false;

        // Find window type in assembly
        var windowType = Assembly.GetExecutingAssembly()
            .GetTypes()
            .FirstOrDefault(t =>
            {
                if (t.Name != windowTypeName || !typeof(Window).IsAssignableFrom(t))
                    return false;

                // If Demo VM, look for Demo Window
                if (isDemo)
                    return t.Namespace?.Contains(".Demo") ?? false;

                // Otherwise, look for non-Demo Window
                return !(t.Namespace?.Contains(".Demo") ?? false);
            });

        if (windowType == null)
            throw new InvalidOperationException($"Window '{windowTypeName}' not found for ViewModel '{vmTypeName}'");

        // Resolve window from container (ScopedWindow with its own scope)
        var window = (Window)_scope.Resolve(windowType);

        return window;
    }
}
