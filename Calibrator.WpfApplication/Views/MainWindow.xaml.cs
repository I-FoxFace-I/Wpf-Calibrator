using System.Windows;
using Calibrator.WpfApplication.ViewModels;

namespace Calibrator.WpfApplication.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        
        // Initialize ViewModel
        Loaded += async (_, _) =>
        {
            await viewModel.InitializeAsync();
        };
    }
}

