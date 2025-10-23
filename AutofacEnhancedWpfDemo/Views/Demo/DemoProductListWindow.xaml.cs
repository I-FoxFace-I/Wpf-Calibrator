using Autofac;
using AutofacEnhancedWpfDemo.ViewModels.Demo;
using AutofacEnhancedWpfDemo.Views;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Views.Demo;
public partial class DemoProductListWindow : ScopedWindow
{
    public DemoProductListWindow(
        ILifetimeScope parentScope,
        ILogger<DemoProductListWindow> logger)
        : base(parentScope, logger, "demo-product-list")
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        if (DataContext is DemoProductListViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}