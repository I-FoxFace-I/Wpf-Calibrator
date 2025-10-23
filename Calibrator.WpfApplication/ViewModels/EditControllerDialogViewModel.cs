using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Entities;
using Calibrator.WpfApplication.Models.Enums;
using Calibrator.WpfApplication.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calibrator.WpfApplication.ViewModels;

public partial class EditControllerDialogViewModel : BaseViewModel, IDialogViewModel<Guid?>
{
    private readonly IDialogService _dialogService;
    private readonly IPromptDialogService _promptDialogService;
    private readonly IControllerRepository _controllerRepository;

    private Controller? _controller;

    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _serialNumber;
    [ObservableProperty] private string? _identifier;
    [ObservableProperty] private ToolConnectionMethod _selectedConnectionMethod;
    [ObservableProperty] private ControllerType _selectedControllerType;

    public List<ToolConnectionMethod> AllConnectionMethods { get; } = Enum.GetValues<ToolConnectionMethod>().ToList();
    public List<ControllerType> AllControllerTypes { get; } = Enum.GetValues<ControllerType>().ToList();

    /// Parameter is Controller <see cref="Controller.Id"/>
    public Guid? Parameter { get; set; }

    public EditControllerDialogViewModel(
        IDialogService dialogService,
        IPromptDialogService promptDialogService,
        IControllerRepository controllerRepository)
    {
        _dialogService = dialogService;
        _promptDialogService = promptDialogService;
        _controllerRepository = controllerRepository;
    }

    public override async Task InitializeAsync()
    {
        await ExecuteWithLoading(async () =>
        {
            _controller = await TryGetController();

            Name = _controller?.Name;
            SerialNumber = _controller?.SerialNumber;
            Identifier = _controller?.Identifier;
            SelectedConnectionMethod = _controller?.ConnectionMethod ?? ToolConnectionMethod.Ethernet;
            SelectedControllerType = _controller?.Type ?? ControllerType.PowerFocus6000;
        });
    }

    private async Task<Controller?> TryGetController()
    {
        if (Parameter is null)
            return null;

        var controller = await _controllerRepository.TryGet(Parameter.Value);

        if (controller is not null)
            return controller;

        await _promptDialogService.Alert(this, "We weren't able to find controller. Something went wrong");
        Close();
        return null;
    }

    [RelayCommand]
    private async Task Save()
    {
        await ExecuteWithLoading(async () =>
        {
            if (!EnsureFieldsValid())
                return;

            var dto = new UpsertControllerDto(
                Parameter ?? Guid.Empty,
                Name!,
                SerialNumber!,
                Identifier!,
                SelectedConnectionMethod,
                SelectedControllerType
            );

            var controller = await _controllerRepository.TryGet(dto.Id) ?? Controller.CreateNew();
            controller.Upsert(dto);
            await _controllerRepository.Upsert(controller);

            _dialogService.Close(this);
        });
    }

    [RelayCommand]
    private void Close()
    {
        _dialogService.Close(this);
    }

    private bool EnsureFieldsValid()
    {
        if (string.IsNullOrEmpty(Name))
        {
            _promptDialogService.Alert("Name is mandatory");
            return false;
        }

        if (string.IsNullOrEmpty(SerialNumber))
        {
            _promptDialogService.Alert("Serial Number is mandatory");
            return false;
        }

        if (string.IsNullOrEmpty(Identifier))
        {
            _promptDialogService.Alert("Identifier is mandatory");
            return false;
        }

        return true;
    }
}


