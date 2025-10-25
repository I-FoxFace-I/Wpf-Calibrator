using Autofac;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Demo.Views;

/// <summary>
/// ScopedWindow for WpfEngine.Demo
/// Inherits from WpfEngine.Core.Views.Windows.ScopedWindow
/// Provides Demo-specific window base class
/// </summary>
public abstract class ScopedWindow : WpfEngine.Core.Views.Windows.ScopedWindow
{
    protected ScopedWindow(
        ILifetimeScope parentScope,
        ILogger logger,
        object? scopeTag = null) : base(parentScope, logger, scopeTag)
    {
        // Base class handles scope creation and disposal
    }
}

