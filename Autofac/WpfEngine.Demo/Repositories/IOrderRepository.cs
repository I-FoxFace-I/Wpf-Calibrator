using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using WpfEngine.Demo.Data;
using WpfEngine.Demo.Models;

namespace WpfEngine.Demo.Repositories;

public interface IOrderRepository : IRepository<DemoOrder>
{
    Task<DemoOrder?> GetOrderAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<DemoOrder>> GetAllOrdersAsync(CancellationToken ct = default);
}
