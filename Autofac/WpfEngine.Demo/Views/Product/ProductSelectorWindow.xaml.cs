using Autofac;
using WpfEngine.Demo.ViewModels;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace WpfEngine.Demo.Views;

public partial class ProductSelectorWindow : ScopedWindow
{
    public ProductSelectorWindow(ILogger<ProductSelectorWindow> logger)
        : base(logger)
    {
        InitializeComponent();
        Loaded += async (s, e) => await OnLoadedAsync();
    }

    private async Task OnLoadedAsync()
    {
        if (DataContext is ProductSelectorViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}

