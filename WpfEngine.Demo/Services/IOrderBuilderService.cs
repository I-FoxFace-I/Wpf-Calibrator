using System;
using System.Collections.Generic;
using System.Linq;
using WpfEngine.Demo.ViewModels;

namespace WpfEngine.Demo.Services;

/// <summary>
/// Shared service for building orders across workflow windows
/// Demonstrates session-scoped service sharing
/// Registered as InstancePerMatchingLifetimeScope("workflow-session-*")
/// </summary>
public interface IOrderBuilderService
{
    /// <summary>
    /// Current customer ID
    /// </summary>
    int? CustomerId { get; set; }
    
    /// <summary>
    /// Current customer name
    /// </summary>
    string? CustomerName { get; set; }
    
    /// <summary>
    /// Order items being built
    /// </summary>
    List<WorkflowOrderItem> OrderItems { get; }
    
    /// <summary>
    /// Selected shipping address ID
    /// </summary>
    int? ShippingAddressId { get; set; }
    
    /// <summary>
    /// Adds item to order
    /// </summary>
    void AddItem(int productId, string productName, decimal unitPrice, int quantity);
    
    /// <summary>
    /// Removes item from order
    /// </summary>
    void RemoveItem(WorkflowOrderItem item);
    
    /// <summary>
    /// Clears all order data
    /// </summary>
    void Clear();
    
    /// <summary>
    /// Gets order total
    /// </summary>
    decimal GetTotal();
    
    /// <summary>
    /// Validates that order can be completed
    /// </summary>
    bool CanComplete();
}

/// <summary>
/// Implementation of order builder service
/// </summary>
public class OrderBuilderService : IOrderBuilderService
{
    private readonly List<WorkflowOrderItem> _orderItems = new();

    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? ShippingAddressId { get; set; }

    public List<WorkflowOrderItem> OrderItems => _orderItems;

    public void AddItem(int productId, string productName, decimal unitPrice, int quantity)
    {
        var existingItem = _orderItems.FirstOrDefault(i => i.ProductId == productId);
        
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            _orderItems.Add(new WorkflowOrderItem
            {
                ProductId = productId,
                ProductName = productName,
                UnitPrice = unitPrice,
                Quantity = quantity
            });
        }
    }

    public void RemoveItem(WorkflowOrderItem item)
    {
        _orderItems.Remove(item);
    }

    public void Clear()
    {
        CustomerId = null;
        CustomerName = null;
        ShippingAddressId = null;
        _orderItems.Clear();
    }

    public decimal GetTotal()
    {
        return _orderItems.Sum(i => i.Total);
    }

    public bool CanComplete()
    {
        return CustomerId.HasValue && 
               ShippingAddressId.HasValue && 
               _orderItems.Any();
    }
}

