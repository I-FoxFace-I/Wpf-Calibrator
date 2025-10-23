using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Data;
using AutofacEnhancedWpfDemo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Application.Products;

// ========== QUERIES ==========

public record GetAllProductsQuery : IQuery<List<Product>>;

public record GetProductByIdQuery(int Id) : IQuery<Product?>;

// ========== COMMANDS ==========

public record CreateProductCommand(string Name, decimal Price, int Stock) : ICommand;

public record UpdateProductCommand(int Id, string Name, decimal Price, int Stock) : ICommand;

public record DeleteProductCommand(int Id) : ICommand;

public record UpdateProductPriceCommand(int Id, decimal NewPrice) : ICommand;

// ========== QUERY HANDLERS ==========

public class GetAllProductsHandler : IQueryHandler<GetAllProductsQuery, List<Product>>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<GetAllProductsHandler> _logger;
    
    public GetAllProductsHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<GetAllProductsHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task<List<Product>> HandleAsync(GetAllProductsQuery query)
    {
        _logger.LogInformation("Getting all products");
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}

public class GetProductByIdHandler : IQueryHandler<GetProductByIdQuery, Product?>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<GetProductByIdHandler> _logger;
    
    public GetProductByIdHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<GetProductByIdHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task<Product?> HandleAsync(GetProductByIdQuery query)
    {
        _logger.LogInformation("Getting product {ProductId}", query.Id);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.Id);
    }
}

// ========== COMMAND HANDLERS ==========

public class CreateProductHandler : ICommandHandler<CreateProductCommand>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<CreateProductHandler> _logger;
    
    public CreateProductHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<CreateProductHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(CreateProductCommand command)
    {
        _logger.LogInformation("Creating product: {Name}", command.Name);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var product = new Product
        {
            Name = command.Name,
            Price = command.Price,
            Stock = command.Stock
        };
        
        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        _logger.LogInformation("Product created with ID: {ProductId}", product.Id);
    }
}

public class UpdateProductHandler : ICommandHandler<UpdateProductCommand>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<UpdateProductHandler> _logger;
    
    public UpdateProductHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<UpdateProductHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(UpdateProductCommand command)
    {
        _logger.LogInformation("Updating product {ProductId}", command.Id);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var product = await context.Products.FindAsync(command.Id);
        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found", command.Id);
            return;
        }
        
        product.Name = command.Name;
        product.Price = command.Price;
        product.Stock = command.Stock;
        
        await context.SaveChangesAsync();
        _logger.LogInformation("Product {ProductId} updated", command.Id);
    }
}

public class UpdateProductPriceHandler : ICommandHandler<UpdateProductPriceCommand>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<UpdateProductPriceHandler> _logger;
    
    public UpdateProductPriceHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<UpdateProductPriceHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(UpdateProductPriceCommand command)
    {
        _logger.LogInformation("Updating price for product {ProductId}", command.Id);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var product = await context.Products.FindAsync(command.Id);
        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found", command.Id);
            return;
        }
        
        product.Price = command.NewPrice;
        await context.SaveChangesAsync();
        
        _logger.LogInformation("Product {ProductId} price updated to {Price}", command.Id, command.NewPrice);
    }
}

public class DeleteProductHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<DeleteProductHandler> _logger;
    
    public DeleteProductHandler(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<DeleteProductHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    
    public async Task HandleAsync(DeleteProductCommand command)
    {
        _logger.LogInformation("Deleting product {ProductId}", command.Id);
        
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var product = await context.Products.FindAsync(command.Id);
        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found", command.Id);
            return;
        }
        
        context.Products.Remove(product);
        await context.SaveChangesAsync();
        
        _logger.LogInformation("Product {ProductId} deleted", command.Id);
    }
}
