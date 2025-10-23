using Autofac;
using AutofacEnhancedWpfDemo.ViewModels.Demo;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AutofacEnhancedWpfDemo.Views.Demo;

public partial class DemoCustomerListWindow : ScopedWindow
{
    public DemoCustomerListWindow(
        ILifetimeScope parentScope,
        ILogger<DemoCustomerListWindow> logger)
        : base(parentScope, logger, "demo-customer-list")
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }
    private async Task OnLoadedAsync()
    {
        if (DataContext is DemoCustomerListViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}