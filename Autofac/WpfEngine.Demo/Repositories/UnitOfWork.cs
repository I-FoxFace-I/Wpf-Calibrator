using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using WpfEngine.Demo.Data;
using WpfEngine.Demo.Models;

namespace WpfEngine.Demo.Repositories;

/// <summary>
/// Unit of Work implementation
/// Coordinates multiple repositories and manages database transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DemoDbContext _context;
    private readonly ILogger<UnitOfWork>? _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(DemoDbContext context, ILogger<UnitOfWork> logger, ILoggerFactory loggerFactory)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

    }

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);
        
        if (!_repositories.TryGetValue(type, out var repository))
        {
            // Create repository without logger for now
            // Logger can be injected via DI if needed
            repository = new Repository<TEntity>(_context, _loggerFactory.CreateLogger<Repository<TEntity>>());
            _repositories[type] = repository;
            
            _logger?.LogDebug("Created repository for {EntityType}", type.Name);
        }

        return (IRepository<TEntity>)repository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await _context.SaveChangesAsync(ct);
            _logger?.LogInformation("Saved {Count} changes", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving changes");
            throw;
        }
    }

    public void Rollback()
    {
        try
        {
            var entries = _context.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Unchanged)
                .ToList();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                }
            }

            _logger?.LogInformation("Rolled back {Count} changes", entries.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error rolling back changes");
            throw;
        }
    }
}
