using System.Collections.ObjectModel;
using System.Linq;
using AutofacEnhancedWpfDemo.Services.Demo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels.Demo;

// ========== STEP 3: REVIEW ==========

public partial class DemoWorkflowStep3ViewModel : BaseViewModel
{
    private readonly INavigator _navigator;
    private readonly WorkflowState _state;
    
    [ObservableProperty]
    private string _customerName = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<WorkflowOrderItem> _orderItems = new();
    
    public decimal OrderTotal => OrderItems.Sum(i => i.Total);
    
    public DemoWorkflowStep3ViewModel(
        INavigator navigator,
        ILogger<DemoWorkflowStep3ViewModel> logger,
        WorkflowState state) : base(logger)
    {
        _navigator = navigator;
        _state = state;
        CustomerName = state.CustomerName;
        
        if (state.OrderItems != null)
        {
            foreach (var item in state.OrderItems)
            {
                OrderItems.Add(item);
            }
        }
        
        Logger.LogInformation("[WORKFLOW] Step3 ViewModel created - reviewing order");
    }
    
    [RelayCommand]
    private async Task BackAsync()
    {
        Logger.LogInformation("[WORKFLOW] Going back to Step 2");
        await _navigator.NavigateBackAsync();
    }
    
    [RelayCommand]
    private void Complete()
    {
        Logger.LogInformation("[WORKFLOW] ✅ Workflow completed! Order would be created for customer {CustomerId}", 
            _state.CustomerId);
        
        // In real app, create order via CQRS here
        Logger.LogInformation("[WORKFLOW] 🎉 SUCCESS! Order with {ItemCount} items, Total: ${Total}", 
            OrderItems.Count, OrderTotal);
        
        // Could navigate back to start or close window
        // For now, just log success
    }
}
