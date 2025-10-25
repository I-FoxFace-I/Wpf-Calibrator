using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WpfEngine.Core.Views.Windows;

namespace WpfEngine.Demo.Views;

/// <summary>
/// Base window class that creates and owns its child lifetime scope
/// ViewModel is set as DataContext from outside
/// Window is NOT IDisposable - scope is disposed internally on close
/// Automatically sets window reference in Navigator if present
/// </summary>
public abstract class ScopedWindow : WpfEngine.Core.Views.Windows.ScopedWindow
{
    private readonly ILifetimeScope _myScope;
    private readonly ILogger _logger;
    private bool _scopeDisposed;

    /// <summary>
    /// Creates window with its own child scope from parent scope
    /// </summary>
    protected ScopedWindow(ILifetimeScope parentScope, ILogger logger, object? scopeTag = null) : base(parentScope, logger)
    {
        //_myScope = parentScope.BeginLifetimeScope(scopeTag ?? GetType().Name);
        //_logger = logger;

        Logger.LogInformation("ScopedWindow [{WindowType}] created with own child scope", GetType().Name);

        //// Set window reference in Navigator if it exists in scope
        //if (_myScope.TryResolve<INavigationService>(out var navigator))
        //{
        //    navigator.SetWindow(this);
        //    _logger.LogInformation("Navigator associated with window [{WindowType}]", GetType().Name);
        //}

        //Closed += OnWindowClosed;
    }

    /// <summary>
    /// Gets this window's scope (for resolving services)
    /// </summary>
    public ILifetimeScope MyScope => _myScope;

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        _logger.LogInformation("ScopedWindow [{WindowType}] closed, disposing scope", GetType().Name);
        DisposeScope();
    }

    private void DisposeScope()
    {
        if (_scopeDisposed) return;

        _logger.LogInformation("ScopedWindow [{WindowType}] disposing scope - cascades to all children",
            GetType().Name);

        _myScope?.Dispose();
        _scopeDisposed = true;
    }
}



///// <summary>
///// Converter to show/hide content based on workflow step
///// </summary>
//public class StepVisibilityConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (value is int currentStep && parameter is string stepStr && int.TryParse(stepStr, out int step))
//        {
//            return currentStep == step ? Visibility.Visible : Visibility.Collapsed;
//        }
//        return Visibility.Collapsed;
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        throw new NotImplementedException();
//    }
//}

///// <summary>
///// Converter for step progress indicator colors
///// </summary>
//public class StepColorConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (value is int currentStep && parameter is string stepStr && int.TryParse(stepStr, out int step))
//        {
//            if (currentStep > step)
//                return Colors.Green; // Completed
//            if (currentStep == step)
//                return Color.FromRgb(59, 130, 246); // Active (Blue)
//            return Color.FromRgb(203, 213, 225); // Pending (Gray)
//        }
//        return Colors.Gray;
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        throw new NotImplementedException();
//    }
//}
