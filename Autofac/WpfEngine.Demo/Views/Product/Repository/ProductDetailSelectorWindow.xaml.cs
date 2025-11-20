using Autofac;
using Microsoft.Extensions.Logging;
using WpfEngine.Demo.ViewModels.Product.Repository;
using WpfEngine.Views.Windows;

namespace WpfEngine.Demo.Views.Product.Repository;

public partial class ProductDetailSelectorWindow : WpfEngine.Views.Windows.ScopedWindow
{
    public ProductDetailSelectorWindow(ILogger<ProductDetailSelectorWindow> logger)
        : base(logger)
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