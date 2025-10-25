using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Autofac;
using WpfEngine.Demo.ViewModels;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Demo.Views;

public partial class DemoWorkflowHostWindow : ScopedWindow
{
    public DemoWorkflowHostWindow(
        ILifetimeScope parentScope,
        ILogger<DemoWorkflowHostWindow> logger)
        : base(parentScope, logger, "demo-workflow-host")
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        if (DataContext is DemoWorkflowHostViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}
