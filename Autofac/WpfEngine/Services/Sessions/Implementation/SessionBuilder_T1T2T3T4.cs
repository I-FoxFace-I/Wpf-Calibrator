using Autofac;
using WpfEngine.Data.Abstract;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Services.Sessions;
using WpfEngine.ViewModels;

namespace WpfEngine.Services.Sessions.Implementation;

/// <summary>
/// Session builder with four declared services (maximum)
/// </summary>
internal class SessionBuilder<T1, T2, T3, T4> : ISessionBuilder<T1, T2, T3, T4>
    where T1 : notnull
    where T2 : notnull
    where T3 : notnull
    where T4 : notnull
{
    private readonly SessionBuilder _baseBuilder;
    
    public SessionBuilder(SessionBuilder baseBuilder)
    {
        _baseBuilder = baseBuilder ?? throw new ArgumentNullException(nameof(baseBuilder));
    }
    
    // ========== DELEGATION TO BASE BUILDER ==========
    
    public ISessionBuilder WithModule<TModule>() where TModule : ISessionModule, new()
        => _baseBuilder.WithModule<TModule>();
    
    public ISessionBuilder WithModule(Action<ContainerBuilder> configure)
        => _baseBuilder.WithModule(configure);
    
    ISessionBuilder<T> ISessionBuilder.WithService<T>()
        => _baseBuilder.WithService<T>();
    
    public ISessionBuilder WithAutoSave(bool enabled = true)
        => _baseBuilder.WithAutoSave(enabled);
    
    public ISessionBuilder OnDispose(Action? hook)
        => _baseBuilder.OnDispose(hook);
    
    public ISessionBuilder AutoCloseWhenEmpty()
        => _baseBuilder.AutoCloseWhenEmpty();
    
    public IScopeSession Build()
        => _baseBuilder.Build();
    
    public IScopeSession OpenWindow<TViewModel>() where TViewModel : IViewModel
        => _baseBuilder.OpenWindow<TViewModel>();
    
    public IScopeSession OpenWindow<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters
        => _baseBuilder.OpenWindow<TViewModel, TParameters>(parameters);
    
    // ========== EXECUTE WITH TYPED SERVICES ==========
    
    public void Execute(Action<T1, T2, T3, T4> action, Action<Exception>? onError = null)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        using var session = _baseBuilder.Build();
        try
        {
            var service1 = session.Resolve<T1>();
            var service2 = session.Resolve<T2>();
            var service3 = session.Resolve<T3>();
            var service4 = session.Resolve<T4>();
            action(service1, service2, service3, service4);
            
            if (session is ScopeSession scopeSession)
            {
                scopeSession.SaveIfAutoSave();
            }
        }
        catch (Exception ex)
        {
            if (onError != null)
            {
                onError(ex);
            }
            else if (session is ScopeSession scopeSession)
            {
                scopeSession.Rollback();
            }
            throw;
        }
    }
    
    public async Task ExecuteAsync(Func<T1, T2, T3, T4, Task> action, Action<Exception>? onError = null)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        await using var session = _baseBuilder.Build();
        try
        {
            var service1 = session.Resolve<T1>();
            var service2 = session.Resolve<T2>();
            var service3 = session.Resolve<T3>();
            var service4 = session.Resolve<T4>();
            await action(service1, service2, service3, service4);
            
            if (session is ScopeSession scopeSession)
            {
                await scopeSession.SaveIfAutoSaveAsync();
            }
        }
        catch (Exception ex)
        {
            if (onError != null)
            {
                onError(ex);
            }
            else if (session is ScopeSession scopeSession)
            {
                scopeSession.Rollback();
            }
            throw;
        }
    }
    
    // ========== EXECUTE WITH RESULT ==========
    
    public TResult ExecuteWithResult<TResult>(Func<T1, T2, T3, T4, TResult> func, TResult defaultValue = default!, Action<Exception>? onError = null)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));
        
        using var session = _baseBuilder.Build();
        try
        {
            var service1 = session.Resolve<T1>();
            var service2 = session.Resolve<T2>();
            var service3 = session.Resolve<T3>();
            var service4 = session.Resolve<T4>();
            var result = func(service1, service2, service3, service4);
            
            if (session is ScopeSession scopeSession)
            {
                scopeSession.SaveIfAutoSave();
            }
            
            return result;
        }
        catch (Exception ex)
        {
            if (onError != null)
            {
                onError(ex);
            }
            else if (session is ScopeSession scopeSession)
            {
                scopeSession.Rollback();
            }
            return defaultValue;
        }
    }
    
    public async Task<TResult> ExecuteWithResultAsync<TResult>(Func<T1, T2, T3, T4, Task<TResult>> func, TResult defaultValue = default!, Action<Exception>? onError = null)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));
        
        await using var session = _baseBuilder.Build();
        try
        {
            var service1 = session.Resolve<T1>();
            var service2 = session.Resolve<T2>();
            var service3 = session.Resolve<T3>();
            var service4 = session.Resolve<T4>();
            var result = await func(service1, service2, service3, service4);
            
            if (session is ScopeSession scopeSession)
            {
                await scopeSession.SaveIfAutoSaveAsync();
            }
            
            return result;
        }
        catch (Exception ex)
        {
            if (onError != null)
            {
                onError(ex);
            }
            else if (session is ScopeSession scopeSession)
            {
                scopeSession.Rollback();
            }
            return defaultValue;
        }
    }
    
    // ========== DISPOSE ==========
    
    public void Dispose()
    {
        _baseBuilder.Dispose();
    }
}

