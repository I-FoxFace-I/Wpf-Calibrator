using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Features.EquipmentsOverview.Commands;
using Calibrator.WpfApplication.Features.EquipmentsOverview.Queries;
using Calibrator.WpfApplication.Features.EquipmentTemplatesOverview.Queries;
using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Entities;
using Calibrator.WpfApplication.Models.Enums;
using Calibrator.WpfApplication.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calibrator.WpfApplication.ViewModels;

public partial class EditEquipmentDialogViewModel : BaseViewModel, IDialogViewModel<Guid?>
{
    private readonly IDialogService _dialogService;
    private readonly IPromptDialogService _promptDialogService;
    private readonly GetEquipmentQuery _getEquipmentQuery;
    private readonly GetEquipmentTemplatesQuery _getEquipmentTemplatesQuery;
    private readonly UpsertEquipmentCommand _upsertEquipmentCommand;

    private Equipment? _equipment;

    [ObservableProperty] private string? _serialNumber;
    [ObservableProperty] private string? _identifier;
    [ObservableProperty] private UnitType _selectedMeasurementUnit;
    [ObservableProperty] private decimal _minimumTorque;
    [ObservableProperty] private decimal _maximumTorque;
    [ObservableProperty] private decimal _minimumAngle;
    [ObservableProperty] private decimal _maximumAngle;

    [ObservableProperty] private bool _isEquipmentCreationMode;
    [ObservableProperty] private EquipmentTemplate? _selectedEquipmentTemplate;
    [ObservableProperty] private List<EquipmentTemplate> _allEquipmentTemplates;

    public List<EquipmentType> AllEquipmentTypes { get; } = Enum.GetValues<EquipmentType>().ToList();
    public List<UnitType> AllUnits { get; } = Enum.GetValues<UnitType>().ToList();

    /// Parameter is Equipment <see cref="Equipment.Id"/>
    public Guid? Parameter { get; set; }

    public EditEquipmentDialogViewModel(
        IDialogService dialogService,
        IPromptDialogService promptDialogService,
        GetEquipmentQuery getEquipmentQuery,
        GetEquipmentTemplatesQuery getEquipmentTemplatesQuery,
        UpsertEquipmentCommand upsertEquipmentCommand)
    {
        _dialogService = dialogService;
        _promptDialogService = promptDialogService;
        _getEquipmentQuery = getEquipmentQuery;
        _getEquipmentTemplatesQuery = getEquipmentTemplatesQuery;
        _upsertEquipmentCommand = upsertEquipmentCommand;
    }

    public override async Task InitializeAsync()
    {
        _equipment = await TryGetEquipment();

        await GetAllEquipmentTemplates();

        IsEquipmentCreationMode = _equipment is null;
        SerialNumber = _equipment?.SerialNumber;
        Identifier = _equipment?.Identifier;
        SelectedMeasurementUnit = _equipment?.MeasurementUnit ?? UnitType.Nm;
        MinimumTorque = _equipment?.MinimumTorque ?? 0;
        MaximumTorque = _equipment?.MaximumTorque ?? 0;
        MinimumAngle = _equipment?.MinimumAngle ?? 0;
        MaximumAngle = _equipment?.MaximumAngle ?? 0;
    }

    private async Task<Equipment?> TryGetEquipment()
    {
        if (Parameter is null)
            return null;

        var equipment = await _getEquipmentQuery.Execute(Parameter.Value);

        if (equipment is not null)
            return equipment;

        await _promptDialogService.Alert(this, "We weren't able to find Equipment. Something went wrong");

        Close();

        return null;
    }

    private async Task GetAllEquipmentTemplates()
    {
        AllEquipmentTemplates = await _getEquipmentTemplatesQuery.Execute();

        if (_equipment?.EquipmentTemplateId is not null)
        {
            SelectedEquipmentTemplate = AllEquipmentTemplates.First(et => et.Id == _equipment.EquipmentTemplateId);
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (!EnsureFieldsValid())
            return;

        await _upsertEquipmentCommand.Execute(
            new UpsertEquipmentDto(
                Parameter ?? Guid.Empty,
                SelectedEquipmentTemplate!.Id,
                SerialNumber!,
                Identifier!,
                SelectedMeasurementUnit,
                MinimumTorque,
                MaximumTorque,
                MinimumAngle,
                MaximumAngle
            ));

        _dialogService.Close(this);
    }

    [RelayCommand]
    private void Close()
    {
        _dialogService.Close(this);
    }

    // [TODO.VS]: Move to validator
    private bool EnsureFieldsValid()
    {
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

        if (SelectedEquipmentTemplate is null)
        {
            _promptDialogService.Alert("Please, select equipment template");
            return false;
        }

        return true;
    }
}
