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

public partial class ControllersOverviewViewModel : BaseViewModel
{
    private readonly IControllerRepository _controllerRepository;
    private readonly IDialogService _dialogService;
    private readonly IPromptDialogService _promptDialogService;

    [ObservableProperty] private List<UniTableColumn>? _columns;
    [ObservableProperty] private List<Controller>? _controllers;
    [ObservableProperty] private List<UniTableBaseAction>? _tableOperations;

    public ControllersOverviewViewModel(
        IControllerRepository controllerRepository,
        IDialogService dialogService,
        IPromptDialogService promptDialogService)
    {
        _controllerRepository = controllerRepository;
        _dialogService = dialogService;
        _promptDialogService = promptDialogService;

        InitUniTable();
    }

    public override async Task InitializeAsync()
    {
        await ExecuteWithLoading(ReloadControllers);
    }

    [RelayCommand]
    private async Task AddNew()
    {
        await ExecuteWithLoading(async () =>
        {
            _dialogService.Open<EditControllerDialogViewModel, Guid?>(null);
            await ReloadControllers();
        });
    }

    private async Task ReloadControllers()
    {
        Controllers = await _controllerRepository.GetAllWithNoTracking();
    }

    private async Task OnEditClicked(Controller controller)
    {
        await ExecuteWithLoading(async () =>
        {
            _dialogService.Open<EditControllerDialogViewModel, Guid?>(controller.Id);
            await ReloadControllers();
        });
    }

    private async Task OnDeleteClicked(Controller controller)
    {
        await ExecuteWithLoading(async () =>
        {
            if (!await _promptDialogService.AskForConfirmation("Are you sure you want to delete this controller?"))
            {
                return;
            }

            await _controllerRepository.Delete(controller.Id);
            await ReloadControllers();
        });
    }

    private void InitUniTable()
    {
        Columns = new()
        {
            new UniTableRegularColumn<Controller>
            {
                ColumnName = "Name",
                PropertySelector = c => c.Name,
            },
            new UniTableRegularColumn<Controller>
            {
                ColumnName = "Serial Number",
                PropertySelector = c => c.SerialNumber,
            },
            new UniTableRegularColumn<Controller>
            {
                ColumnName = "Identifier",
                PropertySelector = c => c.Identifier,
            },
            new UniTableRegularColumn<Controller>
            {
                ColumnName = "Type",
                PropertySelector = c => c.Type,
            },
            new UniTableRegularColumn<Controller>
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
                Command = (item) => OnEditClicked((Controller)item),
            },
            new UniTableAsyncAction
            {
                ToolTip = "Delete",
                IconKind = PackIconMaterialKind.Delete,
                Command = (item) => OnDeleteClicked((Controller)item),
            }
        };
    }
}

