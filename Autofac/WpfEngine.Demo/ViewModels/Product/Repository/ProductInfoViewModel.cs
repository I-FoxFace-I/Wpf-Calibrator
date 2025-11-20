using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using WpfEngine.Abstract;
using WpfEngine.Data.Dialogs;
using WpfEngine.Data.Sessions;
using WpfEngine.Data.Windows.Events;
using WpfEngine.Data;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.Repositories;
using WpfEngine.Demo.Services;
using WpfEngine.Demo.ViewModels.Dialogs;
using WpfEngine.Demo.ViewModels.Parameters.Repository;
using WpfEngine.Demo.ViewModels.Workflow.Repository;
using WpfEngine.Extensions;
using WpfEngine.Services;
using WpfEngine.ViewModels.Dialogs;
using WpfEngine.ViewModels.Managed;
using WpfEngine.ViewModels;
using WpfEngine.Views.Windows;

namespace WpfEngine.Demo.ViewModels.Product.Repository;

/// <summary>
/// Product Info ViewModel using Repository pattern with Fluent API
/// Displays read-only product information
/// </summary>
public partial class ProductInfoViewModel : BaseViewModel, IInitializable
{
    private readonly IScopeManager _scopeManager;
    private readonly IWindowContext _windowContext;
    private readonly int _productId;

    [ObservableProperty] 
    private DemoProduct? _product;

    public ProductInfoViewModel(
        ProductDetailParameters parameters,
        IScopeManager scopeManager,
        IWindowContext windowContext,
        ILogger<ProductInfoViewModel> logger) : base(logger)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _windowContext = windowContext;
        _productId = parameters.ProductId;
        Logger.LogInformation("[DEMO_V2] ProductInfoViewModel created for {ProductId}", _productId);
    }

    public override async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();
            
            // Use Fluent API to load product
            var product = await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoProduct>>()
                .ExecuteWithResultAsync(async (repo) =>
                {
                    return await repo.GetByIdAsync(_productId);
                });
            
            if (product == null)
            {
                SetError($"Product {_productId} not found");
                Logger.LogWarning("[DEMO_V2] Product {ProductId} not found", _productId);
                return;
            }

            Product = product;
            Logger.LogInformation("[DEMO_V2] Loaded product info {ProductId}", _productId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Failed to load product");
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