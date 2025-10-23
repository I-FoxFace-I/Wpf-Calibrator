using Autofac;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Views;

/// <summary>
/// Main menu window - application entry point
/// </summary>
public partial class MainWindow : ScopedWindow
{
    public MainWindow(
        ILifetimeScope parentScope,
        ILogger<MainWindow> logger) 
        : base(parentScope, logger, "main-menu")
    {
        InitializeComponent();
    }
}
