using System.Collections.ObjectModel;
using AutofacEnhancedWpfDemo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels;

/// <summary>
/// ViewModel for Customers window (placeholder)
/// </summary>
//public partial class CustomersViewModel : BaseViewModel
//{
//    [ObservableProperty]
//    private ObservableCollection<Customer> _customers = new();
    
//    [ObservableProperty]
//    private Customer? _selectedCustomer;
    
//    public CustomersViewModel(ILogger<CustomersViewModel> logger) : base(logger)
//    {
//        Logger.LogInformation("CustomersViewModel created");
//    }
//}

/// <summary>
/// ViewModel for Orders window (placeholder)
/// </summary>
//public partial class OrdersViewModel : BaseViewModel
//{
//    [ObservableProperty]
//    private ObservableCollection<Order> _orders = new();
    
//    [ObservableProperty]
//    private Order? _selectedOrder;
    
//    public OrdersViewModel(ILogger<OrdersViewModel> logger) : base(logger)
//    {
//        Logger.LogInformation("OrdersViewModel created");
//    }
//}

/// <summary>
/// ViewModel for Order Detail window (placeholder)
/// </summary>
//public partial class OrderDetailViewModel : BaseViewModel
//{
//    [ObservableProperty]
//    private Order? _order;
    
//    public OrderDetailViewModel(ILogger<OrderDetailViewModel> logger) : base(logger)
//    {
//        Logger.LogInformation("OrderDetailViewModel created");
//    }
//}

/// <summary>
/// ViewModel for Order Workflow window (placeholder)
/// </summary>
//public partial class OrderWorkflowViewModel : BaseViewModel
//{
//    //[ObservableProperty]
//    //private int _currentStep = 1;
    
//    public OrderWorkflowViewModel(ILogger<OrderWorkflowViewModel> logger) : base(logger)
//    {
//        Logger.LogInformation("OrderWorkflowViewModel created");
//    }
//}

/// <summary>
/// ViewModel for Scope Hierarchy Demo window (placeholder)
/// </summary>
//public partial class ScopeHierarchyDemoViewModel : BaseViewModel
//{
//    [ObservableProperty]
//    private ObservableCollection<string> _logMessages = new();
    
//    public ScopeHierarchyDemoViewModel(ILogger<ScopeHierarchyDemoViewModel> logger) : base(logger)
//    {
//        Logger.LogInformation("ScopeHierarchyDemoViewModel created");
//        LogMessages.Add("Scope Hierarchy Demo initialized");
//    }
//}

///// <summary>
///// ViewModel for Child Demo window (placeholder)
///// </summary>
//public partial class ChildDemoViewModel : BaseViewModel
//{
//    [ObservableProperty]
//    private string _windowTitle = "Child Window";
    
//    [ObservableProperty]
//    private string _windowColor = "#3B82F6";
    
//    public ChildDemoViewModel(
//        ILogger<ChildDemoViewModel> logger,
//        ChildDemoOptions? options = null) : base(logger)
//    {
//        if (options != null)
//        {
//            WindowTitle = $"Child Window {options.ChildNumber}";
//            WindowColor = options.Color ?? "#3B82F6";
//        }
        
//        Logger.LogInformation("ChildDemoViewModel created");
//    }
//}

/// <summary>
/// Options for ChildDemoWindow
/// </summary>
public record ChildDemoOptions
{
    public int ChildNumber { get; init; }
    public string? Color { get; init; }
}
