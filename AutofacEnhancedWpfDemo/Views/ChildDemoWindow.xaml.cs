using Autofac;
using AutofacEnhancedWpfDemo.ViewModels;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Views;


// ========== ChildDemoWindow ==========
public partial class ChildDemoWindow : ScopedWindow
{
    public ChildDemoWindow(
        ILifetimeScope parentScope,
        ILogger<ChildDemoWindow> logger)
        : base(parentScope, logger, "child-demo")
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        if (DataContext is ChildDemoViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}