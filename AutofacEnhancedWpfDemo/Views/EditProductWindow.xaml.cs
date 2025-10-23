using Autofac;
using AutofacEnhancedWpfDemo.ViewModels;
using AutofacEnhancedWpfDemo.Views;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AutofacEnhancedWpfDemo.Views;

/// <summary>
/// Edit/Create product dialog window
/// </summary>
public partial class EditProductWindow : ScopedWindow
{
    public EditProductWindow(
        ILifetimeScope parentScope,
        ILogger<EditProductWindow> logger)
        : base(parentScope, logger, "edit-product")
    {
        InitializeComponent();

        // ViewModel will be set as DataContext by Navigator
        Loaded += async (s, e) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        if (DataContext is EditProductViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}