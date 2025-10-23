using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Calibrator.WpfApplication.Features.TransducersOverview.Commands;
using Calibrator.WpfApplication.Features.TransducersOverview.Queries;
using Calibrator.WpfApplication.Models.Entities;
using Calibrator.WpfApplication.Services;
using Calibrator.WpfApplication.Views.Components.UniTable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfApplication.ViewModels;

public partial class TransducersOverviewViewModel : BaseViewModel
{
    private readonly GetTransducersQuery _transducersQuery;
    private readonly DeleteTransducerCommand _deleteTransducerCommand;
    private readonly IDialogService _dialogService;
    private readonly IPromptDialogService _promptDialogService;

    [ObservableProperty] private List<UniTableColumn>? _columns;
    [ObservableProperty] private List<Transducer>? _transducers;
    [ObservableProperty] private List<UniTableBaseAction>? _tableOperations;

    public TransducersOverviewViewModel(
        IDialogService dialogService,
        IPromptDialogService promptDialogService, 
        GetTransducersQuery transducersQuery,
        DeleteTransducerCommand deleteTransducerCommand)
    {
        _dialogService = dialogService;
        _promptDialogService = promptDialogService;
        _transducersQuery = transducersQuery;
        _deleteTransducerCommand = deleteTransducerCommand;

        InitUniTable();
    }

    public override async Task InitializeAsync()
    {
        await ReloadTransducers();
    }

    [RelayCommand]
    private async Task AddNew()
    {
        _dialogService.Open<EditTransducerDialogViewModel, Guid?>(null);

        await ReloadTransducers();
    }

    private async Task ReloadTransducers()
    {
        Transducers = await _transducersQuery.Execute();
    }

    private async Task OnEditClicked(Transducer transducer)
    {
        _dialogService.Open<EditTransducerDialogViewModel, Guid?>(transducer.Id);

        await ReloadTransducers();
    }

    private async Task OnDeleteClicked(Transducer transducer)
    {
        if (!await _promptDialogService.AskForConfirmation("Are you sure you want to delete transducer? "))
        {
            return;
        }

        await _deleteTransducerCommand.Execute(transducer.Id);
        await ReloadTransducers();
    }

    private void InitUniTable()
    {
        Columns = new()
        {
            new UniTableRegularColumn<Transducer>
            {
                ColumnName = "Name",
                PropertySelector = c => c.Name,
            },
            new UniTableRegularColumn<Transducer>
            {
                ColumnName = "Serial Number",
                PropertySelector = c => c.SerialNumber,
            },
            new UniTableRegularColumn<Transducer>
            {
                ColumnName = "Capacity min.",
                PropertySelector = c => c.MinimumCapacity,
            },
            new UniTableRegularColumn<Transducer>
            {
                ColumnName = "Capacity max.",
                PropertySelector = c => c.MaximumCapacity,
            },
            new UniTableRegularColumn<Transducer>
            {
                ColumnName = "Type",
                PropertySelector = c => c.Type,
            },
            new UniTableRegularColumn<Transducer>
            {
                ColumnName = "Capacity unit",
                PropertySelector = c => c.CapacityUnit,
            },
            new UniTableRegularColumn<Transducer>
            {
                ColumnName = "Measurement unit",
                PropertySelector = c => c.MeasurementUnit,
            },
        };

        TableOperations = new()
        {
            new UniTableAsyncAction
            {
                ToolTip = "Edit",
                IconKind = PackIconMaterialKind.PencilCircle,
                Command = (item) => OnEditClicked((Transducer)item),
            },
            new UniTableAsyncAction
            {
                ToolTip = "Delete",
                IconKind = PackIconMaterialKind.Delete,
                Command = (item) => OnDeleteClicked((Transducer)item),
            }
        };
    }
}
