using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Data.Demo;
using AutofacEnhancedWpfDemo.Models.Demo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Application.Demo.Orders;

// ========== QUERIES ==========

public record GetAllDemoOrdersQuery :IQuery<List<DemoOrder>>;

public class GetAllDemoOrdersHandler : IQueryHandler<GetAllDemoOrdersQuery, List<DemoOrder>>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<GetAllDemoOrdersHandler> _logger;

    public GetAllDemoOrdersHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<GetAllDemoOrdersHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<List<DemoOrder>> HandleAsync(GetAllDemoOrdersQuery query)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var orders = await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        foreach (var order in orders)
        {
            order.CustomerName = order.Customer.Name;
            order.ShippingAddressText = order.ShippingAddress?.FullAddress ?? "Not specified";
        }

        _logger.LogInformation("Retrieved {Count} orders", orders.Count);
        return orders;
    }
}

public record GetDemoOrderByIdQuery(int OrderId) : IQuery<DemoOrder?>;

public class GetDemoOrderByIdHandler : IQueryHandler<GetDemoOrderByIdQuery, DemoOrder?>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<GetDemoOrderByIdHandler> _logger;

    public GetDemoOrderByIdHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<GetDemoOrderByIdHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<DemoOrder?> HandleAsync(GetDemoOrderByIdQuery query)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var order = await context.Orders
            .Include(o => o.Customer)
                .ThenInclude(c => c.Addresses)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(o => o.Id == query.OrderId);

        if (order != null)
        {
            order.CustomerName = order.Customer.Name;
            order.ShippingAddressText = order.ShippingAddress?.FullAddress ?? "Not specified";
            
            foreach (var item in order.Items)
            {
                item.ProductName = item.Product.Name;
            }
        }

        _logger.LogInformation("Retrieved order {OrderId}", query.OrderId);
        return order;
    }
}

// ========== COMMANDS ==========

public record CreateDemoOrderCommand(
    int CustomerId,
    int? ShippingAddressId,
    List<DemoOrderItem> Items
) : ICommand;

public class CreateDemoOrderHandler : ICommandHandler<CreateDemoOrderCommand>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<CreateDemoOrderHandler> _logger;

    public CreateDemoOrderHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<CreateDemoOrderHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task HandleAsync(CreateDemoOrderCommand command)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var orderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        
        var order = new DemoOrder
        {
            OrderNumber = orderNumber,
            OrderDate = DateTime.Now,
            CustomerId = command.CustomerId,
            ShippingAddressId = command.ShippingAddressId,
            Status = OrderStatus.Pending,
            Items = command.Items.Select(i => new DemoOrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created order {OrderNumber} with {ItemCount} items", 
            orderNumber, order.Items.Count);
    }
}

public record UpdateDemoOrderCommand(
    int OrderId,
    int? ShippingAddressId,
    OrderStatus Status
) : ICommand;

public class UpdateDemoOrderHandler : ICommandHandler<UpdateDemoOrderCommand>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<UpdateDemoOrderHandler> _logger;

    public UpdateDemoOrderHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<UpdateDemoOrderHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateDemoOrderCommand command)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var order = await context.Orders.FindAsync(command.OrderId);
        
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", command.OrderId);
            throw new InvalidOperationException($"Order {command.OrderId} not found");
        }

        order.ShippingAddressId = command.ShippingAddressId;
        order.Status = command.Status;

        await context.SaveChangesAsync();
        
        _logger.LogInformation("Updated order {OrderId}", command.OrderId);
    }
}

public record DeleteDemoOrderCommand(int OrderId) : ICommand;

public class DeleteDemoOrderHandler : ICommandHandler<DeleteDemoOrderCommand>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<DeleteDemoOrderHandler> _logger;

    public DeleteDemoOrderHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<DeleteDemoOrderHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task HandleAsync(DeleteDemoOrderCommand command)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId);
        
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", command.OrderId);
            return;
        }

        context.Orders.Remove(order);
        await context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted order {OrderId}", command.OrderId);
    }
}
