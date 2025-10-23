using Autofac;
using AutofacEnhancedWpfDemo.Views;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Views.Demo;
public partial class AdvancedDemoMenuWindow : ScopedWindow
{
    public AdvancedDemoMenuWindow(
        ILifetimeScope parentScope,
        ILogger<AdvancedDemoMenuWindow> logger)
        : base(parentScope, logger, "advanced-demo-menu")
    {
        InitializeComponent();
    }
}