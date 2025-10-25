using System;
using System.Collections.ObjectModel;
using WpfEngine.Demo.ViewModels;

namespace WpfEngine.Demo.Services;

/// <summary>
/// Service for building order in workflow
/// Shared across all workflow windows via InstancePerMatchingLifetimeScope
/// </summary>
public interface IOrderBuilderService
{
    // ========== CUSTOMER ==========
    
    int? CustomerId { get; set; }
    string CustomerName { get; set; }
    
    // ========== ORDER ITEMS ==========
    
    ObservableCollection<WorkflowOrderItem> OrderItems { get; }
    
    void AddItem(int productId, string productName, decimal unitPrice, int quantity);
    void RemoveItem(WorkflowOrderItem item);
    void UpdateItemQuantity(int productId, int newQuantity);
    void ClearItems();
    
    // ========== SHIPPING ADDRESS ==========
    
    int? ShippingAddressId { get; set; }
    
    // ========== TOTALS ==========
    
    decimal Subtotal { get; }
    decimal Tax { get; }
    decimal Total { get; }
    
    // ========== VALIDATION ==========
    
    bool IsValid();
    
    // ========== EVENTS ==========
    
    event EventHandler? OrderItemsChanged;
}

