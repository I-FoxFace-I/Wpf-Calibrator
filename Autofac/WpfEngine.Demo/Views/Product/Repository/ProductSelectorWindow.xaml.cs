using Autofac;
using WpfEngine.Demo.ViewModels.Product.Repository;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WpfEngine.Views.Windows;

namespace WpfEngine.Demo.Views.Product.Repository;

public partial class ProductSelectorWindow : WpfEngine.Views.Windows.ScopedWindow
{
    public ProductSelectorWindow(ILogger<ProductSelectorWindow> logger)
        : base(logger)
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        if (DataContext is ProductSelectorViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}
