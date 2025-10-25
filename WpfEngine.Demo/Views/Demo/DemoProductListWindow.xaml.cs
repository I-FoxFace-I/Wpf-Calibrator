using Autofac;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Demo.Views;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Demo.Views;
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