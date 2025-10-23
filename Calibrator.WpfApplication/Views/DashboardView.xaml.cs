using System;
using System.Windows;
using Calibrator.WpfApplication.ViewModels;

namespace Calibrator.WpfApplication.Views;

public partial class DashboardView : Window
{
    public DashboardView(DashboardViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
    
    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        // TODO: Add splash screen closing logic if needed
    }
}
