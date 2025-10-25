using System.Collections.ObjectModel;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Products;
using WpfEngine.Demo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;

namespace WpfEngine.Demo.ViewModels;

// ========== DEMO PRODUCT DETAIL ==========

public partial class DemoProductDetailViewModel : BaseViewModel
{
    private readonly IQueryHandler<GetDemoProductByIdQuery, DemoProduct?> _getProductHandler;
    private readonly IQueryHandler<GetAllDemoCategoriesQuery, List<DemoProductCategory>> _getCategoriesHandler;
    private readonly ICommandHandler<UpdateDemoProductCommand> _updateHandler;
    private readonly IDialogService _dialogService;
    private readonly int _productId;
    
    [ObservableProperty]
    private DemoProduct? _product;
    
    [ObservableProperty]
    private string _name = string.Empty;
    
    [ObservableProperty]
    private string _description = string.Empty;
    
    [ObservableProperty]
    private string _barcode = string.Empty;
    
    [ObservableProperty]
    private decimal _price;
    
    [ObservableProperty]
    private int _stock;
    
    [ObservableProperty]
    private decimal _weight;
    
    [ObservableProperty]
    private string _unit = "pcs";
    
    [ObservableProperty]
    private ObservableCollection<DemoProductCategory> _categories = new();
    
    [ObservableProperty]
    private DemoProductCategory? _selectedCategory;
    
    public DemoProductDetailViewModel(
        IQueryHandler<GetDemoProductByIdQuery, DemoProduct?> getProductHandler,
        IQueryHandler<GetAllDemoCategoriesQuery, List<DemoProductCategory>> getCategoriesHandler,
        ICommandHandler<UpdateDemoProductCommand> updateHandler,
        IDialogService WindowService,
        ILogger<DemoProductDetailViewModel> logger,
        DemoProductDetailParams parameters) : base(logger)
    {
        _getProductHandler = getProductHandler;
        _getCategoriesHandler = getCategoriesHandler;
        _updateHandler = updateHandler;
        _dialogService = WindowService;
        _productId = parameters.ProductId;
    }
    
    public new async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            
            // Load categories
            var categories = await _getCategoriesHandler.HandleAsync(new GetAllDemoCategoriesQuery());
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
            
            // Load product
            var product = await _getProductHandler.HandleAsync(new GetDemoProductByIdQuery(_productId));
            
            if (product != null)
            {
                Product = product;
                Name = product.Name;
                Description = product.Description;
                Barcode = product.Barcode;
                Price = product.Price;
                Stock = product.Stock;
                Weight = product.Weight;
                Unit = product.Unit;
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == product.CategoryId);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load product: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            
            await _updateHandler.HandleAsync(new UpdateDemoProductCommand(
                _productId, Name, Description, Barcode, Price, Stock, Weight, Unit, SelectedCategory?.Id
            ));

            await _getProductHandler.HandleAsync(new GetDemoProductByIdQuery(_productId));
        }
        catch (Exception ex)
        {
            SetError($"Failed to save: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    private bool CanSave() => !string.IsNullOrWhiteSpace(Name) && Price > 0 && !IsBusy;
    
    [RelayCommand]
    private void Cancel()
    {
        return;
    }
    
    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnPriceChanged(decimal value) => SaveCommand.NotifyCanExecuteChanged();
}
