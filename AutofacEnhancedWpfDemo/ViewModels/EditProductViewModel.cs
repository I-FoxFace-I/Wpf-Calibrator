using System;
using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Products;
using AutofacEnhancedWpfDemo.Models;
using AutofacEnhancedWpfDemo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels;

/// <summary>
/// ViewModel for Product edit/create dialog
/// </summary>
public partial class EditProductViewModel : BaseViewModel
{
    private readonly IQueryHandler<GetProductByIdQuery, Product?> _getProductHandler;
    private readonly ICommandHandler<CreateProductCommand> _createHandler;
    private readonly ICommandHandler<UpdateProductCommand> _updateHandler;
    private readonly IWindowNavigator _navigator;
    private readonly int? _productId;
    
    [ObservableProperty]
    private string _name = string.Empty;
    
    [ObservableProperty]
    private decimal _price;
    
    [ObservableProperty]
    private int _stock;
    
    [ObservableProperty]
    private string _title = "Add Product";
    
    public bool IsEditMode => _productId.HasValue;
    
    public EditProductViewModel(
        IQueryHandler<GetProductByIdQuery, Product?> getProductHandler,
        ICommandHandler<CreateProductCommand> createHandler,
        ICommandHandler<UpdateProductCommand> updateHandler,
        IWindowNavigator navigator,
        ILogger<EditProductViewModel> logger,
        EditProductParams? parameters = null) : base(logger)
    {
        _getProductHandler = getProductHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _navigator = navigator;
        _productId = parameters?.ProductId;
        
        if (IsEditMode)
        {
            Title = "Edit Product";
        }
    }
    
    public async Task InitializeAsync()
    {
        if (!IsEditMode) return;
        
        try
        {
            IsBusy = true;
            Logger.LogInformation("Loading product {ProductId}", _productId);
            
            var product = await _getProductHandler.HandleAsync(new GetProductByIdQuery(_productId!.Value));
            
            if (product != null)
            {
                Name = product.Name;
                Price = product.Price;
                Stock = product.Stock;
            }
            else
            {
                SetError("Product not found");
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
            ClearError();
            
            if (IsEditMode)
            {
                Logger.LogInformation("Updating product {ProductId}", _productId);
                await _updateHandler.HandleAsync(new UpdateProductCommand(
                    _productId!.Value, Name, Price, Stock
                ));
            }
            else
            {
                Logger.LogInformation("Creating new product");
                await _createHandler.HandleAsync(new CreateProductCommand(
                    Name, Price, Stock
                ));
            }
            
            _navigator.CloseDialog<EditProductViewModel>(new EditProductResult 
            { 
                Success = true, 
                ProductId = _productId 
            });
        }
        catch (Exception ex)
        {
            SetError($"Failed to save product: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    private bool CanSave() => !string.IsNullOrWhiteSpace(Name) && Price > 0 && Stock >= 0 && !IsBusy;
    
    [RelayCommand]
    private void Cancel()
    {
        Logger.LogInformation("Edit cancelled");
        _navigator.CloseDialog<EditProductViewModel>(new EditProductResult { Success = false });
    }
    
    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnPriceChanged(decimal value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnStockChanged(int value) => SaveCommand.NotifyCanExecuteChanged();
}
