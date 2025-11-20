using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using WpfEngine.Abstract;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.Repositories;
using WpfEngine.Demo.ViewModels.Parameters.Repository;
using WpfEngine.Demo.ViewModels.Workflow.Repository;
using WpfEngine.Extensions;
using WpfEngine.Services;

namespace WpfEngine.Demo.ViewModels.Product.Repository;

/// <summary>
/// Product Detail ViewModel using Repository pattern with Fluent API
/// </summary>
public partial class ProductDetailViewModel : BaseViewModel, IInitializable
{
    private readonly IScopeManager _scopeManager;
    private readonly IWindowContext _windowContext;
    private readonly int _productId;

    [ObservableProperty] private DemoProduct? _product;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private decimal _price;
    [ObservableProperty] private string _barcode = string.Empty;
    [ObservableProperty] private int _stock;
    [ObservableProperty] private decimal _weight;
    [ObservableProperty] private string _unit = "pcs";
    [ObservableProperty] private ObservableCollection<DemoProductCategory> _categories = new();
    [ObservableProperty] private DemoProductCategory? _selectedCategory;

    public ProductDetailViewModel(
        ProductDetailParameters parameters,
        IScopeManager scopeManager,
        IWindowContext windowContext,
        ILogger<ProductDetailViewModel> logger) : base(logger)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _windowContext = windowContext ?? throw new ArgumentNullException(nameof(windowContext));
        _productId = parameters.ProductId;
        Logger.LogInformation("[DEMO_V2] ProductDetailViewModel created for {ProductId}", _productId);
    }

    public override async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            // Use Fluent API to load product and categories
            (DemoProduct? product, IEnumerable<DemoProductCategory> categories) = await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoProduct>>()
                .WithService<IRepository<DemoProductCategory>>()
                .ExecuteWithResultAsync(async (productRepo, categoryRepo) =>
                {
                    var prod = await productRepo.GetByIdAsync(_productId);
                    var cats = await categoryRepo.GetAllAsync();
                    return (prod, cats);
                });

            if (product == null)
            {
                SetError($"Product {_productId} not found");
                return;
            }

            Product = product;
            Name = product.Name;
            Description = product.Description;
            Price = product.Price;
            Stock = product.Stock;
            Barcode = product.Barcode;
            Weight = product.Weight;
            Unit = product.Unit;

            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }

            if (product.CategoryId.HasValue)
            {
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == product.CategoryId.Value);
            }

            Logger.LogInformation("[DEMO_V2] Loaded product {ProductId}", _productId);
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

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (Product == null) return;
        try
        {
            IsBusy = true;
            ClearError();

            // Use Fluent API to update product
            await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoProduct>>()
                .ExecuteAsync(async (repo) =>
                {
                    var product = await repo.GetByIdAsync(_productId);
                    if (product == null) return;

                    product.Name = Name;
                    product.Description = Description;
                    product.Barcode = Barcode;
                    product.Price = Price;
                    product.Stock = Stock;
                    product.Weight = Weight;
                    product.Unit = Unit;
                    product.CategoryId = SelectedCategory?.Id;

                    await repo.UpdateAsync(product);
                    Logger.LogInformation("[DEMO_V2] Product {ProductId} updated", _productId);
                });

            _windowContext.CloseWindow();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Failed to save product");
            SetError("Failed to save: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Name) && Price >= 0;

    [RelayCommand]
    private void Cancel()
    {
        Logger.LogInformation("[DEMO_V2] Cancelling product edit");
        _windowContext.CloseWindow();
    }

    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnPriceChanged(decimal value) => SaveCommand.NotifyCanExecuteChanged();
}