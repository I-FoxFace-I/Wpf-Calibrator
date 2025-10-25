using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Data.Demo;
using AutofacEnhancedWpfDemo.Models.Demo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Application.Demo.Products;

// ========== QUERIES ==========

public record GetAllDemoProductsQuery : IQuery<List<DemoProduct>>;

public record GetDemoProductByIdQuery(int Id) : IQuery<DemoProduct?>;

public record GetAllDemoCategoriesQuery : IQuery<List<DemoProductCategory>>;

// ========== COMMANDS ==========

public record CreateDemoProductCommand(
    string Name,
    string Description,
    string Barcode,
    decimal Price,
    int Stock,
    decimal Weight,
    string Unit,
    int? CategoryId) : ICommand;

public record UpdateDemoProductCommand(
    int Id,
    string Name,
    string Description,
    string Barcode,
    decimal Price,
    int Stock,
    decimal Weight,
    string Unit,
    int? CategoryId) : ICommand;

public record DeleteDemoProductCommand(int Id) : ICommand;

// ========== QUERY HANDLERS ==========

public class GetAllDemoProductsHandler : IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<GetAllDemoProductsHandler> _logger;
    
    public GetAllDemoProductsHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<GetAllDemoProductsHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task<List<DemoProduct>> HandleAsync(GetAllDemoProductsQuery query)
    {
        _logger.LogInformation("[DEMO] Getting all products");
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}

public class GetDemoProductByIdHandler : IQueryHandler<GetDemoProductByIdQuery, DemoProduct?>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<GetDemoProductByIdHandler> _logger;
    
    public GetDemoProductByIdHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<GetDemoProductByIdHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task<DemoProduct?> HandleAsync(GetDemoProductByIdQuery query)
    {
        _logger.LogInformation("[DEMO] Getting product {ProductId}", query.Id);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.Id);
    }
}

public class GetAllDemoCategoriesHandler : IQueryHandler<GetAllDemoCategoriesQuery, List<DemoProductCategory>>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<GetAllDemoCategoriesHandler> _logger;
    
    public GetAllDemoCategoriesHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<GetAllDemoCategoriesHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task<List<DemoProductCategory>> HandleAsync(GetAllDemoCategoriesQuery query)
    {
        _logger.LogInformation("[DEMO] Getting all categories");
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}

// ========== COMMAND HANDLERS ==========

public class CreateDemoProductHandler : ICommandHandler<CreateDemoProductCommand>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<CreateDemoProductHandler> _logger;
    
    public CreateDemoProductHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<CreateDemoProductHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(CreateDemoProductCommand command)
    {
        _logger.LogInformation("[DEMO] Creating product: {Name}", command.Name);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var product = new DemoProduct
        {
            Name = command.Name,
            Description = command.Description,
            Barcode = command.Barcode,
            Price = command.Price,
            Stock = command.Stock,
            Weight = command.Weight,
            Unit = command.Unit,
            CategoryId = command.CategoryId
        };
        
        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        _logger.LogInformation("[DEMO] Product created with ID: {ProductId}", product.Id);
    }
}

public class UpdateDemoProductHandler : ICommandHandler<UpdateDemoProductCommand>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<UpdateDemoProductHandler> _logger;
    
    public UpdateDemoProductHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<UpdateDemoProductHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(UpdateDemoProductCommand command)
    {
        _logger.LogInformation("[DEMO] Updating product {ProductId}", command.Id);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var product = await context.Products.FindAsync(command.Id);
        if (product == null)
        {
            _logger.LogWarning("[DEMO] Product {ProductId} not found", command.Id);
            return;
        }
        
        product.Name = command.Name;
        product.Description = command.Description;
        product.Barcode = command.Barcode;
        product.Price = command.Price;
        product.Stock = command.Stock;
        product.Weight = command.Weight;
        product.Unit = command.Unit;
        product.CategoryId = command.CategoryId;
        
        await context.SaveChangesAsync();
        _logger.LogInformation("[DEMO] Product {ProductId} updated", command.Id);
    }
}

public class DeleteDemoProductHandler : ICommandHandler<DeleteDemoProductCommand>
{
    private readonly IDbContextFactory<DemoDbContext> _contextFactory;
    private readonly ILogger<DeleteDemoProductHandler> _logger;
    
    public DeleteDemoProductHandler(
        IDbContextFactory<DemoDbContext> contextFactory,
        ILogger<DeleteDemoProductHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(DeleteDemoProductCommand command)
    {
        _logger.LogInformation("[DEMO] Deleting product {ProductId}", command.Id);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var product = await context.Products.FindAsync(command.Id);
        if (product == null)
        {
            _logger.LogWarning("[DEMO] Product {ProductId} not found", command.Id);
            return;
        }
        
        context.Products.Remove(product);
        await context.SaveChangesAsync();
        
        _logger.LogInformation("[DEMO] Product {ProductId} deleted", command.Id);
    }
}
