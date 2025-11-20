using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using WpfEngine.Abstract;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Products;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.ViewModels.Parameters;
using WpfEngine.Services;
using WpfEngine.ViewModels.Dialogs;

namespace WpfEngine.Demo.ViewModels;

public partial class ProductCreateViewModel : ResultDialogViewModel<ProductCreateParameters, DemoProduct>, IInitializable
{
    private readonly IWindowContext _windowContext;
    private readonly IQueryHandler<GetAllDemoCategoriesQuery, List<DemoProductCategory>> _getAllDemoCategoriesHandler;
    private readonly int _productId;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private decimal _price;
    [ObservableProperty] private decimal _weight;
    [ObservableProperty] private string _unit = "pcs";
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private ObservableCollection<DemoProductCategory> _categories = new();
    [ObservableProperty] private DemoProductCategory? _selectedCategory;

    public ProductCreateViewModel(
        ProductCreateParameters parameters,
        IQueryHandler<GetAllDemoCategoriesQuery, List<DemoProductCategory>> getAllDemoCategoriesHandler,
        IWindowContext windowContext,
        IDialogHost dialogHost,
        ILogger<ProductCreateViewModel> logger) : base(logger, dialogHost, parameters)
    {
        _windowContext = windowContext;
        _productId = parameters.ProductId;
        _getAllDemoCategoriesHandler = getAllDemoCategoriesHandler;

        Name = parameters.Name;
        Unit = parameters.Unit;
        Weight = parameters.Weight;
        Description = parameters.Description;
        Logger.LogInformation("[DEMO] ProductDetailViewModel created for {ProductId}", _productId);
    }

    public override async Task InitializeAsync() => await InitializeAsync(CancellationToken.None);
    public async Task InitializeAsync(CancellationToken cancelationToken = default)
    {
        var result = await _getAllDemoCategoriesHandler.HandleAsync(new GetAllDemoCategoriesQuery());

        Categories.Clear();
        foreach(var item in result)
        {
            Categories.Add(item);
        }
    }


    partial void OnDescriptionChanged(string value) => CompleteDialogCommand.NotifyCanExecuteChanged();
    partial void OnNameChanged(string value) => CompleteDialogCommand.NotifyCanExecuteChanged();
    partial void OnPriceChanged(decimal value) => CompleteDialogCommand.NotifyCanExecuteChanged();
    partial void OnUnitChanged(string value) => CompleteDialogCommand.NotifyCanExecuteChanged();
    partial void OnWeightChanged(decimal value) => CompleteDialogCommand.NotifyCanExecuteChanged();
    partial void OnSelectedCategoryChanged(DemoProductCategory? value) => CompleteDialogCommand.NotifyCanExecuteChanged();


    private static Random random = new Random();

    private string GetBarCode(int lenght=3)
    {
        var chars = string.Concat(Name.Where(char.IsLetter));
        var prefix = new string(Enumerable.Repeat(chars.ToLower(), lenght).Select(s => s[random.Next(s.Length-1)]).ToArray());

        return $"{prefix}-{DateTime.Now.ToString("MM-YYYY")}";
    }

    protected override bool CanCompleteDialog()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return false;
        if(Price <= 0)
            return false;
        if (SelectedCategory is null)
            return false;
        if (Weight <= 0)
            return false;

        return true;
    }

    protected override DemoProduct? CreateResult()
    {
        return new DemoProduct
        {
            Id = _productId,
            Name = Name,
            Description = Description,
            Price = Price,
            Weight = Weight,
            Stock = 0,
            Barcode = GetBarCode(),
            Category = SelectedCategory,
            CategoryId = SelectedCategory!.Id,
        };
    }

    /// <summary>
    /// Clears error message
    /// </summary>
    protected void ClearError()
    {
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Sets error message
    /// </summary>
    protected void SetError(string message)
    {
        ErrorMessage = message;
        Logger.LogError("ViewModel error: {Message}", message);
    }

    protected override async Task CompleteDialogAsync()
    {
        await Task.CompletedTask;
        OnComplete();
        CloseDialogWindow(ResultData);
    }

    protected override async Task CancelDialogAsync()
    {
        await Task.CompletedTask;
        
        OnCancel();
        CloseDialogWindow(null);
    }
}
