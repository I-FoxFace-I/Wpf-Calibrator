using Autofac;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using WpfEngine.Services;
using WpfEngine.Services.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WpfEngine.Services.Autofac;

// ========== WINDOW TRACKER IMPLEMENTATION ==========

/// <summary>
/// Window tracker with advanced synchronization using ReaderWriterLockSlim
/// </summary>
public class WindowTracker : IWindowTracker
{
    private readonly ILogger<WindowTracker> _logger;

    // Main storage with reader-writer lock for optimal read performance
    private readonly ReaderWriterLockSlim _rwLock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly Dictionary<Guid, WindowMetadata> _windows = new Dictionary<Guid, WindowMetadata>();

    // Relationship tracking with ConcurrentDictionary for lock-free reads
    private readonly ConcurrentDictionary<Guid, IImmutableSet<Guid>> _parentToChildren = new ConcurrentDictionary<Guid, IImmutableSet<Guid>>();
    private readonly ConcurrentDictionary<Guid, IImmutableSet<Guid>> _sessionToWindows = new ConcurrentDictionary<Guid, IImmutableSet<Guid>>();

    public WindowTracker(ILogger<WindowTracker> logger)
    {
        _logger = logger;
        _logger.LogInformation("[WINDOW_TRACKER] Service initialized");
    }

    // ========== TRACKING ==========

    public void SetParent(Guid childId, Guid parentId)
    {
        WithMetadata(childId, metadata =>
        {
            // Remove from old parent
            if (metadata.ParentId.HasValue && metadata.ParentId.Value != parentId)
            {
                _parentToChildren.AddOrUpdate(
                    metadata.ParentId.Value,
                    ImmutableHashSet<Guid>.Empty,
                    (key, set) => set.Remove(childId));
            }

            // Set new parent
            metadata.ParentId = parentId;

            // Add to new parent
            _parentToChildren.AddOrUpdate(
                parentId,
                ImmutableHashSet.Create(childId),
                (key, set) => set.Add(childId));
        });

        _logger.LogDebug("[WINDOW_TRACKER] Set parent {ParentId} for window {ChildId}", parentId, childId);
    }

    public void AssociateWithSession(Guid windowId, Guid sessionId)
    {
        WithMetadata(windowId, metadata =>
        {
            // Remove from old session
            if (metadata.SessionId.HasValue && metadata.SessionId.Value != sessionId)
            {
                _sessionToWindows.AddOrUpdate(
                    metadata.SessionId.Value,
                    ImmutableHashSet<Guid>.Empty,
                    (key, set) => set.Remove(windowId));
            }

            // Set new session
            metadata.SessionId = sessionId;

            // Add to new session
            _sessionToWindows.AddOrUpdate(
                sessionId,
                ImmutableHashSet.Create(windowId),
                (key, set) => set.Add(windowId));
        });

        _logger.LogDebug("[WINDOW_TRACKER] Associated window {WindowId} with session {SessionId}",
            windowId, sessionId);
    }

    public void Track(Guid windowId, WindowMetadata metadata)
    {
        if (metadata == null) throw new ArgumentNullException(nameof(metadata));

        _rwLock.EnterWriteLock();

        try
        {
            _windows[windowId] = metadata;
            _logger.LogDebug("[WINDOW_TRACKER] Tracked window {WindowId} of type {ViewModelType}", 
                windowId, metadata.ViewModelType?.Name);

            if (metadata.ParentId.HasValue)
            {
                // Add to new parent
                _parentToChildren.AddOrUpdate(
                    metadata.ParentId.Value,
                    ImmutableHashSet.Create(windowId),
                    (key, set) => set.Add(windowId));

                _logger.LogDebug("[WINDOW_TRACKER] Set parent {ParentId} for window {ChildId}", 
                    metadata.ParentId, windowId);

            }
            if (metadata.SessionId.HasValue)
            {
                _sessionToWindows.AddOrUpdate(
                    metadata.SessionId.Value,
                    ImmutableHashSet.Create(windowId),
                    (key, set) => set.Add(windowId));

                _logger.LogDebug("[WINDOW_TRACKER] Associated window {WindowId} with session {SessionId}",
                    windowId, metadata.SessionId);
            }
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public void Untrack(Guid windowId)
    {
        WindowMetadata? metadata = null;

        _rwLock.EnterWriteLock();
        try
        {
            if (_windows.TryGetValue(windowId, out metadata))
            {
                _windows.Remove(windowId);

                // Clean up relationships
                if (metadata?.ParentId != null)
                {
                    _parentToChildren.AddOrUpdate(
                        metadata.ParentId.Value,
                        ImmutableHashSet<Guid>.Empty,
                        (key, set) => set.Remove(windowId));
                }

                if (metadata?.SessionId != null)
                {
                    _sessionToWindows.AddOrUpdate(
                        metadata.SessionId.Value,
                        ImmutableHashSet<Guid>.Empty,
                        (key, set) => set.Remove(windowId));
                }

                // Remove as parent
                _parentToChildren.TryRemove(windowId, out _);

                _logger.LogDebug("[WINDOW_TRACKER] Untracked window {WindowId}", windowId);

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WINDOW_TRACKER] Failed to untracked window {WindowId}", windowId);
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public TResult? WithMetadata<TResult>(Guid windowId, Func<WindowMetadata, TResult> func, TResult? defaultValue = default)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));

        TResult? result = defaultValue;
        _rwLock.EnterUpgradeableReadLock();
        try
        {
            if (_windows.TryGetValue(windowId, out var metadata))
            {
                _rwLock.EnterWriteLock();
                try
                {
                    result = func(metadata);
                    _logger.LogDebug("[WINDOW_TRACKER] Updated metadata {WindowId} wit result {Result}", windowId, result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[WINDOW_TRACKER] Failed to update window {WindowId}", windowId);
                    result = defaultValue;
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WINDOW_TRACKER] Failed to update window {WindowId}", windowId);
            result = defaultValue;
        }
        finally
        {
            _rwLock.ExitUpgradeableReadLock();
        }

        return result;
    }

    public void WithMetadata(Guid windowId, Action<WindowMetadata> update)
    {
        if (update == null) throw new ArgumentNullException(nameof(update));

        _rwLock.EnterUpgradeableReadLock();
        try
        {
            if (_windows.TryGetValue(windowId, out var metadata))
            {
                _rwLock.EnterWriteLock();
                try
                {
                    update(metadata);
                    _logger.LogDebug("[WINDOW_TRACKER] Updated window {WindowId}", windowId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[WINDOW_TRACKER] Failed to update window {WindowId}", windowId);
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WINDOW_TRACKER] Failed to update window {WindowId}", windowId);
        }
        finally
        {
            _rwLock.ExitUpgradeableReadLock();
        }
    }

    // ========== RETRIEVAL ==========

    public bool TryGetMetadata(Guid windowId, [NotNullWhen(true)] out WindowMetadata? metadata)
    {
        metadata = null;

        bool result = false;

        try
        {
            _rwLock.EnterReadLock();

            if (_windows.TryGetValue(windowId, out metadata) && metadata is WindowMetadata)
            {
                _logger.LogDebug("[WINDOW_TRACKER] Metadata retrieved for window {WindowId}.", windowId);

                result = true;
            }
            else
            {
                _logger.LogDebug("[WINDOW_TRACKER] Metadata not retrieved for window {WindowId}.", windowId);
            }


        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WINDOW_TRACKER] Failed to get window  {WindowId} metadata.", windowId);

            result = false;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }

        return result;
    }

    public WindowMetadata? GetMetadata(Guid windowId)
    {
        if (TryGetMetadata(windowId, out WindowMetadata? metadata))
        {
            return metadata;
        }

        return null;
    }

    public IReadOnlyList<WindowMetadata> Find(Predicate<WindowMetadata> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        _rwLock.EnterReadLock();
        try
        {
            return _windows.Values.Where(x => predicate(x)).ToList().AsReadOnly();
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    public IReadOnlyList<Guid> OpenWindows => GetOpenWindows();

    // ========== SCOPE MANAGEMENT ==========

    public ILifetimeScope? GetWindowScope(Guid windowId)
    {
        _rwLock.EnterReadLock();
        try
        {
            if(_windows.TryGetValue(windowId, out var metadata))
            {
                return metadata.WindowScope;
            }
            return null;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }


        ////var metadata = GetMetadata(windowId);
        //if (metadata?.WindowScope == null)
        //{
        //    return GetParentScope(windowId);
        //}
        //return metadata?.WindowScope;
    }

    public ILifetimeScope? GetParentScope(Guid windowId)
    {
        var metadata = GetMetadata(windowId);
        if (metadata?.ParentId != null)
        {
            var parentMetadata = GetMetadata(metadata.ParentId.Value);
            return parentMetadata?.WindowScope;
        }
        return null;
    }

    // ========== RELATIONSHIPS ==========

    public Guid? GetParent(Guid windowId)
    {
        if (TryGetMetadata(windowId, out var metadata))
        {
            return metadata.ParentId;
        }

        return null;
    }

    public Guid? GetSessionId(Guid windowId)
    {
        if(TryGetMetadata(windowId, out var metadata))
        {
            return metadata.SessionId;
        }

        return null;
    }

    public IReadOnlyList<Guid> GetChildWindows(Guid parentId)
    {
        return _parentToChildren.TryGetValue(parentId, out var children)
            ? children.ToList().AsReadOnly()
            : new List<Guid>().AsReadOnly();
    }

    public IReadOnlyList<Guid> GetSessionWindows(Guid sessionId)
    {
        return _sessionToWindows.TryGetValue(sessionId, out var windows)
            ? windows.ToList().AsReadOnly()
            : new List<Guid>().AsReadOnly();
    }

    public IReadOnlyList<Guid> GetDescendants(Guid parentId)
    {
        var descendants = new List<Guid>();
        var toProcess = new Queue<Guid>();
        toProcess.Enqueue(parentId);

        while (toProcess.Count > 0)
        {
            var current = toProcess.Dequeue();
            var children = GetChildWindows(current);

            foreach (var child in children)
            {
                descendants.Add(child);
                toProcess.Enqueue(child);
            }

        }

        return descendants.AsReadOnly();
    }

    // ========== SESSION ASSOCIATION ==========

    private IReadOnlyList<Guid> GetOpenWindows()
    {
        _rwLock.EnterReadLock();
        try
        {
            return _windows.Keys.ToList().AsReadOnly();
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }
    public bool IsWindowOpen(Guid windowId)
    {
        _rwLock.EnterReadLock();
        try
        {
            return _windows.Any(x => x.Key == windowId && x.Value.IsOpened());
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }


    public Type? GetWindowViewModelType(Guid windowId)
    {
        if (TryGetMetadata(windowId, out var metadata))
        {
            return metadata.ViewModelType;
        }

        return null;
    }
}