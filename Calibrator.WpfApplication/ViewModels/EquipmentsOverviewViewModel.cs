using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Features.EquipmentsOverview.Commands;
using Calibrator.WpfApplication.Features.EquipmentsOverview.Queries;
using Calibrator.WpfApplication.Models.Entities;
using Calibrator.WpfApplication.Services;
using Calibrator.WpfApplication.Views.Components.UniTable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfApplication.ViewModels;

public partial class EquipmentsOverviewViewModel : BaseViewModel
{
    private readonly GetEquipmentsQuery _equipmentsQuery;
    private readonly DeleteEquipmentCommand _deleteEquipmentCommand;
    private readonly IDialogService _dialogService;
    private readonly IPromptDialogService _promptDialogService;

    [ObservableProperty] private List<UniTableColumn>? _columns;
    [ObservableProperty] private List<Equipment>? _equipments;
    [ObservableProperty] private List<UniTableBaseAction>? _tableOperations;

    public EquipmentsOverviewViewModel(
        GetEquipmentsQuery equipmentsQuery,
        DeleteEquipmentCommand deleteEquipmentCommand,
        IDialogService dialogService,
        IPromptDialogService promptDialogService)
    {
        _equipmentsQuery = equipmentsQuery;
        _deleteEquipmentCommand = deleteEquipmentCommand;
        _dialogService = dialogService;
        _promptDialogService = promptDialogService;

        InitUniTable();
    }

    public override async Task InitializeAsync()
    {
        await ReloadEquipments();
    }

    [RelayCommand]
    private async Task AddNew()
    {
        _dialogService.Open<EditEquipmentDialogViewModel, Guid?>(null);

        await ReloadEquipments();
    }

    private async Task ReloadEquipments()
    {
        Equipments = await _equipmentsQuery.Execute();
    }

    private async Task OnEditClicked(Equipment equipment)
    {
        _dialogService.Open<EditEquipmentDialogViewModel, Guid?>(equipment.Id);

        await ReloadEquipments();
    }

    private async Task OnDeleteClicked(Equipment equipment)
    {
        if (!await _promptDialogService.AskForConfirmation("Are you sure you want to delete Equipment? "))
        {
            return;
        }

        await _deleteEquipmentCommand.Execute(equipment.Id);
        await ReloadEquipments();
    }

    private void InitUniTable()
    {
        Columns = new()
        {
            new UniTableRegularColumn<Equipment>
            {
                ColumnName = "Serial Number",
                PropertySelector = c => c.SerialNumber,
            },
            new UniTableRegularColumn<Equipment>
            {
                ColumnName = "Identifier",
                PropertySelector = c => c.Identifier,
            },
            new UniTableRegularColumn<Equipment>
            {
                ColumnName = "Measurement unit",
                PropertySelector = c => c.MeasurementUnit,
            },
            new UniTableRegularColumn<Equipment>
            {
                ColumnName = "Minimum torque",
                PropertySelector = c => c.MinimumTorque,
            },
            new UniTableRegularColumn<Equipment>
            {
                ColumnName = "Maximum torque",
                PropertySelector = c => c.MaximumTorque,
            },
            new UniTableRegularColumn<Equipment>
            {
                ColumnName = "Minimum angle",
                PropertySelector = c => c.MinimumAngle,
            },
            new UniTableRegularColumn<Equipment>
            {
                ColumnName = "Maximum angle",
                PropertySelector = c => c.MaximumAngle,
            },
        };

        TableOperations = new()
        {
            new UniTableAsyncAction
            {
                ToolTip = "Edit",
                IconKind = PackIconMaterialKind.PencilCircle,
                Command = (item) => OnEditClicked((Equipment)item),
            },
            new UniTableAsyncAction
            {
                ToolTip = "Delete",
                IconKind = PackIconMaterialKind.Delete,
                Command = (item) => OnDeleteClicked((Equipment)item),
            }
        };
    }
}
