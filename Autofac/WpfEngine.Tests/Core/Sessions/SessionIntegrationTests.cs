using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.Data.Sessions;
using WpfEngine.Extensions;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using Xunit;

namespace WpfEngine.Tests.Core.Sessions;

public class SessionIntegrationTests : IDisposable
{
    private readonly IContainer _container;
    private readonly IScopeManager _scopeManager;
    
    public SessionIntegrationTests()
    {
        var builder = new ContainerBuilder();
        
        // Register core services
        builder.Register(c => Mock.Of<ILogger>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<ScopeSession>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<ScopeManager>>()).InstancePerDependency();
        
        // Register test services with scope tags
        builder.RegisterType<OrderService>()
            .AsSelf()
            .InstancePerMatchingLifetimeScope(ScopeTag.Workflow("order").ToAutofacTag());
        
        builder.RegisterType<CustomerRepository>()
            .AsSelf()
            .InstancePerMatchingLifetimeScope(ScopeTag.Database().ToAutofacTag());
        
        builder.RegisterType<OrderRepository>()
            .AsSelf()
            .InstancePerMatchingLifetimeScope(ScopeTag.Database().ToAutofacTag());
        
        _container = builder.Build();
        _scopeManager = new ScopeManager(_container, Mock.Of<ILogger<ScopeManager>>());
    }
    
    [Fact]
    public void DatabaseSession_Execute_ShouldResolveRepositories()
    {
        // Act
        var result = _scopeManager
            .CreateDatabaseSession()
            .WithService<CustomerRepository>()
            .WithService<OrderRepository>()
            .ExecuteWithResult((customerRepo, orderRepo) =>
            {
                customerRepo.Should().NotBeNull();
                orderRepo.Should().NotBeNull();
                return $"{customerRepo.GetName()}+{orderRepo.GetName()}";
            }, defaultValue: "");
        
        // Assert
        result.Should().Be("CustomerRepo+OrderRepo");
    }
    
    [Fact]
    public async Task DatabaseSession_ExecuteAsync_ShouldWork()
    {
        // Act
        var result = await _scopeManager
            .CreateDatabaseSession()
            .WithService<CustomerRepository>()
            .ExecuteWithResultAsync(async repo =>
            {
                await Task.Delay(10);
                return repo.GetName();
            }, defaultValue: "");
        
        // Assert
        result.Should().Be("CustomerRepo");
    }
    
    [Fact]
    public void WorkflowSession_ShouldShareServices()
    {
        // Arrange & Act
        using var session = _scopeManager
            .CreateWorkflowSession("order")
            .Build();
        
        var service1 = session.Resolve<OrderService>();
        var service2 = session.Resolve<OrderService>();
        
        // Assert
        service1.Should().BeSameAs(service2); // Same instance in same scope
    }
    
    [Fact]
    public void ChildSession_ShouldInheritParentServices()
    {
        // Arrange
        using var parentSession = _scopeManager
            .CreateWorkflowSession("order")
            .Build();
        
        // Act
        using var childSession = parentSession
            .CreateChildDatabaseSession()
            .Build();
        
        // Get order service from child (should resolve from parent)
        var orderService = childSession.Resolve<OrderService>();
        
        // Assert
        orderService.Should().NotBeNull();
    }
    
    [Fact]
    public void MultipleChildSessions_ShouldShareParentServices()
    {
        // Arrange
        using var parentSession = _scopeManager
            .CreateWorkflowSession("order")
            .Build();
        
        // Act
        using var child1 = parentSession.CreateChild(ScopeTag.Database()).Build();
        using var child2 = parentSession.CreateChild(ScopeTag.Database()).Build();
        
        var service1 = child1.Resolve<OrderService>();
        var service2 = child2.Resolve<OrderService>();
        
        // Assert - should be the same instance from parent
        service1.Should().BeSameAs(service2);
    }
    
    [Fact]
    public void CustomSession_ShouldWork()
    {
        // Act
        using var session = _scopeManager
            .CreateCustomSession("my-operation")
            .Build();
        
        // Assert
        session.Should().NotBeNull();
        session.IsActive.Should().BeTrue();
        session.Tag.Category.Should().Be(ScopeCategory.Custom);
    }
    
    [Fact]
    public async Task ComplexScenario_WorkflowWithNestedDatabaseOperations()
    {
        // Scenario: Workflow session with multiple DB operations
        
        using var workflowSession = _scopeManager
            .CreateWorkflowSession("order-entry")
            .Build();
        
        // First DB operation
        var customer = await workflowSession
            .CreateChildDatabaseSession()
            .WithService<CustomerRepository>()
            .ExecuteWithResultAsync(async repo =>
            {
                await Task.Delay(5);
                return repo.GetName();
            }, defaultValue: "");
        
        customer.Should().Be("CustomerRepo");
        
        // Second DB operation
        var order = await workflowSession
            .CreateChildDatabaseSession()
            .WithService<OrderRepository>()
            .ExecuteWithResultAsync(async repo =>
            {
                await Task.Delay(5);
                return repo.GetName();
            }, defaultValue: "");
        
        order.Should().Be("OrderRepo");
        
        // Workflow is still active
        workflowSession.IsActive.Should().BeTrue();
    }
    
    [Fact]
    public void Session_Dispose_ShouldCloseSession()
    {
        // Arrange
        IScopeSession session;
        using (session = _scopeManager.CreateDatabaseSession().Build())
        {
            session.IsActive.Should().BeTrue();
        }
        
        // Assert
        session.IsActive.Should().BeFalse();
    }
    
    [Fact]
    public void OnDispose_Hook_ShouldExecute()
    {
        // Arrange
        var hookExecuted = false;
        
        // Act
        using (var session = _scopeManager
            .CreateDatabaseSession()
            .OnDispose(() => hookExecuted = true)
            .Build())
        {
            hookExecuted.Should().BeFalse();
        }
        
        // Assert
        hookExecuted.Should().BeTrue();
    }
    
    public void Dispose()
    {
        _scopeManager?.CloseAllSessions();
        _container?.Dispose();
    }
    
    // Test services
    private class OrderService
    {
        public string Process() => "Order processed";
    }
    
    private class CustomerRepository
    {
        public string GetName() => "CustomerRepo";
    }
    
    private class OrderRepository
    {
        public string GetName() => "OrderRepo";
    }
}

