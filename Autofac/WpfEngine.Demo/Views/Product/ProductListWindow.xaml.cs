using Autofac;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Demo.Views;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Demo.Views;
public partial class ProductListWindow : ScopedWindow
{
    public ProductListWindow(ILogger<ProductListWindow> logger)
        : base(logger)
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        if (DataContext is ProductListViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}