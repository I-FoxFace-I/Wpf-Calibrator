using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Models.Entities;
using Calibrator.WpfApplication.Services;
using Calibrator.WpfApplication.Views.Components.UniTable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfApplication.ViewModels;

public partial class EquipmentTemplatesOverviewViewModel : BaseViewModel
{
    private readonly IEquipmentTemplateRepository _equipmentTemplateRepository;
    private readonly IDialogService _dialogService;
    private readonly IPromptDialogService _promptDialogService;

    [ObservableProperty] private List<UniTableColumn>? _columns;
    [ObservableProperty] private List<EquipmentTemplate>? _equipmentTemplates;
    [ObservableProperty] private List<UniTableBaseAction>? _tableOperations;

    public EquipmentTemplatesOverviewViewModel(
        IEquipmentTemplateRepository equipmentTemplateRepository,
        IDialogService dialogService,
        IPromptDialogService promptDialogService)
    {
        _equipmentTemplateRepository = equipmentTemplateRepository;
        _dialogService = dialogService;
        _promptDialogService = promptDialogService;

        InitUniTable();
    }

    public override async Task InitializeAsync()
    {
        await ExecuteWithLoading(ReloadEquipmentTemplates);
    }

    [RelayCommand]
    private async Task AddNew()
    {
        await ExecuteWithLoading(async () =>
        {
            _dialogService.Open<EditEquipmentTemplateDialogViewModel, Guid?>(null);
            await ReloadEquipmentTemplates();
        });
    }

    private async Task ReloadEquipmentTemplates()
    {
        EquipmentTemplates = await _equipmentTemplateRepository.GetAllWithNoTracking();
    }

    private async Task OnEditClicked(EquipmentTemplate equipmentTemplate)
    {
        await ExecuteWithLoading(async () =>
        {
            _dialogService.Open<EditEquipmentTemplateDialogViewModel, Guid?>(equipmentTemplate.Id);
            await ReloadEquipmentTemplates();
        });
    }

    private async Task OnDeleteClicked(EquipmentTemplate equipmentTemplate)
    {
        await ExecuteWithLoading(async () =>
        {
            if (!await _promptDialogService.AskForConfirmation("Are you sure you want to delete this equipment template?"))
            {
                return;
            }

            await _equipmentTemplateRepository.Delete(equipmentTemplate.Id);
            await ReloadEquipmentTemplates();
        });
    }

    private void InitUniTable()
    {
        Columns = new()
        {
            new UniTableRegularColumn<EquipmentTemplate>
            {
                ColumnName = "Name",
                PropertySelector = c => c.Name,
            },
            new UniTableRegularColumn<EquipmentTemplate>
            {
                ColumnName = "Type",
                PropertySelector = c => c.Type,
            },
            new UniTableRegularColumn<EquipmentTemplate>
            {
                ColumnName = "Measurement unit",
                PropertySelector = c => c.MeasurementUnit,
            },
            new UniTableRegularColumn<EquipmentTemplate>
            {
                ColumnName = "Minimum torque",
                PropertySelector = c => c.MinimumTorque,
            },
            new UniTableRegularColumn<EquipmentTemplate>
            {
                ColumnName = "Maximum torque",
                PropertySelector = c => c.MaximumTorque,
            },
        };

        TableOperations = new()
        {
            new UniTableAsyncAction
            {
                ToolTip = "Edit",
                IconKind = PackIconMaterialKind.PencilCircle,
                Command = (item) => OnEditClicked((EquipmentTemplate)item),
            },
            new UniTableAsyncAction
            {
                ToolTip = "Delete",
                IconKind = PackIconMaterialKind.Delete,
                Command = (item) => OnDeleteClicked((EquipmentTemplate)item),
            }
        };
    }
}

