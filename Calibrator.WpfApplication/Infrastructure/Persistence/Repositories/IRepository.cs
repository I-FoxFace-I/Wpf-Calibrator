using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Models.Entities;

namespace Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;

public interface IRepository<T> where T : AggregateRoot
{
    Task<List<T>> GetAll();
    Task<List<T>> GetAll(Expression<Func<T, bool>> expression);
    Task<T?> TryGetFirst();
    Task<T?> TryGet(Guid id);
    Task<T?> TryGet(Expression<Func<T, bool>> expression);
    Task Upsert(T item);
    Task Delete(Guid id);
    Task Delete(T item);

    Task<List<T>> GetAllWithNoTracking();
    Task<List<T>> GetAllWithNoTracking(Expression<Func<T, bool>> expression);
    Task<T?> TryGetWithNoTracking(Guid id);
    Task<T?> TryGetWithNoTracking(Expression<Func<T, bool>> expression);
}
