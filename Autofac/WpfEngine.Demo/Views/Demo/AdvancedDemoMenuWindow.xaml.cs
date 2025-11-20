using Autofac;
using WpfEngine.Demo.Views;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Demo.Views;
public partial class AdvancedDemoMenuWindow : ScopedWindow
{
    public AdvancedDemoMenuWindow(ILogger<AdvancedDemoMenuWindow> logger)
        : base(logger)
    {
        InitializeComponent();
    }
}