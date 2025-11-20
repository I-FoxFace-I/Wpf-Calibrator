using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Autofac;
using Microsoft.Extensions.Logging;
using WpfEngine.Demo.ViewModels.Workflow;
using WpfEngine.Demo.ViewModels.Obsolete;

namespace WpfEngine.Demo.Views;

public partial class WorkflowHostWindow : ScopedWindow
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
        else if (DataContext is WorkflowHostViewModelRefactored vmRefactored)
        {
            await vmRefactored.InitializeAsync();
        }
    }
}
