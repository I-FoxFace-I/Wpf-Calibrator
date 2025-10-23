using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Models.Entities;

namespace Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;

public abstract class MockRepositoryBase<T> : IRepository<T> where T : AggregateRoot
{
    protected readonly Dictionary<Guid, T> _data = new();

    public Task<List<T>> GetAll()
    {
        return Task.FromResult(_data.Values.ToList());
    }

    public Task<List<T>> GetAll(Expression<Func<T, bool>> expression)
    {
        return Task.FromResult(_data.Values.Where(expression.Compile()).ToList());
    }

    public Task<T?> TryGetFirst()
    {
        return Task.FromResult(_data.Values.FirstOrDefault());
    }

    public Task<T?> TryGet(Guid id)
    {
        return Task.FromResult(_data.TryGetValue(id, out var entity) ? entity : null);
    }

    public Task<T?> TryGet(Expression<Func<T, bool>> expression)
    {
        return Task.FromResult(_data.Values.FirstOrDefault(expression.Compile()));
    }

    public Task Upsert(T item)
    {
        if (item.IsNew)
        {
            _data[item.Id] = item;
            item.IsNew = false;
        }
        else
        {
            _data[item.Id] = item;
        }
        return Task.CompletedTask;
    }

    public Task Delete(Guid id)
    {
        _data.Remove(id);
        return Task.CompletedTask;
    }

    public Task Delete(T item)
    {
        _data.Remove(item.Id);
        return Task.CompletedTask;
    }

    public Task<List<T>> GetAllWithNoTracking()
    {
        return Task.FromResult(_data.Values.ToList());
    }

    public Task<List<T>> GetAllWithNoTracking(Expression<Func<T, bool>> expression)
    {
        return Task.FromResult(_data.Values.Where(expression.Compile()).ToList());
    }

    public Task<T?> TryGetWithNoTracking(Guid id)
    {
        return Task.FromResult(_data.TryGetValue(id, out var entity) ? entity : null);
    }

    public Task<T?> TryGetWithNoTracking(Expression<Func<T, bool>> expression)
    {
        return Task.FromResult(_data.Values.FirstOrDefault(expression.Compile()));
    }
}
