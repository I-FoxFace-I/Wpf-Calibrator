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
using WpfEngine.Demo.ViewModels.Parameters.Repository;
using WpfEngine.Extensions;
using WpfEngine.Services;
using WpfEngine.ViewModels.Dialogs;

namespace WpfEngine.Demo.ViewModels.Product.Repository;

/// <summary>
/// Product Create ViewModel using Repository pattern with Fluent API
/// </summary>
public partial class ProductCreateViewModel : ResultDialogViewModel<ProductCreateParameters, DemoProduct>
{
    private readonly IScopeManager _scopeManager;
    private readonly IWindowContext _windowContext;
    private readonly int _productId;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private decimal _price;
    [ObservableProperty] private decimal _weight;
    [ObservableProperty] private string _unit = "pcs";
    [ObservableProperty] private ObservableCollection<DemoProductCategory> _categories = new();
    [ObservableProperty] private DemoProductCategory? _selectedCategory;

    public ProductCreateViewModel(
        ProductCreateParameters parameters,
        IDialogHost dialogHost,
        IScopeManager scopeManager,
        IWindowContext windowContext,
        ILogger<ProductCreateViewModel> logger) : base(logger, dialogHost, parameters)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _windowContext = windowContext ?? throw new ArgumentNullException(nameof(windowContext));
        _productId = parameters.ProductId;

        Name = parameters.Name;
        Unit = parameters.Unit;
        Weight = parameters.Weight;
        Description = parameters.Description;

        Logger.LogInformation("[DEMO_V2] ProductCreateViewModel created for {ProductId}", _productId);
    }

    public override async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;

            // Use Fluent API to load categories
            var categories = await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoProductCategory>>()
                .ExecuteWithResultAsync(async (repo) =>
                {
                    return await repo.GetAllAsync();
                });

            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }

            Logger.LogInformation("[DEMO_V2] Loaded {Count} categories", Categories.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Error loading categories");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static Random random = new Random();

    private string GetBarCode(int length = 3)
    {
        var chars = string.Concat(Name.Where(char.IsLetter));
        if (chars.Length == 0) chars = "XXX";
        var prefix = new string(Enumerable.Repeat(chars.ToLower(), length).Select(s => s[random.Next(s.Length)]).ToArray());
        return $"{prefix}-{DateTime.Now:MM-yyyy}";
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Name) &&
               Price > 0 &&
               Weight > 0 &&
               SelectedCategory != null;
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;

            // Use Fluent API to create product
            await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoProduct>>()
                .ExecuteAsync(async (repo) =>
                {
                    var product = new DemoProduct
                    {
                        Name = Name,
                        Description = Description,
                        Price = Price,
                        Weight = Weight,
                        Stock = 0,
                        Unit = Unit,
                        Barcode = GetBarCode(),
                        CategoryId = SelectedCategory!.Id
                    };

                    await repo.AddAsync(product);
                    Logger.LogInformation("[DEMO_V2] Created product {ProductId}", product.Id);
                });

            _windowContext.CloseWindow();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Error creating product");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private void Cancel()
    {
        Logger.LogInformation("[DEMO_V2] Cancelling product creation");
        _windowContext.CloseWindow();
    }

    partial void OnDescriptionChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnPriceChanged(decimal value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnUnitChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnWeightChanged(decimal value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnSelectedCategoryChanged(DemoProductCategory? value) => SaveCommand.NotifyCanExecuteChanged();

    protected override async Task CompleteDialogAsync()
    {
        base.OnComplete();
        await Task.CompletedTask;
        base.CloseDialogWindow(DialogResult<DemoProduct>.Success(CreateResult()));
    }

    protected override async Task CancelDialogAsync()
    {
        base.OnComplete();
        await Task.CompletedTask;
        base.CloseDialogWindow(DialogResult<DemoProduct>.Cancel());
    }

    protected override DemoProduct? CreateResult()
    {
        return new DemoProduct
        {
            Name = Name,
            Description = Description,
            Price = Price,
            Weight = Weight,
            Stock = 0,
            Unit = Unit,
            Barcode = GetBarCode(),
            CategoryId = SelectedCategory!.Id
        };
    }
}