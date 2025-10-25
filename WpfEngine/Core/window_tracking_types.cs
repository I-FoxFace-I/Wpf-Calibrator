using System;
using System.Windows;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Services.WindowTracking;

/// <summary>
/// Key for identifying ViewModels in window tracking
/// Uses ViewModel's Guid Id as unique identifier
/// </summary>
public readonly struct VmKey : IEquatable<VmKey>
{
    public Guid Id { get; }
    public Type ViewModelType { get; }

    public VmKey(Guid id, Type viewModelType)
    {
        Id = id;
        ViewModelType = viewModelType ?? throw new ArgumentNullException(nameof(viewModelType));
    }

    public bool Equals(VmKey other)
    {
        return Id.Equals(other.Id) && ViewModelType == other.ViewModelType;
    }

    public override bool Equals(object? obj)
    {
        return obj is VmKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, ViewModelType);
    }

    public static bool operator ==(VmKey left, VmKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(VmKey left, VmKey right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"{ViewModelType.Name}({Id})";
    }
}

/// <summary>
/// Information about tracked window
/// Uses WeakReferences to allow GC cleanup
/// ViewModelRef uses IViewModel for type safety
/// </summary>
internal class WindowInfo
{
    public Guid WindowId { get; init; }
    public VmKey VmKey { get; init; }
    public WeakReference<Window> WindowRef { get; init; } = null!;
    public WeakReference<IViewModel> ViewModelRef { get; init; } = null!;
    public Guid? ParentWindowId { get; init; }

    /// <summary>
    /// Try to get window if still alive
    /// </summary>
    public bool TryGetWindow(out Window? window)
    {
        return WindowRef.TryGetTarget(out window);
    }

    /// <summary>
    /// Try to get ViewModel if still alive
    /// </summary>
    public bool TryGetViewModel(out IViewModel? viewModel)
    {
        return ViewModelRef.TryGetTarget(out viewModel);
    }

    /// <summary>
    /// Check if window and ViewModel are still alive
    /// </summary>
    public bool IsAlive => WindowRef.TryGetTarget(out _) && ViewModelRef.TryGetTarget(out _);
}

/// <summary>
/// Extension methods for window tracking
/// Specifically designed for BaseViewModel tracking
/// </summary>
public static class WindowTrackingExtensions
{
    /// <summary>
    /// Gets VmKey from ViewModel instance
    /// Works with any IViewModel implementation (BaseViewModel preferred)
    /// </summary>
    public static VmKey GetVmKey(this IViewModel viewModel)
    {
        if (viewModel == null)
            throw new ArgumentNullException(nameof(viewModel));

        if (viewModel is IViewModel vm)
        {
            return new VmKey(vm.Id, vm.GetType());
        }
        else
        {
            throw new InvalidOperationException(
                $"ViewModel {viewModel.GetType().Name} does not implement IViewModel or have Guid Id property. " +
                "Ensure it derives from BaseViewModel or implements IViewModel.");
        }
    }

    /// <summary>
    /// Checks if object is a valid trackable ViewModel
    /// </summary>
    public static bool IsTrackableViewModel(this object viewModel)
    {
        if (viewModel == null)
            return false;

        // Check if implements IViewModel
        if (viewModel is IViewModel)
            return true;

        // Check if has Guid Id property
        var vmType = viewModel.GetType();
        var idProperty = vmType.GetProperty("Id");
        return idProperty?.PropertyType == typeof(Guid);
    }
}
