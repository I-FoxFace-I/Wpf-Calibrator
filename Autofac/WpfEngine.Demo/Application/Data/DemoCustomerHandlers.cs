using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Data;
using WpfEngine.Demo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Demo.Application.Customers;

// ========== QUERIES ==========

public record GetAllDemoCustomersQuery : IQuery<List<DemoCustomer>>;

public record GetDemoCustomerByIdQuery(int Id) : IQuery<DemoCustomer?>;

// ========== COMMANDS ==========

public record CreateDemoCustomerCommand(
    string Name, 
    string Email, 
    string Phone, 
    string CompanyName, 
    string TaxId, 
    CustomerType Type) : ICommand;

public record UpdateDemoCustomerCommand(
    int Id,
    string Name, 
    string Email, 
    string Phone, 
    string CompanyName, 
    string TaxId, 
    CustomerType Type) : ICommand;

public record DeleteDemoCustomerCommand(int Id) : ICommand;

// ========== QUERY HANDLERS ==========

public class GetAllDemoCustomersHandler : IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<GetAllDemoCustomersHandler> _logger;
    
    public GetAllDemoCustomersHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<GetAllDemoCustomersHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task<List<DemoCustomer>> HandleAsync(GetAllDemoCustomersQuery query)
    {
        _logger.LogInformation("[DEMO] Getting all customers");
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Customers
            .Include(c => c.Addresses)
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}

public class GetDemoCustomerByIdHandler : IQueryHandler<GetDemoCustomerByIdQuery, DemoCustomer?>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<GetDemoCustomerByIdHandler> _logger;
    
    public GetDemoCustomerByIdHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<GetDemoCustomerByIdHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task<DemoCustomer?> HandleAsync(GetDemoCustomerByIdQuery query)
    {
        _logger.LogInformation("[DEMO] Getting customer {CustomerId}", query.Id);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Customers
            .Include(c => c.Addresses)
            .Include(c => c.Orders)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.Id);
    }
}

// ========== COMMAND HANDLERS ==========

public class CreateDemoCustomerHandler : ICommandHandler<CreateDemoCustomerCommand>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<CreateDemoCustomerHandler> _logger;
    
    public CreateDemoCustomerHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<CreateDemoCustomerHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(CreateDemoCustomerCommand command)
    {
        _logger.LogInformation("[DEMO] Creating customer: {Name}", command.Name);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var customer = new DemoCustomer
        {
            Name = command.Name,
            Email = command.Email,
            Phone = command.Phone,
            CompanyName = command.CompanyName,
            TaxId = command.TaxId,
            Type = command.Type
        };
        
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        
        _logger.LogInformation("[DEMO] Customer created with ID: {CustomerId}", customer.Id);
    }
}

public class UpdateDemoCustomerHandler : ICommandHandler<UpdateDemoCustomerCommand>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<UpdateDemoCustomerHandler> _logger;
    
    public UpdateDemoCustomerHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<UpdateDemoCustomerHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(UpdateDemoCustomerCommand command)
    {
        _logger.LogInformation("[DEMO] Updating customer {CustomerId}", command.Id);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var customer = await context.Customers.FindAsync(command.Id);
        if (customer == null)
        {
            _logger.LogWarning("[DEMO] Customer {CustomerId} not found", command.Id);
            return;
        }
        
        customer.Name = command.Name;
        customer.Email = command.Email;
        customer.Phone = command.Phone;
        customer.CompanyName = command.CompanyName;
        customer.TaxId = command.TaxId;
        customer.Type = command.Type;
        
        await context.SaveChangesAsync();
        _logger.LogInformation("[DEMO] Customer {CustomerId} updated", command.Id);
    }
}

public class DeleteDemoCustomerHandler : ICommandHandler<DeleteDemoCustomerCommand>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<DeleteDemoCustomerHandler> _logger;
    
    public DeleteDemoCustomerHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<DeleteDemoCustomerHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(DeleteDemoCustomerCommand command)
    {
        _logger.LogInformation("[DEMO] Deleting customer {CustomerId}", command.Id);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var customer = await context.Customers.FindAsync(command.Id);
        if (customer == null)
        {
            _logger.LogWarning("[DEMO] Customer {CustomerId} not found", command.Id);
            return;
        }
        
        context.Customers.Remove(customer);
        await context.SaveChangesAsync();
        
        _logger.LogInformation("[DEMO] Customer {CustomerId} deleted", command.Id);
    }
}
