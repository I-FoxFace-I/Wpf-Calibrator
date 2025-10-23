using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Services.Demo;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels.Demo;

/// <summary>
/// Workflow Host ViewModel - Shell pattern
/// Exposes Navigator's CurrentViewModel for content binding
/// </summary>
public partial class DemoWorkflowHostViewModel : BaseViewModel, IAsyncInitializable
{
    private readonly INavigator _navigator;
    
    /// <summary>
    /// Current workflow step ViewModel from Navigator
    /// </summary>
    public object? CurrentContent => _navigator.CurrentViewModel;
    
    public DemoWorkflowHostViewModel(
        INavigator navigator,
        ILogger<DemoWorkflowHostViewModel> logger) : base(logger)
    {
        _navigator = navigator;
        
        // Subscribe to Navigator's PropertyChanged
        _navigator.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(INavigator.CurrentViewModel))
            {
                OnPropertyChanged(nameof(CurrentContent));
                Logger.LogInformation("[WORKFLOW] Content changed to {Type}", 
                    _navigator.CurrentViewModel?.GetType().Name ?? "null");
            }
        };
    }
    
    public async Task InitializeAsync()
    {
        Logger.LogInformation("[WORKFLOW] Starting workflow - navigating to Step 1");
        await _navigator.NavigateToAsync<DemoWorkflowStep1ViewModel>();
    }
}
