using Autofac;
using WpfEngine.Data.Abstract;
using WpfEngine.Services.Autofac;
using WpfEngine.ViewModels;

namespace WpfEngine.Services.Sessions.Implementation;

/// <summary>
/// Session builder with two declared services
/// </summary>
internal class SessionBuilder<T1, T2> : ISessionBuilder<T1, T2>
    where T1 : notnull
    where T2 : notnull
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
    
    // ========== SERVICE DECLARATION ==========
    
    public ISessionBuilder<T1, T2, T3> WithService<T3>() where T3 : notnull
    {
        return new SessionBuilder<T1, T2, T3>(_baseBuilder);
    }
    
    // ========== EXECUTE WITH TYPED SERVICES ==========
    
    public void Execute(Action<T1, T2> action, Action<Exception>? onError = null)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        using var session = _baseBuilder.Build();
        try
        {
            var service1 = session.Resolve<T1>();
            var service2 = session.Resolve<T2>();
            action(service1, service2);
            
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
    
    public async Task ExecuteAsync(Func<T1, T2, Task> action, Action<Exception>? onError = null)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        await using var session = _baseBuilder.Build();
        try
        {
            var service1 = session.Resolve<T1>();
            var service2 = session.Resolve<T2>();
            await action(service1, service2);
            
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
    
    public TResult ExecuteWithResult<TResult>(Func<T1, T2, TResult> func, TResult defaultValue = default!, Action<Exception>? onError = null)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));
        
        using var session = _baseBuilder.Build();
        try
        {
            var service1 = session.Resolve<T1>();
            var service2 = session.Resolve<T2>();
            var result = func(service1, service2);
            
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
    
    public async Task<TResult> ExecuteWithResultAsync<TResult>(Func<T1, T2, Task<TResult>> func, TResult defaultValue = default!, Action<Exception>? onError = null)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));
        
        await using var session = _baseBuilder.Build();
        try
        {
            var service1 = session.Resolve<T1>();
            var service2 = session.Resolve<T2>();
            var result = await func(service1, service2);
            
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

