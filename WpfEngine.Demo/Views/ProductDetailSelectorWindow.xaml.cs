using Autofac;
using WpfEngine.Demo.ViewModels;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace WpfEngine.Demo.Views;

public partial class ProductDetailSelectorWindow : ScopedWindow
{
    public ProductDetailSelectorWindow(
        ILifetimeScope parentScope,
        ILogger<ProductDetailSelectorWindow> logger)
        : base(parentScope, logger, "product-detail-selector")
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        if (DataContext is ProductDetailSelectorViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}

