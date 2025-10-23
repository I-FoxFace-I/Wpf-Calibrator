using Autofac;
using AutofacEnhancedWpfDemo.ViewModels.Demo;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Views.Demo;

// ========== DEMO PRODUCT DETAIL WINDOW ==========

public partial class DemoProductDetailWindow : ScopedWindow
{
    public DemoProductDetailWindow(
        ILifetimeScope parentScope,
        ILogger<DemoProductDetailWindow> logger)
        : base(parentScope, logger, "demo-product-detail")
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        if (DataContext is DemoProductDetailViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}

// ========== DEMO WORKFLOW HOST WINDOW ==========


