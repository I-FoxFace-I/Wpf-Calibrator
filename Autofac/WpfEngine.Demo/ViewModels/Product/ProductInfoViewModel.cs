using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Products;
using WpfEngine.Demo.Models;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using WpfEngine.Demo.ViewModels.Parameters;
using WpfEngine.Abstract;
using WpfEngine.Services;

namespace WpfEngine.Demo.ViewModels;


public partial class ProductInfoViewModel : BaseViewModel, IInitializable
{
    private readonly IQueryHandler<GetDemoProductByIdQuery, DemoProduct?> _getProductHandler;
    private readonly IWindowContext _windowContext;
    private readonly int _productId;

    [ObservableProperty] 
    private DemoProduct? _product;

    public ProductInfoViewModel(
        ProductDetailParameters parameters,
        IQueryHandler<GetDemoProductByIdQuery, DemoProduct?> getProductHandler,
        IWindowContext windowContext,
        ILogger<ProductInfoViewModel> logger) : base(logger)
    {
        _getProductHandler = getProductHandler;
        _windowContext = windowContext;
        _productId = parameters.ProductId;
        Logger.LogInformation("[DEMO] ProductInfoViewModel created for {ProductId}", _productId);
    }

    public async Task InitializeAsync(CancellationToken cancelationToken = default)
    {
        try
        {
            IsBusy = true;
            ClearError();
            var product = await _getProductHandler.HandleAsync(new GetDemoProductByIdQuery (_productId));
            
            if (product == null)
            {
                SetError($"Product {_productId} not found");
                return;
            }

            Product = product;
            Logger.LogInformation("[DEMO] Loaded product info {ProductId}", _productId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO] Failed to load product");
            SetError("Failed to load product: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Close()
    {
        Logger.LogInformation("[DEMO] Closing product info");
        _windowContext.CloseWindow();
    }
}
