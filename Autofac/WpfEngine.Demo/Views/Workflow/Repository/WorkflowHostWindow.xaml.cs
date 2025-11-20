using Autofac;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Windows;
using WpfEngine.Demo.ViewModels.Workflow;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Demo.Views;
using WpfEngine.Views.Windows;
using WpfEngine.Views;
using WpfEngine.Demo.ViewModels.Workflow.Repository;

namespace WpfEngine.Demo.Views.Workflow.Repository;

public partial class WorkflowHostWindow : WpfEngine.Views.Windows.ScopedWindow
{
    public WorkflowHostWindow(ILogger<WorkflowHostWindow> logger)
        : base(logger)  // Tag matches InstancePerMatchingLifetimeScope
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        if (DataContext is WorkflowHostViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}