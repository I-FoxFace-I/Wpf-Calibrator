using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Data;
using AutofacEnhancedWpfDemo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Application.Orders;

// ========== QUERIES ==========

public record GetAllOrdersQuery : IQuery<List<Order>>;

public record GetOrderByIdQuery(int Id) : IQuery<Order?>;

public record GetOrdersByCustomerQuery(int CustomerId) : IQuery<List<Order>>;

// ========== COMMANDS ==========

public record CreateOrderCommand(int CustomerId, List<OrderItemDto> Items) : ICommand;

public record DeleteOrderCommand(int Id) : ICommand;

// ========== DTOs ==========

public record OrderItemDto
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}

// ========== QUERY HANDLERS ==========

public class GetAllOrdersHandler : IQueryHandler<GetAllOrdersQuery, List<Order>>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<GetAllOrdersHandler> _logger;
    
    public GetAllOrdersHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<GetAllOrdersHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task<List<Order>> HandleAsync(GetAllOrdersQuery query)
    {
        _logger.LogInformation("Getting all orders");
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }
}

public class GetOrderByIdHandler : IQueryHandler<GetOrderByIdQuery, Order?>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<GetOrderByIdHandler> _logger;
    
    public GetOrderByIdHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<GetOrderByIdHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task<Order?> HandleAsync(GetOrderByIdQuery query)
    {
        _logger.LogInformation("Getting order {OrderId}", query.Id);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == query.Id);
    }
}

public class GetOrdersByCustomerHandler : IQueryHandler<GetOrdersByCustomerQuery, List<Order>>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<GetOrdersByCustomerHandler> _logger;
    
    public GetOrdersByCustomerHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<GetOrdersByCustomerHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task<List<Order>> HandleAsync(GetOrdersByCustomerQuery query)
    {
        _logger.LogInformation("Getting orders for customer {CustomerId}", query.CustomerId);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Where(o => o.CustomerId == query.CustomerId)
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }
}

// ========== COMMAND HANDLERS ==========

public class CreateOrderHandler : ICommandHandler<CreateOrderCommand>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<CreateOrderHandler> _logger;
    
    public CreateOrderHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<CreateOrderHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(CreateOrderCommand command)
    {
        _logger.LogInformation("Creating order for customer {CustomerId}", command.CustomerId);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var order = new Order
        {
            CustomerId = command.CustomerId,
            OrderDate = DateTime.Now,
            Items = new List<OrderItem>()
        };
        
        foreach (var itemDto in command.Items)
        {
            var product = await context.Products.FindAsync(itemDto.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found", itemDto.ProductId);
                continue;
            }
            
            order.Items.Add(new OrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price
            });
        }
        
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        
        _logger.LogInformation("Order created with ID: {OrderId}", order.Id);
    }
}

public class DeleteOrderHandler : ICommandHandler<DeleteOrderCommand>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<DeleteOrderHandler> _logger;
    
    public DeleteOrderHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<DeleteOrderHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(DeleteOrderCommand command)
    {
        _logger.LogInformation("Deleting order {OrderId}", command.Id);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == command.Id);
            
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", command.Id);
            return;
        }
        
        context.Orders.Remove(order);
        await context.SaveChangesAsync();
        
        _logger.LogInformation("Order {OrderId} deleted", command.Id);
    }
}
