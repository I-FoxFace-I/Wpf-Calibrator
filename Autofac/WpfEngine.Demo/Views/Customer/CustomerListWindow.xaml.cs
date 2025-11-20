using Autofac;
using WpfEngine.Demo.ViewModels;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WpfEngine.Demo.Views;

namespace WpfEngine.Demo.Views;

public partial class CustomerListWindow : ScopedWindow
{
    public CustomerListWindow(ILogger<CustomerListWindow> logger)
        : base(logger)
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }
    
    private async Task OnLoadedAsync()
    {
        if (DataContext is CustomerListViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}