using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Features.MeasuringInstrumentsOverview.Commands;
using Calibrator.WpfApplication.Features.MeasuringInstrumentsOverview.Queries;
using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Entities;
using Calibrator.WpfApplication.Models.Enums;
using Calibrator.WpfApplication.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calibrator.WpfApplication.ViewModels;

public partial class EditMeasuringInstrumentDialogViewModel : BaseViewModel, IDialogViewModel<Guid?>
{
    private readonly IDialogService _dialogService;
    private readonly IPromptDialogService _promptDialogService;
    private readonly GetMeasuringInstrumentQuery _getMeasuringInstrumentQuery;
    private readonly UpsertMeasuringInstrumentCommand _upsertMeasuringInstrumentCommand;

    private MeasuringInstrument? _measuringInstrument;

    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _serialNumber;
    [ObservableProperty] private ToolConnectionMethod _selectedConnectionMethod;
    [ObservableProperty] private MeasuringInstrumentType _selectedMeasuringInstrumentType;

    public List<ToolConnectionMethod> AllConnectionMethods { get; } = Enum.GetValues<ToolConnectionMethod>().ToList();

    public List<MeasuringInstrumentType> AllMeasuringInstrumentTypes { get; } =
        Enum.GetValues<MeasuringInstrumentType>().ToList();

    /// Parameter is MeasuringInstrument <see cref="MeasuringInstrument.Id"/>
    public Guid? Parameter { get; set; }

    public EditMeasuringInstrumentDialogViewModel(
        IDialogService dialogService,
        IPromptDialogService promptDialogService,
        GetMeasuringInstrumentQuery getMeasuringInstrumentQuery,
        UpsertMeasuringInstrumentCommand upsertMeasuringInstrumentCommand)
    {
        _dialogService = dialogService;
        _promptDialogService = promptDialogService;
        _getMeasuringInstrumentQuery = getMeasuringInstrumentQuery;
        _upsertMeasuringInstrumentCommand = upsertMeasuringInstrumentCommand;
    }

    public override async Task InitializeAsync()
    {
        _measuringInstrument = await TryGetMeasuringInstrument();

        Name = _measuringInstrument?.Name;
        SerialNumber = _measuringInstrument?.SerialNumber;
        SelectedConnectionMethod = _measuringInstrument?.ConnectionMethod ?? ToolConnectionMethod.Ethernet;
        SelectedMeasuringInstrumentType = _measuringInstrument?.Type ?? MeasuringInstrumentType.STa6000;
    }

    private async Task<MeasuringInstrument?> TryGetMeasuringInstrument()
    {
        if (Parameter is null)
            return null;

        var measuringInstrument = await _getMeasuringInstrumentQuery.Execute(Parameter.Value);

        if (measuringInstrument is not null)
            return measuringInstrument;

        await _promptDialogService.Alert(this, "We weren't able to find measuring instrument. Something went wrong");

        Close();

        return null;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (!EnsureFieldsValid())
            return;

        await _upsertMeasuringInstrumentCommand.Execute(
            new UpsertMeasuringInstrumentDto(
                Parameter ?? Guid.Empty,
                Name!,
                SerialNumber!,
                SelectedConnectionMethod,
                SelectedMeasuringInstrumentType
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
