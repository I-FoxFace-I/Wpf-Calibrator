using Autofac;
using WpfEngine.Demo.ViewModels;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Demo.Views;

// ========== DEMO PRODUCT DETAIL WINDOW ==========

public partial class ProductDetailWindow : ScopedWindow
{
    public ProductDetailWindow(ILogger<ProductDetailWindow> logger) : base(logger)
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        if (DataContext is ProductDetailViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}

// ========== DEMO WORKFLOW HOST WINDOW ==========


