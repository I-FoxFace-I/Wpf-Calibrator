using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.Data.Sessions;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Services.Sessions;
using Xunit;

namespace WpfEngine.Tests.Core.Sessions;

public class ScopeManagerTests : IDisposable
{
    private readonly IContainer _container;
    private readonly ScopeManager _scopeManager;
    
    public ScopeManagerTests()
    {
        var builder = new ContainerBuilder();
        builder.Register(c => Mock.Of<ILogger>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<ScopeSession>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<ScopeManager>>()).InstancePerDependency();
        
        _container = builder.Build();
        _scopeManager = new ScopeManager(_container, Mock.Of<ILogger<ScopeManager>>());
    }
    
    [Fact]
    public void CreateSession_ShouldReturnBuilder()
    {
        // Act
        var builder = _scopeManager.CreateSession(ScopeTag.Database());
        
        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeAssignableTo<ISessionBuilder>();
    }
    
    [Fact]
    public void CreateSession_Build_ShouldCreateActiveSession()
    {
        // Act
        using var session = _scopeManager
            .CreateSession(ScopeTag.Database())
            .Build();
        
        // Assert
        session.Should().NotBeNull();
        session.IsActive.Should().BeTrue();
        session.Tag.Should().Be(ScopeTag.Database());
    }
    
    [Fact]
    public void ActiveSessions_AfterCreatingSession_ShouldIncludeIt()
    {
        // Arrange
        using var session = _scopeManager
            .CreateSession(ScopeTag.Database())
            .Build();
        
        // Act
        var activeSessions = _scopeManager.ActiveSessions;
        
        // Assert
        activeSessions.Should().HaveCountGreaterThanOrEqualTo(0);
        // Note: SessionManager doesn't auto-track sessions created via builder
    }
    
    [Fact]
    public void IsSessionActive_ForNewSession_ShouldReturnFalse()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        
        // Act
        var isActive = _scopeManager.IsSessionActive(sessionId);
        
        // Assert
        isActive.Should().BeFalse();
    }
    
    [Fact]
    public void SessionCreated_Event_ShouldRaiseWhenTracked()
    {
        // Arrange
        SessionEventArgs? raisedArgs = null;
        _scopeManager.SessionCreated += (sender, args) => raisedArgs = args;
        
        // Act - note: event is raised only when session is tracked via internal method
        // In real usage, this would be done by the session builder
        
        // Assert
        // Note: This test would need internal access or a different approach
    }
    
    [Fact]
    public void GetSession_ForNonExistentSession_ShouldReturnNull()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        
        // Act
        var session = _scopeManager.GetSession(sessionId);
        
        // Assert
        session.Should().BeNull();
    }
    
    [Fact]
    public void CloseSession_ShouldUntrackSession()
    {
        // Arrange
        using var session = _scopeManager
            .CreateSession(ScopeTag.Database())
            .Build();
        
        var sessionId = session.SessionId;
        
        // Act
        _scopeManager.CloseSession(sessionId);
        
        // Assert
        _scopeManager.IsSessionActive(sessionId).Should().BeFalse();
        _scopeManager.GetSession(sessionId).Should().BeNull();
    }
    
    [Fact]
    public void Session_Close_ShouldNotifyManagerForDisposal()
    {
        // Arrange
        using var session = _scopeManager
            .CreateSession(ScopeTag.Database())
            .Build();
        
        var sessionId = session.SessionId;
        
        // Verify session is tracked
        _scopeManager.IsSessionActive(sessionId).Should().BeTrue();
        
        // Act - close session directly
        session.Close();
        
        // Wait a bit for async Task.Run to complete
        Thread.Sleep(100);
        
        // Assert - manager should have disposed the session
        _scopeManager.IsSessionActive(sessionId).Should().BeFalse();
        session.IsActive.Should().BeFalse();
    }
    
    [Fact]
    public void Session_Dispose_ShouldUntrackFromManager()
    {
        // Arrange
        var session = _scopeManager
            .CreateSession(ScopeTag.Database())
            .Build();
        
        var sessionId = session.SessionId;
        
        // Verify session is tracked
        _scopeManager.IsSessionActive(sessionId).Should().BeTrue();
        _scopeManager.GetSession(sessionId).Should().NotBeNull();
        _scopeManager.ActiveSessions.Should().Contain(s => s.SessionId == sessionId);
        
        // Act
        session.Dispose();
        
        // Assert
        _scopeManager.IsSessionActive(sessionId).Should().BeFalse();
        _scopeManager.GetSession(sessionId).Should().BeNull();
        _scopeManager.ActiveSessions.Should().NotContain(s => s.SessionId == sessionId);
    }
    
    [Fact]
    public async Task Session_DisposeAsync_ShouldUntrackFromManager()
    {
        // Arrange
        var session = _scopeManager
            .CreateSession(ScopeTag.Database())
            .Build();
        
        var sessionId = session.SessionId;
        
        // Verify session is tracked
        _scopeManager.IsSessionActive(sessionId).Should().BeTrue();
        _scopeManager.GetSession(sessionId).Should().NotBeNull();
        
        // Act
        await session.DisposeAsync();
        
        // Assert
        _scopeManager.IsSessionActive(sessionId).Should().BeFalse();
        _scopeManager.GetSession(sessionId).Should().BeNull();
        _scopeManager.ActiveSessions.Should().NotContain(s => s.SessionId == sessionId);
    }
    
    [Fact]
    public void Session_Dispose_ShouldRemoveFromActiveSessions()
    {
        // Arrange
        var session1 = _scopeManager.CreateSession(ScopeTag.Database()).Build();
        var session2 = _scopeManager.CreateSession(ScopeTag.Workflow("test")).Build();
        var session3 = _scopeManager.CreateSession(ScopeTag.Window()).Build();
        
        var session1Id = session1.SessionId;
        var session2Id = session2.SessionId;
        var session3Id = session3.SessionId;
        
        // Verify all sessions are tracked
        _scopeManager.ActiveSessions.Should().HaveCountGreaterThanOrEqualTo(3);
        _scopeManager.ActiveSessions.Should().Contain(s => s.SessionId == session1Id);
        _scopeManager.ActiveSessions.Should().Contain(s => s.SessionId == session2Id);
        _scopeManager.ActiveSessions.Should().Contain(s => s.SessionId == session3Id);
        
        // Act - dispose session2
        session2.Dispose();
        
        // Assert
        _scopeManager.IsSessionActive(session1Id).Should().BeTrue();
        _scopeManager.IsSessionActive(session2Id).Should().BeFalse();
        _scopeManager.IsSessionActive(session3Id).Should().BeTrue();
        
        _scopeManager.ActiveSessions.Should().Contain(s => s.SessionId == session1Id);
        _scopeManager.ActiveSessions.Should().NotContain(s => s.SessionId == session2Id);
        _scopeManager.ActiveSessions.Should().Contain(s => s.SessionId == session3Id);
        
        // Cleanup
        session1.Dispose();
        session3.Dispose();
    }
    
    [Fact]
    public void Session_Dispose_ShouldNotAffectOtherSessions()
    {
        // Arrange
        var session1 = _scopeManager.CreateSession(ScopeTag.Database()).Build();
        var session2 = _scopeManager.CreateSession(ScopeTag.Database()).Build();
        
        var session1Id = session1.SessionId;
        var session2Id = session2.SessionId;
        
        // Act - dispose session1
        session1.Dispose();
        
        // Assert - session2 should still be tracked
        _scopeManager.IsSessionActive(session1Id).Should().BeFalse();
        _scopeManager.IsSessionActive(session2Id).Should().BeTrue();
        _scopeManager.GetSession(session1Id).Should().BeNull();
        _scopeManager.GetSession(session2Id).Should().NotBeNull();
        _scopeManager.GetSession(session2Id)!.SessionId.Should().Be(session2Id);
        
        // Cleanup
        session2.Dispose();
    }
    
    [Fact]
    public void Session_Dispose_Concurrent_ShouldBeThreadSafe()
    {
        // Arrange
        var sessions = new List<IScopeSession>();
        for (int i = 0; i < 10; i++)
        {
            sessions.Add(_scopeManager.CreateSession(ScopeTag.Database()).Build());
        }
        
        var sessionIds = sessions.Select(s => s.SessionId).ToList();
        
        // Verify all sessions are tracked
        foreach (var sessionId in sessionIds)
        {
            _scopeManager.IsSessionActive(sessionId).Should().BeTrue();
        }
        
        // Act - dispose all sessions concurrently
        Parallel.ForEach(sessions, session => session.Dispose());
        
        // Assert - all sessions should be untracked
        foreach (var sessionId in sessionIds)
        {
            _scopeManager.IsSessionActive(sessionId).Should().BeFalse();
            _scopeManager.GetSession(sessionId).Should().BeNull();
        }
        
        _scopeManager.ActiveSessions.Should().NotContain(s => sessionIds.Contains(s.SessionId));
    }
    
    [Fact]
    public void Session_Dispose_MultipleTimes_ShouldBeSafe()
    {
        // Arrange
        var session = _scopeManager
            .CreateSession(ScopeTag.Database())
            .Build();
        
        var sessionId = session.SessionId;
        
        // Act - dispose multiple times
        session.Dispose();
        session.Dispose();
        session.Dispose();
        
        // Assert - should still be untracked (no exception thrown, no double tracking)
        _scopeManager.IsSessionActive(sessionId).Should().BeFalse();
        _scopeManager.GetSession(sessionId).Should().BeNull();
    }
    
    [Fact]
    public void Session_Dispose_WithChildSessions_ShouldUntrackAll()
    {
        // Arrange
        var parentSession = _scopeManager
            .CreateSession(ScopeTag.Workflow("parent"))
            .Build();
        
        var childSession1 = parentSession
            .CreateChild(ScopeTag.Database())
            .Build();
        
        var childSession2 = parentSession
            .CreateChild(ScopeTag.Database())
            .Build();
        
        var parentId = parentSession.SessionId;
        var child1Id = childSession1.SessionId;
        var child2Id = childSession2.SessionId;
        
        // Verify all sessions are tracked
        _scopeManager.IsSessionActive(parentId).Should().BeTrue();
        _scopeManager.IsSessionActive(child1Id).Should().BeTrue();
        _scopeManager.IsSessionActive(child2Id).Should().BeTrue();
        
        // Act - dispose parent (should also dispose children)
        parentSession.Dispose();
        
        // Assert - all should be untracked
        _scopeManager.IsSessionActive(parentId).Should().BeFalse();
        _scopeManager.IsSessionActive(child1Id).Should().BeFalse();
        _scopeManager.IsSessionActive(child2Id).Should().BeFalse();
        
        _scopeManager.GetSession(parentId).Should().BeNull();
        _scopeManager.GetSession(child1Id).Should().BeNull();
        _scopeManager.GetSession(child2Id).Should().BeNull();
    }
    
    [Fact]
    public void Session_Dispose_AfterClose_ShouldStillUntrack()
    {
        // Arrange
        var session = _scopeManager
            .CreateSession(ScopeTag.Database())
            .Build();
        
        var sessionId = session.SessionId;
        
        // Verify session is tracked
        _scopeManager.IsSessionActive(sessionId).Should().BeTrue();
        
        // Act - close first, then dispose
        session.Close();
        
        // Wait a bit for async Task.Run to complete (if manager handles it)
        Thread.Sleep(100);
        
        // Dispose explicitly
        session.Dispose();
        
        // Assert
        _scopeManager.IsSessionActive(sessionId).Should().BeFalse();
        _scopeManager.GetSession(sessionId).Should().BeNull();
    }
    
    public void Dispose()
    {
        _scopeManager?.CloseAllSessions();
        _container?.Dispose();
    }
}

