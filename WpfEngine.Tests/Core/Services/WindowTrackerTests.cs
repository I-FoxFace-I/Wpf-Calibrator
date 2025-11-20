using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.Enums;
using WpfEngine.Services.Autofac;
using WpfEngine.Services.Metadata;
using Xunit;

namespace WpfEngine.Tests.Core.Services.Sessions;

public class WindowTrackerTests
{
    private readonly WindowTracker _tracker;
    private readonly Mock<ILogger<WindowTracker>> _loggerMock;

    public WindowTrackerTests()
    {
        _loggerMock = new Mock<ILogger<WindowTracker>>();
        _tracker = new WindowTracker(_loggerMock.Object);
    }

    [Fact]
    public void Track_AddsWindowMetadata()
    {
        // Arrange
        var windowId = Guid.NewGuid();
        var metadata = new WindowMetadata
        {
            WindowId = windowId,
            ViewModelType = typeof(TestViewModel),
            Lifecycle = WindowLifecycleState.Open
        };

        // Act
        _tracker.Track(windowId, metadata);

        // Assert
        //var retrieved = _tracker.GetMetadata(windowId);
        _tracker.TryGetMetadata(windowId, out var retrieved).Should().BeTrue();

        retrieved.Should().NotBeNull();
        retrieved!.WindowId.Should().Be(windowId);
        retrieved.ViewModelType.Should().Be(typeof(TestViewModel));
    }

    [Fact]
    public void Untrack_RemovesWindowMetadata()
    {
        // Arrange
        var windowId = Guid.NewGuid();
        var metadata = new WindowMetadata { WindowId = windowId };
        _tracker.Track(windowId, metadata);

        // Act
        _tracker.Untrack(windowId);

        // Assert
        //var retrieved = _tracker.GetMetadata(windowId);
        _tracker.TryGetMetadata(windowId, out var retrieved).Should().BeFalse();

        retrieved.Should().BeNull();
    }

    [Fact]
    public void Update_ModifiesExistingMetadata()
    {
        // Arrange
        var windowId = Guid.NewGuid();
        var metadata = new WindowMetadata
        {
            WindowId = windowId,
            Lifecycle = WindowLifecycleState.Open
        };
        _tracker.Track(windowId, metadata);

        // Act
        _tracker.WithMetadata(windowId, m => m.Lifecycle = WindowLifecycleState.Closing);

        // Assert
        //var retrieved = _tracker.GetMetadata(windowId);

        _tracker.TryGetMetadata(windowId, out var retrieved).Should().BeTrue();

        retrieved.Should().NotBeNull();
        retrieved!.Lifecycle.Should().Be(WindowLifecycleState.Closing);
    }

    [Fact]
    public void Find_ReturnsMatchingWindows()
    {
        // Arrange
        var window1 = Guid.NewGuid();
        var window2 = Guid.NewGuid();
        var window3 = Guid.NewGuid();

        _tracker.Track(window1, new WindowMetadata
        {
            WindowId = window1,
            ViewModelType = typeof(TestViewModel)
        });
        _tracker.Track(window2, new WindowMetadata
        {
            WindowId = window2,
            ViewModelType = typeof(TestViewModel)
        });
        _tracker.Track(window3, new WindowMetadata
        {
            WindowId = window3,
            ViewModelType = typeof(TestViewModel)
        });

        // Act
        var results = _tracker.Find(m => m.ViewModelType == typeof(TestViewModel));

        // Assert
        results.Should().HaveCount(3);
        results.Select(m => m.WindowId).Should().Contain(new[] { window1, window2, window3 });
    }

    [Fact]
    public void SetParent_EstablishesParentChildRelationship()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        _tracker.Track(parentId, new WindowMetadata { WindowId = parentId });
        _tracker.Track(childId, new WindowMetadata { WindowId = childId });

        // Act
        _tracker.SetParent(childId, parentId);

        // Assert
        _tracker.GetParent(childId).Should().Be(parentId);
        _tracker.GetChildWindows(parentId).Should().Contain(childId);
    }

    [Fact]
    public void GetDescendants_ReturnsAllDescendantsRecursively()
    {
        // Arrange
        var grandparentId = Guid.NewGuid();
        var parent1Id = Guid.NewGuid();
        var parent2Id = Guid.NewGuid();
        var child1Id = Guid.NewGuid();
        var child2Id = Guid.NewGuid();
        var child3Id = Guid.NewGuid();

        _tracker.Track(grandparentId, new WindowMetadata { WindowId = grandparentId });
        _tracker.Track(parent1Id, new WindowMetadata { WindowId = parent1Id });
        _tracker.Track(parent2Id, new WindowMetadata { WindowId = parent2Id });
        _tracker.Track(child1Id, new WindowMetadata { WindowId = child1Id });
        _tracker.Track(child2Id, new WindowMetadata { WindowId = child2Id });
        _tracker.Track(child3Id, new WindowMetadata { WindowId = child3Id });

        _tracker.SetParent(parent1Id, grandparentId);
        _tracker.SetParent(parent2Id, grandparentId);
        _tracker.SetParent(child1Id, parent1Id);
        _tracker.SetParent(child2Id, parent1Id);
        _tracker.SetParent(child3Id, parent2Id);

        // Act
        var descendants = _tracker.GetDescendants(grandparentId);

        // Assert
        descendants.Should().HaveCount(5);
        descendants.Should().Contain(new[] { parent1Id, parent2Id, child1Id, child2Id, child3Id });
    }

    [Fact]
    public void AssociateWithSession_TracksSessionWindows()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var window1Id = Guid.NewGuid();
        var window2Id = Guid.NewGuid();

        _tracker.Track(window1Id, new WindowMetadata { WindowId = window1Id, SessionId = sessionId });
        _tracker.Track(window2Id, new WindowMetadata { WindowId = window2Id, SessionId = sessionId });

        // Act
        //_tracker.AssociateWithSession(window1Id, sessionId);
        //_tracker.AssociateWithSession(window2Id, sessionId);

        // Assert
        _tracker.GetSessionId(window1Id).Should().Be(sessionId);
        _tracker.GetSessionId(window2Id).Should().Be(sessionId);
        _tracker.GetSessionWindows(sessionId).Should().Contain(new[] { window1Id, window2Id });
    }

    [Fact]
    public async Task ConcurrentOperations_HandleSafelyAsync()
    {
        // Arrange
        var tasks = new List<Task>();
        var windowCount = 100;
        var random = new Random();

        // Act - Perform many concurrent operations
        for (int i = 0; i < windowCount; i++)
        {
            var windowId = Guid.NewGuid();

            tasks.Add(Task.Run(() =>
            {
                // Track window
                _tracker.Track(windowId, new WindowMetadata
                {
                    WindowId = windowId,
                    Lifecycle = WindowLifecycleState.Open
                });

                // Random delay
                Thread.Sleep(random.Next(1, 10));

                // Update window
                _tracker.WithMetadata(windowId, m => m.Lifecycle = WindowLifecycleState.Closing);

                // Get window
                var metadata = _tracker.GetMetadata(windowId);

                // Find windows
                var windows = _tracker.Find(m => m.WindowId == windowId);

                // Untrack some windows randomly
                if (random.Next(2) == 0)
                {
                    _tracker.Untrack(windowId);
                }
            }));
        }

        // Assert - Should complete without exceptions
        try
        {
            await Task.WhenAll(tasks);
        }
        catch
        {
            Assert.Fail();
        }
        finally
        {
            Assert.True(true);
        }
    }
}