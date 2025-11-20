using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Data.Dialogs;
using WpfEngine.Data.Parameters;
using WpfEngine.Demo.Models;
using WpfEngine.Services;
using WpfEngine.ViewModels.Base;
using WpfEngine.ViewModels.Dialogs;

namespace WpfEngine.Demo.ViewModels.Dialogs;

/// <summary>
/// Parameters for CreateAddressDialog
/// </summary>
public record CreateAddressDialogParams : BaseModelParameters
{
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = "";
}

/// <summary>
/// Result from CreateAddressDialog
/// </summary>
public record CreateAddressDialogResult : BaseResult
{
    private readonly Guid _resultKey = Guid.NewGuid();
    public string Street { get; init; } = "";
    public string City { get; init; } = "";
    public string PostalCode { get; init; } = "";
    public string Country { get; init; } = "";
    public AddressType AddressType { get; init; } = AddressType.Both;
    public override Guid Key => _resultKey;
}

/// <summary>
/// Dialog ViewModel for creating a new address
/// Demonstrates DialogService usage with parameters and result
/// </summary>
public partial class CreateAddressDialogViewModel : ResultDialogViewModel<CreateAddressDialogParams, CreateAddressDialogResult>
{
    [ObservableProperty]
    private string _customerName = "";

    [ObservableProperty]
    private string _street = "";

    [ObservableProperty]
    private string _city = "";

    [ObservableProperty]
    private string _postalCode = "";

    [ObservableProperty]
    private string _country = "";

    [ObservableProperty]
    private bool _hasErrors;

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private AddressType _addressType = AddressType.Both;

    private int _customerId;

    public CreateAddressDialogViewModel(ILogger<CreateAddressDialogViewModel> logger, IDialogHost host, CreateAddressDialogParams parameters) 
        : base(logger, host, parameters)
    {
        Logger.LogInformation("[CREATE_ADDRESS_DIALOG] Opened for customer {CustomerId}", _customerId);
    }
    
    public override async Task InitializeAsync() => await InitializeAsync(CancellationToken.None);
    public Task InitializeAsync(CancellationToken cancelationToken = default)
    {
        Logger.LogInformation("[CREATE_ADDRESS_DIALOG] Opened for customer {CustomerId}", _customerId);
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates and creates result
    /// </summary>
    protected override CreateAddressDialogResult? CreateResult()
    {
        // Validate
        if (string.IsNullOrWhiteSpace(Street))
        {
            HasErrors = true;
            ErrorMessage = "Street is required";
            return null;
        }

        if (string.IsNullOrWhiteSpace(City))
        {
            HasErrors = true;
            ErrorMessage = "City is required";
            return null;
        }

        if (string.IsNullOrWhiteSpace(PostalCode))
        {
            HasErrors = true;
            ErrorMessage = "Postal code is required";
            return null;
        }

        if (string.IsNullOrWhiteSpace(Country))
        {
            HasErrors = true;
            ErrorMessage = "Country is required";
            return null;
        }

        HasErrors = false;
        ErrorMessage = "";

        Logger.LogInformation("[CREATE_ADDRESS_DIALOG] Creating result: {Street}, {City}", Street, City);

        return new CreateAddressDialogResult
        {
            Street = Street,
            City = City,
            PostalCode = PostalCode,
            Country = Country,
            AddressType = AddressType,
        };
    }

    ///// <summary>
    ///// Override CloseDialog to validate before closing
    ///// </summary>
    //protected override void CloseDialogInternal()
    //{
    //    base.CloseDialogInternal();
    //    var result = GetResult();
        
    //    if (result == null)
    //    {
    //        // Validation failed - don't close
    //        Logger.LogWarning("[CREATE_ADDRESS_DIALOG] Validation failed, dialog stays open");
    //        return;
    //    }
        
    //    //CloseDialog(result);

    //    //CloseDialog(DialogResult<CreateAddressDialogResult>.Success(result));
    //}

    partial void OnStreetChanged(string value)
    {
        if (HasErrors)
        {
            HasErrors = false;
            ErrorMessage = "";
        }
    }

    partial void OnCityChanged(string value)
    {
        if (HasErrors)
        {
            HasErrors = false;
            ErrorMessage = "";
        }
    }

    partial void OnPostalCodeChanged(string value)
    {
        if (HasErrors)
        {
            HasErrors = false;
            ErrorMessage = "";
        }
    }

    partial void OnCountryChanged(string value)
    {
        if (HasErrors)
        {
            HasErrors = false;
            ErrorMessage = "";
        }
    }

    protected override async Task CompleteDialogAsync()
    {
        await Task.CompletedTask;
        
        OnComplete();

        if (ResultData == null)
        {
            // Validation failed - don't close
            Logger.LogWarning("[CREATE_ADDRESS_DIALOG] Validation failed, dialog stays open");
            
            return;
        }

        CloseDialogWindow(ResultData);
    }

    protected override async Task CancelDialogAsync()
    {
        await Task.CompletedTask;

        OnComplete();

        CloseDialogWindow(ResultData);
    }
}
