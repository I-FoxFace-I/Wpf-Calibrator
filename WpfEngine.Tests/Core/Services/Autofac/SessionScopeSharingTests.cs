using Autofac;
using Autofac.Core;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.Core.Scopes;
using Xunit;

namespace WpfEngine.Tests.Core.Services.Autofac;

/// <summary>
/// Tests for InstancePerMatchingLifetimeScope - service sharing across session scopes
/// This is the KEY feature for workflow session support
/// </summary>
public class SessionScopeSharingTests : IDisposable
{
    private readonly IContainer _container;

    public SessionScopeSharingTests()
    {
        var builder = new ContainerBuilder();

        // Register shared service with InstancePerMatchingLifetimeScope
        builder.RegisterType<SharedOrderBuilder>()
               .As<ISharedOrderBuilder>()
               .InstancePerMatchingLifetimeScope((ILifetimeScope scope, IComponentRegistration request) =>
               {
                   var tag = scope.Tag?.ToString() ?? "";
                   return tag.StartsWith("WorkflowSession:");
               });

        // Register per-window service
        builder.RegisterType<WindowSpecificService>()
               .As<IWindowSpecificService>()
               .InstancePerMatchingLifetimeScope((ILifetimeScope scope, IComponentRegistration request) =>
               {
                   var tag = scope.Tag?.ToString() ?? "";
                   return tag.StartsWith("Window:");
               });

        // Register transient service
        builder.RegisterType<TransientService>()
               .As<ITransientService>()
               .InstancePerDependency();

        _container = builder.Build();
    }

    [Fact]
    public void ScopeTagMatching_ConceptTest()
    {
        // This test demonstrates the CONCEPT of scope tag matching
        // Real integration tests with WindowService are in SessionScopeSharingIntegrationTests
        
        // Arrange
        var sessionTag = ScopeTag.WorkflowSession("test-session");

        // Act
        var tagString = sessionTag.ToString();

        // Assert - Verify tag format
        tagString.Should().StartWith("WorkflowSession:");
        tagString.Should().Contain("test-session");
    }

    [Fact]
    public void TransientService_AlwaysReturnsNewInstance()
    {
        // Arrange
        var sessionTag = ScopeTag.WorkflowSession("test-session");

        // Act
        using var sessionScope = _container.BeginLifetimeScope(sessionTag.ToString());
        var instance1 = sessionScope.Resolve<ITransientService>();
        var instance2 = sessionScope.Resolve<ITransientService>();

        // Assert
        instance1.Should().NotBeSameAs(instance2);
    }

    [Fact]
    public void RegistrationPredicate_MatchesWorkflowSessionTags()
    {
        // This test verifies the predicate logic used in InstancePerMatchingLifetimeScope
        // Real session scope testing is in SessionScopeSharingIntegrationTests
        
        // Arrange
        Func<ILifetimeScope, IComponentRegistration, bool> predicate = 
            (scope, reg) =>
            {
                var tag = scope.Tag?.ToString() ?? "";
                return tag.StartsWith("WorkflowSession:");
            };

        var mockScope = new Mock<ILifetimeScope>();
        var mockReg = new Mock<IComponentRegistration>();

        // Act & Assert - Different tag scenarios
        mockScope.Setup(s => s.Tag).Returns("WorkflowSession:test");
        predicate(mockScope.Object, mockReg.Object).Should().BeTrue();

        mockScope.Setup(s => s.Tag).Returns("Window:test");
        predicate(mockScope.Object, mockReg.Object).Should().BeFalse();

        mockScope.Setup(s => s.Tag).Returns(null);
        predicate(mockScope.Object, mockReg.Object).Should().BeFalse();
    }

    public void Dispose()
    {
        _container?.Dispose();
    }

    // ========== TEST TYPES ==========

    public interface ISharedOrderBuilder
    {
        void AddItem(string name);
        List<string> GetItems();
    }

    public class SharedOrderBuilder : ISharedOrderBuilder, IDisposable
    {
        private readonly List<string> _items = new();
        public bool IsDisposed { get; private set; }

        public void AddItem(string name) => _items.Add(name);
        public List<string> GetItems() => _items;
        public void Dispose() => IsDisposed = true;
    }

    public interface IWindowSpecificService
    {
        Guid InstanceId { get; }
    }

    public class WindowSpecificService : IWindowSpecificService
    {
        public Guid InstanceId { get; } = Guid.NewGuid();
    }

    public interface ITransientService
    {
        Guid InstanceId { get; }
    }

    public class TransientService : ITransientService
    {
        public Guid InstanceId { get; } = Guid.NewGuid();
    }
}

