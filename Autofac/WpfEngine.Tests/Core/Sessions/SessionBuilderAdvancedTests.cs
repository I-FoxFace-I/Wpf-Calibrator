using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using WpfEngine.Data.Sessions;
using WpfEngine.Services.Autofac;
using WpfEngine.Services.Sessions;
using WpfEngine.Services.Sessions.Implementation;
using Xunit;

namespace WpfEngine.Tests.Core.Sessions;

/// <summary>
/// Advanced tests for SessionBuilder with multiple generic parameters (T1, T2, T3, T4)
/// </summary>
public class SessionBuilderAdvancedTests : IDisposable
{
    private readonly IContainer _container;
    private readonly ILifetimeScope _scope;
    
    public SessionBuilderAdvancedTests()
    {
        var builder = new ContainerBuilder();
        builder.Register(c => Mock.Of<ILogger>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<ScopeSession>>()).InstancePerDependency();
        builder.RegisterType<TestService1>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<TestService2>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<TestService3>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<TestService4>().AsSelf().InstancePerLifetimeScope();
        
        _container = builder.Build();
        _scope = _container.BeginLifetimeScope();
    }
    
    [Fact]
    public void WithService_T1T2T3_ShouldCreateTypedBuilder()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        
        // Act
        var typedBuilder = builder
            .WithService<TestService1>()
            .WithService<TestService2>()
            .WithService<TestService3>();
        
        // Assert
        typedBuilder.Should().NotBeNull();
        typedBuilder.Should().BeAssignableTo<ISessionBuilder<TestService1, TestService2, TestService3>>();
    }
    
    [Fact]
    public void Execute_T1T2T3_ShouldResolveAllServices()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var executed = false;
        
        // Act
        builder
            .WithService<TestService1>()
            .WithService<TestService2>()
            .WithService<TestService3>()
            .Execute((s1, s2, s3) =>
            {
                s1.Should().NotBeNull();
                s2.Should().NotBeNull();
                s3.Should().NotBeNull();
                executed = true;
            });
        
        // Assert
        executed.Should().BeTrue();
    }
    
    [Fact]
    public void ExecuteWithResult_T1T2T3_ShouldReturnResult()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        
        // Act
        var result = builder
            .WithService<TestService1>()
            .WithService<TestService2>()
            .WithService<TestService3>()
            .ExecuteWithResult((s1, s2, s3) =>
            {
                return $"{s1.GetValue()}+{s2.GetValue()}+{s3.GetValue()}";
            }, defaultValue: "");
        
        // Assert
        result.Should().Be("1+2+3");
    }
    
    [Fact]
    public async Task ExecuteAsync_T1T2T3_ShouldExecuteAsync()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var executed = false;
        
        // Act
        await builder
            .WithService<TestService1>()
            .WithService<TestService2>()
            .WithService<TestService3>()
            .ExecuteAsync(async (s1, s2, s3) =>
            {
                await Task.Delay(10);
                s1.Should().NotBeNull();
                s2.Should().NotBeNull();
                s3.Should().NotBeNull();
                executed = true;
            });
        
        // Assert
        executed.Should().BeTrue();
    }
    
    [Fact]
    public async Task ExecuteWithResultAsync_T1T2T3_ShouldReturnResult()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        
        // Act
        var result = await builder
            .WithService<TestService1>()
            .WithService<TestService2>()
            .WithService<TestService3>()
            .ExecuteWithResultAsync(async (s1, s2, s3) =>
            {
                await Task.Delay(10);
                return $"{s1.GetValue()}+{s2.GetValue()}+{s3.GetValue()}";
            }, defaultValue: "");
        
        // Assert
        result.Should().Be("1+2+3");
    }
    
    [Fact]
    public void WithService_T1T2T3T4_ShouldCreateTypedBuilder()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        
        // Act
        var typedBuilder = builder
            .WithService<TestService1>()
            .WithService<TestService2>()
            .WithService<TestService3>()
            .WithService<TestService4>();
        
        // Assert
        typedBuilder.Should().NotBeNull();
        typedBuilder.Should().BeAssignableTo<ISessionBuilder<TestService1, TestService2, TestService3, TestService4>>();
    }
    
    [Fact]
    public void Execute_T1T2T3T4_ShouldResolveAllServices()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var executed = false;
        
        // Act
        builder
            .WithService<TestService1>()
            .WithService<TestService2>()
            .WithService<TestService3>()
            .WithService<TestService4>()
            .Execute((s1, s2, s3, s4) =>
            {
                s1.Should().NotBeNull();
                s2.Should().NotBeNull();
                s3.Should().NotBeNull();
                s4.Should().NotBeNull();
                executed = true;
            });
        
        // Assert
        executed.Should().BeTrue();
    }
    
    [Fact]
    public void ExecuteWithResult_T1T2T3T4_ShouldReturnResult()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        
        // Act
        var result = builder
            .WithService<TestService1>()
            .WithService<TestService2>()
            .WithService<TestService3>()
            .WithService<TestService4>()
            .ExecuteWithResult((s1, s2, s3, s4) =>
            {
                return $"{s1.GetValue()}+{s2.GetValue()}+{s3.GetValue()}+{s4.GetValue()}";
            }, defaultValue: "");
        
        // Assert
        result.Should().Be("1+2+3+4");
    }
    
    [Fact]
    public async Task ExecuteAsync_T1T2T3T4_ShouldExecuteAsync()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var executed = false;
        
        // Act
        await builder
            .WithService<TestService1>()
            .WithService<TestService2>()
            .WithService<TestService3>()
            .WithService<TestService4>()
            .ExecuteAsync(async (s1, s2, s3, s4) =>
            {
                await Task.Delay(10);
                s1.Should().NotBeNull();
                s2.Should().NotBeNull();
                s3.Should().NotBeNull();
                s4.Should().NotBeNull();
                executed = true;
            });
        
        // Assert
        executed.Should().BeTrue();
    }
    
    [Fact]
    public async Task ExecuteWithResultAsync_T1T2T3T4_ShouldReturnResult()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        
        // Act
        var result = await builder
            .WithService<TestService1>()
            .WithService<TestService2>()
            .WithService<TestService3>()
            .WithService<TestService4>()
            .ExecuteWithResultAsync(async (s1, s2, s3, s4) =>
            {
                await Task.Delay(10);
                return $"{s1.GetValue()}+{s2.GetValue()}+{s3.GetValue()}+{s4.GetValue()}";
            }, defaultValue: "");
        
        // Assert
        result.Should().Be("1+2+3+4");
    }
    
    [Fact]
    public void WithService_Chained_ShouldAllowFluentConfiguration()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var hookExecuted = false;
        
        // Act
        var typedBuilder = builder
            .WithService<TestService1>()
            .WithService<TestService2>();
        
        var result = typedBuilder.ExecuteWithResult((s1, s2) =>
        {
            return s1.GetValue() + s2.GetValue();
        }, defaultValue: 0);
        
        // Assert
        result.Should().Be(3);
    }
    
    [Fact]
    public void Execute_T1T2T3_WithOnError_ShouldCallErrorHandler()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var errorHandlerCalled = false;
        
        // Act
        try
        {
            builder
                .WithService<TestService1>()
                .WithService<TestService2>()
                .WithService<TestService3>()
                .Execute((s1, s2, s3) =>
                {
                    throw new InvalidOperationException("Test error");
                }, 
                onError: ex => 
                {
                    errorHandlerCalled = true;
                });
        }
        catch
        {
            // Exception is still thrown
        }
        
        // Assert
        errorHandlerCalled.Should().BeTrue();
    }
    
    [Fact]
    public void ExecuteWithResult_T1T2T3_WithDefaultValue_ShouldReturnDefaultOnException()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var defaultValue = "Error occurred";
        
        // Act
        var result = builder
            .WithService<TestService1>()
            .WithService<TestService2>()
            .WithService<TestService3>()
            .ExecuteWithResult((s1, s2, s3) =>
            {
                throw new InvalidOperationException("Test error");
            }, 
            defaultValue: defaultValue);
        
        // Assert
        result.Should().Be(defaultValue);
    }
    
    [Fact]
    public async Task ExecuteWithResultAsync_T1T2T3T4_WithOnError_ShouldCallErrorHandlerAndReturnDefault()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var defaultValue = 0;
        var errorHandlerCalled = false;
        
        // Act
        var result = await builder
            .WithService<TestService1>()
            .WithService<TestService2>()
            .WithService<TestService3>()
            .WithService<TestService4>()
            .ExecuteWithResultAsync(async (s1, s2, s3, s4) =>
            {
                await Task.Delay(10);
                throw new InvalidOperationException("Test error");
            }, 
            defaultValue: defaultValue,
            onError: ex => 
            {
                errorHandlerCalled = true;
            });
        
        // Assert
        result.Should().Be(defaultValue);
        errorHandlerCalled.Should().BeTrue();
    }
    
    [Fact]
    public void ExecuteWithResult_T1T2T3T4_WithDefaultValue_ShouldReturnActualResultOnSuccess()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var defaultValue = "Error";
        
        // Act
        var result = builder
            .WithService<TestService1>()
            .WithService<TestService2>()
            .WithService<TestService3>()
            .WithService<TestService4>()
            .ExecuteWithResult((s1, s2, s3, s4) =>
            {
                return $"{s1.GetValue()}+{s2.GetValue()}+{s3.GetValue()}+{s4.GetValue()}";
            }, 
            defaultValue: defaultValue);
        
        // Assert
        result.Should().Be("1+2+3+4");
        result.Should().NotBe(defaultValue);
    }
    
    public void Dispose()
    {
        _scope?.Dispose();
        _container?.Dispose();
    }
    
    // Test services
    private class TestService1
    {
        public int GetValue() => 1;
    }
    
    private class TestService2
    {
        public int GetValue() => 2;
    }
    
    private class TestService3
    {
        public int GetValue() => 3;
    }
    
    private class TestService4
    {
        public int GetValue() => 4;
    }
}

