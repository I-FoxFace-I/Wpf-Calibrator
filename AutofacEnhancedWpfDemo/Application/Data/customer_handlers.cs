using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Data;
using AutofacEnhancedWpfDemo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Application.Customers;

// ========== QUERIES ==========

public record GetAllCustomersQuery : IQuery<List<Customer>>;

public record GetCustomerByIdQuery(int Id) : IQuery<Customer?>;

// ========== COMMANDS ==========

public record CreateCustomerCommand(string Name, string Email) : ICommand;

public record UpdateCustomerCommand(int Id, string Name, string Email) : ICommand;

public record DeleteCustomerCommand(int Id) : ICommand;

// ========== QUERY HANDLERS ==========

public class GetAllCustomersHandler : IQueryHandler<GetAllCustomersQuery, List<Customer>>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<GetAllCustomersHandler> _logger;
    
    public GetAllCustomersHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<GetAllCustomersHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task<List<Customer>> HandleAsync(GetAllCustomersQuery query)
    {
        _logger.LogInformation("Getting all customers");
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}

public class GetCustomerByIdHandler : IQueryHandler<GetCustomerByIdQuery, Customer?>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<GetCustomerByIdHandler> _logger;
    
    public GetCustomerByIdHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<GetCustomerByIdHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task<Customer?> HandleAsync(GetCustomerByIdQuery query)
    {
        _logger.LogInformation("Getting customer {CustomerId}", query.Id);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.Id);
    }
}

// ========== COMMAND HANDLERS ==========

public class CreateCustomerHandler : ICommandHandler<CreateCustomerCommand>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<CreateCustomerHandler> _logger;
    
    public CreateCustomerHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<CreateCustomerHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(CreateCustomerCommand command)
    {
        _logger.LogInformation("Creating customer: {Name}", command.Name);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var customer = new Customer
        {
            Name = command.Name,
            Email = command.Email
        };
        
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        
        _logger.LogInformation("Customer created with ID: {CustomerId}", customer.Id);
    }
}

public class UpdateCustomerHandler : ICommandHandler<UpdateCustomerCommand>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<UpdateCustomerHandler> _logger;
    
    public UpdateCustomerHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<UpdateCustomerHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(UpdateCustomerCommand command)
    {
        _logger.LogInformation("Updating customer {CustomerId}", command.Id);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var customer = await context.Customers.FindAsync(command.Id);
        if (customer == null)
        {
            _logger.LogWarning("Customer {CustomerId} not found", command.Id);
            return;
        }
        
        customer.Name = command.Name;
        customer.Email = command.Email;
        
        await context.SaveChangesAsync();
        _logger.LogInformation("Customer {CustomerId} updated", command.Id);
    }
}

public class DeleteCustomerHandler : ICommandHandler<DeleteCustomerCommand>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<DeleteCustomerHandler> _logger;
    
    public DeleteCustomerHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<DeleteCustomerHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(DeleteCustomerCommand command)
    {
        _logger.LogInformation("Deleting customer {CustomerId}", command.Id);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var customer = await context.Customers.FindAsync(command.Id);
        if (customer == null)
        {
            _logger.LogWarning("Customer {CustomerId} not found", command.Id);
            return;
        }
        
        context.Customers.Remove(customer);
        await context.SaveChangesAsync();
        
        _logger.LogInformation("Customer {CustomerId} deleted", command.Id);
    }
}
