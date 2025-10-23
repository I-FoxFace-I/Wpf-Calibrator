using System.Threading.Tasks;
using Autofac;
using AutofacEnhancedWpfDemo.ViewModels;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Views;

/// <summary>
/// Products management window
/// ViewModel resolved and set as DataContext from outside (via Navigator/ViewLocator)
/// </summary>
public partial class ProductsWindow : ScopedWindow
{
    public ProductsWindow(
        ILifetimeScope parentScope,
        ILogger<ProductsWindow> logger)
        : base(parentScope, logger, "products")
    {
        InitializeComponent();
        
        // ViewModel will be set as DataContext by Navigator
        Loaded += async (s, e) => await OnLoadedAsync();
    }
    
    private async Task OnLoadedAsync()
    {
        if (DataContext is ProductsViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}
