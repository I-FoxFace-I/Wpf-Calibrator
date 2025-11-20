using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using WpfEngine.Demo.Data;
using WpfEngine.Demo.Models;

namespace WpfEngine.Demo.Repositories;

/// <summary>
/// Unit of Work pattern interface
/// Manages multiple repositories and coordinates database transactions
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Get repository for entity type
    /// </summary>
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

    /// <summary>
    /// Save all pending changes
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Discard all pending changes
    /// </summary>
    void Rollback();
}
