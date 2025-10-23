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

public partial class EditEquipmentTemplateDialogViewModel : BaseViewModel, IDialogViewModel<Guid?>
{
    private readonly IDialogService _dialogService;
    private readonly IPromptDialogService _promptDialogService;
    private readonly IEquipmentTemplateRepository _equipmentTemplateRepository;

    private EquipmentTemplate? _equipmentTemplate;

    [ObservableProperty] private string? _name;
    [ObservableProperty] private UnitType _selectedMeasurementUnit;
    [ObservableProperty] private EquipmentType _selectedEquipmentType;
    [ObservableProperty] private decimal _minimumTorque;
    [ObservableProperty] private decimal _maximumTorque;

    public List<EquipmentType> AllEquipmentTypes { get; } = Enum.GetValues<EquipmentType>().ToList();
    public List<UnitType> AllUnits { get; } = Enum.GetValues<UnitType>().ToList();

    public Guid? Parameter { get; set; }

    public EditEquipmentTemplateDialogViewModel(
        IDialogService dialogService,
        IPromptDialogService promptDialogService,
        IEquipmentTemplateRepository equipmentTemplateRepository)
    {
        _dialogService = dialogService;
        _promptDialogService = promptDialogService;
        _equipmentTemplateRepository = equipmentTemplateRepository;
    }

    public override async Task InitializeAsync()
    {
        await ExecuteWithLoading(async () =>
        {
            _equipmentTemplate = await TryGetEquipmentTemplate();

            Name = _equipmentTemplate?.Name;
            SelectedMeasurementUnit = _equipmentTemplate?.MeasurementUnit ?? UnitType.Nm;
            SelectedEquipmentType = _equipmentTemplate?.Type ?? EquipmentType.RatchetWrench;
            MinimumTorque = _equipmentTemplate?.MinimumTorque ?? 0;
            MaximumTorque = _equipmentTemplate?.MaximumTorque ?? 0;
        });
    }

    private async Task<EquipmentTemplate?> TryGetEquipmentTemplate()
    {
        if (Parameter is null)
            return null;

        return await _equipmentTemplateRepository.TryGet(Parameter.Value);
    }

    [RelayCommand]
    private async Task Save()
    {
        await ExecuteWithLoading(async () =>
        {
            if (!EnsureFieldsValid()) return;

            var dto = new UpsertEquipmentTemplateDto(
                Parameter ?? Guid.Empty,
                Name!,
                SelectedEquipmentType,
                SelectedMeasurementUnit,
                MinimumTorque,
                MaximumTorque
            );

            var equipmentTemplate = await _equipmentTemplateRepository.TryGet(dto.Id) ?? EquipmentTemplate.CreateNew();
            equipmentTemplate.Upsert(dto);
            await _equipmentTemplateRepository.Upsert(equipmentTemplate);

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

        return true;
    }
}

// Additional ViewModels with similar structure (abbreviated for brevity)

//public partial class EquipmentsOverviewViewModel : BaseViewModel
//{
//    // Similar implementation to EquipmentTemplatesOverviewViewModel
//    public EquipmentsOverviewViewModel(IEquipmentRepository equipmentRepository, IDialogService dialogService, IPromptDialogService promptDialogService) { }
//    //public override Task InitializeAsync() => Task.CompletedTask;
//}

//public partial class EditEquipmentDialogViewModel : BaseViewModel, IDialogViewModel<Guid?>
//{
//    public Guid? Parameter { get; set; }
//    public EditEquipmentDialogViewModel(IDialogService dialogService, IPromptDialogService promptDialogService, IEquipmentRepository equipmentRepository) { }
//    //public override Task InitializeAsync() => Task.CompletedTask;
//}

//public partial class MeasuringInstrumentsOverviewViewModel : BaseViewModel
//{
//    public MeasuringInstrumentsOverviewViewModel(IMeasuringInstrumentRepository measuringInstrumentRepository, IDialogService dialogService, IPromptDialogService promptDialogService) { }
//    //public override Task InitializeAsync() => Task.CompletedTask;
//}

//public partial class EditMeasuringInstrumentDialogViewModel : BaseViewModel, IDialogViewModel<Guid?>
//{
//    public Guid? Parameter { get; set; }
//    public EditMeasuringInstrumentDialogViewModel(IDialogService dialogService, IPromptDialogService promptDialogService, IMeasuringInstrumentRepository measuringInstrumentRepository) { }
//    //public override Task InitializeAsync() => Task.CompletedTask;
//}

//public partial class TransducersOverviewViewModel : BaseViewModel
//{
//    public TransducersOverviewViewModel(ITransducerRepository transducerRepository, IDialogService dialogService, IPromptDialogService promptDialogService) { }
//    //public override Task InitializeAsync() => Task.CompletedTask;
//}

//public partial class EditTransducerDialogViewModel : BaseViewModel, IDialogViewModel<Guid?>
//{
//    //public Guid? Parameter { get; set; }
//    public EditTransducerDialogViewModel(IDialogService dialogService, IPromptDialogService promptDialogService, ITransducerRepository transducerRepository) { }
//    //public override Task InitializeAsync() => Task.CompletedTask;
//}

