using System.Collections.ObjectModel;
using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Demo.Products;
using AutofacEnhancedWpfDemo.Models.Demo;
using AutofacEnhancedWpfDemo.Services.Demo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels.Demo;

// ========== DEMO PRODUCT LIST ==========

public partial class DemoProductListViewModel : BaseViewModel
{
    private readonly IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> _getAllHandler;
    private readonly ICommandHandler<DeleteDemoProductCommand> _deleteHandler;
    private readonly IWindowManager _windowManager;
    
    [ObservableProperty]
    private ObservableCollection<DemoProduct> _products = new();
    
    [ObservableProperty]
    private DemoProduct? _selectedProduct;
    
    public DemoProductListViewModel(
        IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> getAllHandler,
        ICommandHandler<DeleteDemoProductCommand> deleteHandler,
        IWindowManager windowManager,
        ILogger<DemoProductListViewModel> logger) : base(logger)
    {
        _getAllHandler = getAllHandler;
        _deleteHandler = deleteHandler;
        _windowManager = windowManager;
    }
    
    public async Task InitializeAsync()
    {
        await LoadProductsAsync();
    }
    
    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();
            
            var products = await _getAllHandler.HandleAsync(new GetAllDemoProductsQuery());
            
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
            
            Logger.LogInformation("[DEMO] Loaded {Count} products", Products.Count);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load products: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand(CanExecute = nameof(CanViewDetail))]
    private async Task ViewDetailAsync()
    {
        if (SelectedProduct == null) return;
        
        Logger.LogInformation("[DEMO] Opening product detail for {ProductId}", SelectedProduct.Id);
        
        await _windowManager.ShowDialogAsync<DemoProductDetailViewModel, bool>(
            new DemoProductDetailParams { ProductId = SelectedProduct.Id }
        );
        
        await LoadProductsAsync();
    }
    
    private bool CanViewDetail() => SelectedProduct != null && !IsBusy;
    
    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteProductAsync()
    {
        if (SelectedProduct == null) return;
        
        try
        {
            IsBusy = true;
            await _deleteHandler.HandleAsync(new DeleteDemoProductCommand(SelectedProduct.Id));
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to delete product: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    private bool CanDelete() => SelectedProduct != null && !IsBusy;
    
    partial void OnSelectedProductChanged(DemoProduct? value)
    {
        ViewDetailCommand.NotifyCanExecuteChanged();
        DeleteProductCommand.NotifyCanExecuteChanged();
    }
}
