using Autofac;
using WpfEngine.Demo.Views;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Demo.Views;
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