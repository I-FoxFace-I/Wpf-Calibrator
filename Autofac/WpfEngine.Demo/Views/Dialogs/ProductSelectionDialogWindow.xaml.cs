using Microsoft.Extensions.Logging;
using WpfEngine.Demo.ViewModels.Dialogs;
using WpfEngine.Enums;
using WpfEngine.Views.Windows;

namespace WpfEngine.Demo.Views.Dialogs;

/// <summary>
/// Product selection dialog window
/// </summary>
public partial class ProductSelectionDialogWindow : DialogWindow
{
    public ProductSelectionDialogWindow(ILogger<ProductSelectionDialogWindow> logger) : base(logger)
    {
        InitializeComponent();
        
        // Handle dialog close request from ViewModel
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }
    
    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ProductSelectionDialogViewModel viewModel)
        {
            viewModel.OnRequestClose += OnViewModelRequestClose;
        }
    }
    
    private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ProductSelectionDialogViewModel viewModel)
        {
            viewModel.OnRequestClose -= OnViewModelRequestClose;
        }
    }
    
    private void OnViewModelRequestClose(object? sender, bool dialogResult)
    {
        DialogResult = dialogResult;
        Close();
    }
    
    public override DialogType DialogType => DialogType.Selection;
    
    public override string? AppModule => "ProductCatalog";
}