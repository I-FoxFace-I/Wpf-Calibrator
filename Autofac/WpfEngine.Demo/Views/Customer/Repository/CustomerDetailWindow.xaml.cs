using Autofac;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Windows;
using WpfEngine.Demo.ViewModels.Workflow;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Demo.Views;
using WpfEngine.Views.Windows;
using WpfEngine.Views;

namespace WpfEngine.Demo.Views.Customer.Repository;

public partial class CustomerDetailWindow : WpfEngine.Views.Windows.ScopedWindow
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