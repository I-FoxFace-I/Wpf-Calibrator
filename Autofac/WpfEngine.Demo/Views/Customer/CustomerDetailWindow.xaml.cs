using Autofac;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Demo.Views;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Demo.Views;
public partial class CustomerDetailWindow : ScopedWindow
{
    public CustomerDetailWindow(ILogger<CustomerDetailWindow> logger)
        : base(logger)
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        if (DataContext is CustomerDetailViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}