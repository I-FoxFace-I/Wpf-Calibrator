using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Features.MeasuringInstrumentsOverview.Commands;
using Calibrator.WpfApplication.Features.MeasuringInstrumentsOverview.Queries;
using Calibrator.WpfApplication.Models.Entities;
using Calibrator.WpfApplication.Services;
using Calibrator.WpfApplication.Views.Components.UniTable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfApplication.ViewModels;

public partial class MeasuringInstrumentsOverviewViewModel : BaseViewModel
{
    private readonly GetMeasuringInstrumentsQuery _measuringInstrumentsQuery;
    private readonly DeleteMeasuringInstrumentCommand _deleteMeasuringInstrumentCommand;
    private readonly IDialogService _dialogService;
    private readonly IPromptDialogService _promptDialogService;

    [ObservableProperty] private List<UniTableColumn>? _columns;
    [ObservableProperty] private List<MeasuringInstrument>? _measuringInstruments;
    [ObservableProperty] private List<UniTableBaseAction>? _tableOperations;

    public MeasuringInstrumentsOverviewViewModel(
        IDialogService dialogService,
        IPromptDialogService promptDialogService, 
        GetMeasuringInstrumentsQuery measuringInstrumentsQuery,
        DeleteMeasuringInstrumentCommand deleteMeasuringInstrumentCommand)
    {
        _dialogService = dialogService;
        _promptDialogService = promptDialogService;
        _measuringInstrumentsQuery = measuringInstrumentsQuery;
        _deleteMeasuringInstrumentCommand = deleteMeasuringInstrumentCommand;

        InitUniTable();
    }

    public override async Task InitializeAsync()
    {
        await ReloadMeasuringInstruments();
    }

    [RelayCommand]
    private async Task AddNew()
    {
        _dialogService.Open<EditMeasuringInstrumentDialogViewModel, Guid?>(null);

        await ReloadMeasuringInstruments();
    }

    private async Task ReloadMeasuringInstruments()
    {
        MeasuringInstruments = await _measuringInstrumentsQuery.Execute();
    }

    private async Task OnEditClicked(MeasuringInstrument measuringInstrument)
    {
        _dialogService.Open<EditMeasuringInstrumentDialogViewModel, Guid?>(measuringInstrument.Id);

        await ReloadMeasuringInstruments();
    }

    private async Task OnDeleteClicked(MeasuringInstrument measuringInstrument)
    {
        if (!await _promptDialogService.AskForConfirmation("Are you sure you want to delete measuring instrument? "))
        {
            return;
        }

        await _deleteMeasuringInstrumentCommand.Execute(measuringInstrument.Id);
        await ReloadMeasuringInstruments();
    }

    private void InitUniTable()
    {
        Columns = new()
        {
            new UniTableRegularColumn<MeasuringInstrument>
            {
                ColumnName = "Name",
                PropertySelector = c => c.Name,
            },
            new UniTableRegularColumn<MeasuringInstrument>
            {
                ColumnName = "Serial Number",
                PropertySelector = c => c.SerialNumber,
            },
            new UniTableRegularColumn<MeasuringInstrument>
            {
                ColumnName = "Type",
                PropertySelector = c => c.Type,
            },
            new UniTableRegularColumn<MeasuringInstrument>
            {
                ColumnName = "Connection Method",
                PropertySelector = c => c.ConnectionMethod,
            },
        };

        TableOperations = new()
        {
            new UniTableAsyncAction
            {
                ToolTip = "Edit",
                IconKind = PackIconMaterialKind.PencilCircle,
                Command = (item) => OnEditClicked((MeasuringInstrument)item),
            },
            new UniTableAsyncAction
            {
                ToolTip = "Delete",
                IconKind = PackIconMaterialKind.Delete,
                Command = (item) => OnDeleteClicked((MeasuringInstrument)item),
            }
        };
    }
}
