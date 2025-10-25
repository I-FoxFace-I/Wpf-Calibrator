using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Data.Demo;
using AutofacEnhancedWpfDemo.Models.Demo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Application.Demo.Addresses;

// ========== QUERIES ==========

public record GetAddressesByCustomerQuery(int CustomerId) : IQuery<List<DemoAddress>>;

public class GetAddressesByCustomerHandler : IQueryHandler<GetAddressesByCustomerQuery, List<DemoAddress>>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<GetAddressesByCustomerHandler> _logger;

    public GetAddressesByCustomerHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<GetAddressesByCustomerHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<List<DemoAddress>> HandleAsync(GetAddressesByCustomerQuery query)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var addresses = await context.Addresses
            .Where(a => a.CustomerId == query.CustomerId)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} addresses for customer {CustomerId}", 
            addresses.Count, query.CustomerId);
        
        return addresses;
    }
}

public record GetShippingAddressesQuery(int CustomerId) : IQuery<List<DemoAddress>>;

public class GetShippingAddressesHandler : IQueryHandler<GetShippingAddressesQuery, List<DemoAddress>>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<GetShippingAddressesHandler> _logger;

    public GetShippingAddressesHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<GetShippingAddressesHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<List<DemoAddress>> HandleAsync(GetShippingAddressesQuery query)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var addresses = await context.Addresses
            .Where(a => a.CustomerId == query.CustomerId && 
                       (a.Type == AddressType.Shipping || a.Type == AddressType.Both))
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} shipping addresses for customer {CustomerId}", 
            addresses.Count, query.CustomerId);
        
        return addresses;
    }
}

// ========== COMMANDS ==========

public record CreateAddressCommand(
    int CustomerId,
    string Street,
    string City,
    string ZipCode,
    string Country,
    AddressType Type
) : ICommand;

public class CreateAddressHandler : ICommandHandler<CreateAddressCommand>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<CreateAddressHandler> _logger;

    public CreateAddressHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<CreateAddressHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task HandleAsync(CreateAddressCommand command)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var address = new DemoAddress
        {
            CustomerId = command.CustomerId,
            Street = command.Street,
            City = command.City,
            ZipCode = command.ZipCode,
            Country = command.Country,
            Type = command.Type
        };

        context.Addresses.Add(address);
        await context.SaveChangesAsync();

        _logger.LogInformation("Created address for customer {CustomerId}", command.CustomerId);
    }
}

public record UpdateAddressCommand(
    int AddressId,
    string Street,
    string City,
    string ZipCode,
    string Country,
    AddressType Type
): ICommand;

public class UpdateAddressHandler : ICommandHandler<UpdateAddressCommand>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<UpdateAddressHandler> _logger;

    public UpdateAddressHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<UpdateAddressHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateAddressCommand command)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var address = await context.Addresses.FindAsync(command.AddressId);
        
        if (address == null)
        {
            _logger.LogWarning("Address {AddressId} not found", command.AddressId);
            throw new InvalidOperationException($"Address {command.AddressId} not found");
        }

        address.Street = command.Street;
        address.City = command.City;
        address.ZipCode = command.ZipCode;
        address.Country = command.Country;
        address.Type = command.Type;

        await context.SaveChangesAsync();
        
        _logger.LogInformation("Updated address {AddressId}", command.AddressId);
    }
}
