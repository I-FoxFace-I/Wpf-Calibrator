using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using WpfEngine.Demo.ViewModels;

namespace WpfEngine.Demo.Services;

/// <summary>
/// Order builder service - shared state for workflow
/// Registered as InstancePerMatchingLifetimeScope("WorkflowSession:*")
/// All windows in same workflow session see the SAME instance
/// </summary>
public class OrderBuilderService : IOrderBuilderService
{
    private readonly ILogger<OrderBuilderService> _logger;
    private readonly ObservableCollection<WorkflowOrderItem> _orderItems = new();

    public OrderBuilderService(ILogger<OrderBuilderService> logger)
    {
        _logger = logger;
        _logger.LogInformation("[ORDER_BUILDER] Service created - will be shared across workflow session");
    }

    // ========== CUSTOMER ==========

    public int? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    // ========== ORDER ITEMS ==========

    public ObservableCollection<WorkflowOrderItem> OrderItems => _orderItems;

    public void AddItem(int productId, string productName, decimal unitPrice, int quantity)
    {
        var existingItem = _orderItems.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            _logger.LogInformation("[ORDER_BUILDER] Updated quantity for {Product} to {Quantity}",
                productName, existingItem.Quantity);
        }
        else
        {
            var item = new WorkflowOrderItem
            {
                ProductId = productId,
                ProductName = productName,
                UnitPrice = unitPrice,
                Quantity = quantity
            };
            
            _orderItems.Add(item);
            _logger.LogInformation("[ORDER_BUILDER] Added {Quantity}x {Product} to order",
                quantity, productName);
        }

        OrderItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveItem(WorkflowOrderItem item)
    {
        if (_orderItems.Remove(item))
        {
            _logger.LogInformation("[ORDER_BUILDER] Removed {Product} from order", item.ProductName);
            OrderItemsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void UpdateItemQuantity(int productId, int newQuantity)
    {
        var item = _orderItems.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.Quantity = newQuantity;
            _logger.LogInformation("[ORDER_BUILDER] Updated {Product} quantity to {Quantity}",
                item.ProductName, newQuantity);
            OrderItemsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ClearItems()
    {
        _orderItems.Clear();
        _logger.LogInformation("[ORDER_BUILDER] Cleared all items");
        OrderItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    // ========== SHIPPING ADDRESS ==========

    public int? ShippingAddressId { get; set; }

    // ========== TOTALS ==========

    public decimal Subtotal => _orderItems.Sum(i => i.Total);
    public decimal Tax => Subtotal * 0.21m;
    public decimal Total => Subtotal + Tax;

    // ========== VALIDATION ==========

    public bool IsValid()
    {
        return CustomerId.HasValue && _orderItems.Any();
    }

    // ========== EVENTS ==========

    public event EventHandler? OrderItemsChanged;
}

