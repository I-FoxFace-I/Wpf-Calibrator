using Autofac;
using AutofacEnhancedWpfDemo.ViewModels;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Views;


// ========== ScopeHierarchyDemoWindow ==========
public partial class ScopeHierarchyDemoWindow : ScopedWindow
{
    public ScopeHierarchyDemoWindow(
        ILifetimeScope parentScope,
        ILogger<ScopeHierarchyDemoWindow> logger)
        : base(parentScope, logger, "scope-hierarchy-demo")
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        if (DataContext is ScopeHierarchyDemoViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}
