using Microsoft.Extensions.Logging;

namespace WpfEngine.Demo.Views.Repository;

/// <summary>
/// Main Window for Repository Pattern Demo
/// </summary>
public partial class MainWindow : WpfEngine.Views.Windows.ScopedWindow
{
    public MainWindow(ILogger<MainWindow> logger) : base(logger)
    {
        InitializeComponent();
    }
}
