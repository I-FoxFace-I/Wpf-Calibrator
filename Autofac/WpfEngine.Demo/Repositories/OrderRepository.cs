using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using WpfEngine.Demo.Data;
using WpfEngine.Demo.Models;

namespace WpfEngine.Demo.Repositories;

public class OrderRepository : Repository<DemoOrder>, IOrderRepository
{
    public OrderRepository(DemoDbContext context, ILogger<OrderRepository>? logger = null)
        : base(context, logger)
    {

    }

    public async Task<IEnumerable<DemoOrder>> GetAllOrdersAsync(CancellationToken ct = default)
    {
        try
        {
            return await Context.Orders.Include(o => o.Customer)
                                       .Include(o => o.ShippingAddress)
                                       .Include(o => o.Items)
                                           .ThenInclude(i => i.Product)
                                       .OrderByDescending(o => o.OrderDate)
                                       .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting all entities {EntityType}", typeof(DemoOrder).Name);
            throw;
        }
    }

    public async Task<DemoOrder?> GetOrderAsync(int id, CancellationToken ct = default)
    {

        try
        {
            return await Context.Orders.Where(ord => ord.Id == id)
                                       .Include(o => o.Customer)
                                            .ThenInclude(oc => oc.Addresses)
                                       .Include(o => o.ShippingAddress)
                                       .Include(o => o.Items)
                                           .ThenInclude(i => i.Product)
                                       .FirstOrDefaultAsync(ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting entity {EntityType} with with ID {OrderId}", typeof(DemoOrder).Name, id);
            throw;
        }


    }
}
