using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Features.TransducersOverview.Commands;
using Calibrator.WpfApplication.Features.TransducersOverview.Queries;
using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Entities;
using Calibrator.WpfApplication.Models.Enums;
using Calibrator.WpfApplication.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calibrator.WpfApplication.ViewModels;

public partial class EditTransducerDialogViewModel: BaseViewModel, IDialogViewModel<Guid?>
{
    private readonly IDialogService _dialogService;
    private readonly IPromptDialogService _promptDialogService;
    private readonly GetTransducerQuery _getTransducerQuery;
    private readonly UpsertTransducerCommand _upsertTransducerCommand;

    private Transducer? _transducer;

    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _serialNumber;
    [ObservableProperty] private decimal _minimumCapacity;
    [ObservableProperty] private decimal _maximumCapacity;
    [ObservableProperty] private TransducersType _selectedTransducerType;
    [ObservableProperty] private UnitType _selectedCapacityUnit;
    [ObservableProperty] private UnitType _selectedMeasurementUnit;

    public List<TransducersType> AllTransducerTypes { get; } =
        Enum.GetValues<TransducersType>().ToList();
    
    public List<UnitType> AllUnits { get; } =
        Enum.GetValues<UnitType>().ToList();

    /// Parameter is Transducer <see cref="Transducer.Id"/>
    public Guid? Parameter { get; set; }

    public EditTransducerDialogViewModel(
        IDialogService dialogService,
        IPromptDialogService promptDialogService,
        GetTransducerQuery getTransducerQuery,
        UpsertTransducerCommand upsertTransducerCommand)
    {
        _dialogService = dialogService;
        _promptDialogService = promptDialogService;
        _getTransducerQuery = getTransducerQuery;
        _upsertTransducerCommand = upsertTransducerCommand;
    }

    public override async Task InitializeAsync()
    {
        _transducer = await TryGetTransducer();

        Name = _transducer?.Name;
        SerialNumber = _transducer?.SerialNumber;
        MinimumCapacity = _transducer?.MinimumCapacity ?? 0;
        MaximumCapacity = _transducer?.MaximumCapacity ?? 0;
        SelectedTransducerType = _transducer?.Type ?? TransducersType.Brake;
        SelectedCapacityUnit = _transducer?.CapacityUnit ?? UnitType.Nm;
        SelectedMeasurementUnit = _transducer?.MeasurementUnit ?? UnitType.Nm;
    }

    private async Task<Transducer?> TryGetTransducer()
    {
        if (Parameter is null)
            return null;

        var transducer = await _getTransducerQuery.Execute(Parameter.Value);

        if (transducer is not null)
            return transducer;

        await _promptDialogService.Alert(this, "We weren't able to find transducer. Something went wrong");

        Close();

        return null;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (!EnsureFieldsValid())
            return;

        await _upsertTransducerCommand.Execute(
            new UpsertTransducerDto(
                Parameter ?? Guid.Empty,
                Name!,
                SerialNumber!,
                MinimumCapacity,
                MaximumCapacity,
                SelectedTransducerType,
                SelectedCapacityUnit,
                SelectedMeasurementUnit
            ));

        Close();
        //_dialogService.Close(this);
    }

    [RelayCommand]
    private void Close()
    {
        _dialogService.Close(this);
    }

    // [TODO.VS]: Move to validator
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

        return true;
    }
}
