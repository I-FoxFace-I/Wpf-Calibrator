using Autofac;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Demo.Views;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Demo.Views;
public partial class DemoCustomerDetailWindow : ScopedWindow
{
    public DemoCustomerDetailWindow(
        ILifetimeScope parentScope,
        ILogger<DemoCustomerDetailWindow> logger)
        : base(parentScope, logger, "demo-customer-detail")
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        if (DataContext is DemoCustomerDetailViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}