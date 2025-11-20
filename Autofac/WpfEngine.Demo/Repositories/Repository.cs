using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using WpfEngine.Demo.Data;
using WpfEngine.Demo.Models;

namespace WpfEngine.Demo.Repositories;

/// <summary>
/// Generic repository implementation using Entity Framework Core
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly DemoDbContext _context;
    private readonly DbSet<TEntity> _dbSet;
    private readonly ILogger<Repository<TEntity>> _logger;

    protected DemoDbContext Context => _context;
    protected ILogger<Repository<TEntity>> Logger => _logger;
    public Repository(DemoDbContext context, ILogger<Repository<TEntity>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentException(nameof(logger));
        _dbSet = _context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            return await _dbSet.FindAsync(new object[] { id }, ct);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting entity {EntityType} by ID {Id}", typeof(TEntity).Name, id);
            throw;
        }
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            return await _dbSet.ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting all entities {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            await _dbSet.AddAsync(entity, ct);
            _logger?.LogDebug("Added entity {EntityType}", typeof(TEntity).Name);
            return entity;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error adding entity {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            _dbSet.Update(entity);
            _logger?.LogDebug("Updated entity {EntityType}", typeof(TEntity).Name);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error updating entity {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            _dbSet.Remove(entity);
            _logger?.LogDebug("Deleted entity {EntityType}", typeof(TEntity).Name);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting entity {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, 
        CancellationToken ct = default)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        try
        {
            return await _dbSet.Where(predicate).ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error finding entities {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    public async Task<bool> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            await Context.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving {EntityType} changes", typeof(TEntity).Name);
            return false;
        }
    }
}
