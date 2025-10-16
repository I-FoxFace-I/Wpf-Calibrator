using System.Windows.Controls;
using Calibrator.WpfControl.Sandbox.ViewModels;

namespace Calibrator.WpfControl.Sandbox.Views;

public partial class DataLoadingDemo : UserControl
{
    public DataLoadingDemo()
    {
        InitializeComponent();
        
        // Initialize the ViewModel
        if (DataContext is DataLoadingDemoViewModel viewModel)
        {
            _ = viewModel.InitializeAsync();
        }
    }
}

