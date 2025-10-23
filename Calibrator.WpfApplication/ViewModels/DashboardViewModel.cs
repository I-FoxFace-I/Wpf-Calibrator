using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Calibrator.WpfApplication.Infrastructure.Authentication;
using Calibrator.WpfApplication.Services;
using Calibrator.WpfApplication.Views;
using Calibrator.WpfApplication.Views.Components.UniTable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.DependencyInjection;

namespace Calibrator.WpfApplication.ViewModels;

public partial class DashboardViewModel : ObservableObject, IWindowNavigatableViewModel
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWindowNavigationService _windowNavigationService;
    [ObservableProperty] private List<UniTableColumn> _columns;
    [ObservableProperty] private List<Calibration> _calibrations;
    [ObservableProperty] private List<UniTableBaseAction> _tableOperations;

    [ObservableProperty] private string _loggedUserName;

    public DashboardViewModel(
        IServiceProvider serviceProvider, 
        IWindowNavigationService windowNavigationService,
        UserIdentificationContext userIdentificationContext)
    {
        _serviceProvider = serviceProvider;
        _windowNavigationService = windowNavigationService;

        LoggedUserName = userIdentificationContext.GetUser()?.FullName ?? "Unknown User";
        
        Calibrations = Enumerable.Range(0, 50).Select(num => new Calibration
        {
            ControllerId = $"Controller {num}",
            EquipmentId = $"Equipment {num}",
            Customer = $"Customer {num}",
            WorkOrderNumber = $"GB-{num}",
            SerialNumber = $"SerialNumber {num}",
            Process = $"Process {num}",
            Operator = $"Operator {num}",
            EquipmentCategory = $"Equipment Category {num}",
            Result = num % 2 == 0,
            Date = DateTime.Now.AddDays(num)
        }).ToList();

        Columns = new()
        {
            new UniTableRegularColumn<Calibration>
            {
                ColumnName = "Result",
                IsCheckBox = true,
                PropertySelector = c => c.Result,
            },
            new UniTableRegularColumn<Calibration>
            {
                ColumnName = "Controller",
                PropertySelector = c => c.ControllerId,
            },
            new UniTableRegularColumn<Calibration>
            {
                ColumnName = "Equipment",
                PropertySelector = c => c.EquipmentId
            },
            new UniTableRegularColumn<Calibration>
            {
                ColumnName = "Customer",
                PropertySelector = c => c.Customer
            },
            new UniTableRegularColumn<Calibration>
            {
                ColumnName = "WO Number",
                PropertySelector = c => c.WorkOrderNumber
            },
            new UniTableRegularColumn<Calibration>
            {
                ColumnName = "Serial Number",
                PropertySelector = c => c.SerialNumber
            },
            new UniTableRegularColumn<Calibration>
            {
                ColumnName = "Process",
                PropertySelector = c => c.Process
            },
            new UniTableRegularColumn<Calibration>
            {
                ColumnName = "Operator",
                PropertySelector = c => c.Operator
            },
            new UniTableRegularColumn<Calibration>
            {
                ColumnName = "Equipment Category",
                PropertySelector = c => c.EquipmentCategory
            },
            new UniTableRegularColumn<Calibration>
            {
                ColumnName = "Date",
                PropertySelector = c => c.Date
            },
        };

        TableOperations = new()
        {
            new UniTableAction
            {
                Command = (item) => Console.WriteLine(((Calibration)item).ControllerId), ToolTip = "Edit",
                IconKind = PackIconMaterialKind.Plus
            },
            new UniTableAction
            {
                Command = (item) => Console.WriteLine("DELETE"), ToolTip = "Delete",
                IconKind = PackIconMaterialKind.Delete
            }
        };
    }

    [RelayCommand]
    private void Works()
    {
        MessageBox.Show("Works!");
    }

    [RelayCommand]
    private void OpenControllers()
    {
        // Make factory for view resolve
        _serviceProvider.GetRequiredService<ControllersOverviewView>().ShowDialog();
    }

    [RelayCommand]
    private void OpenMeasuringInstruments()
    {
        // Make factory for view resolve
        _serviceProvider.GetRequiredService<MeasuringInstrumentsOverviewView>().ShowDialog();
    }

    [RelayCommand]
    private void OpenTransducers()
    {
        // Make factory for view resolve
        _serviceProvider.GetRequiredService<TransducersOverviewView>().ShowDialog();
    }

    [RelayCommand]
    private void OpenUsersManagement()
    {
        // TODO: Implement users management
        MessageBox.Show("Users Management not implemented yet");
    }
    
    [RelayCommand]
    private void OpenEquipmentTemplates()
    {
        // Make factory for view resolve
        _serviceProvider.GetRequiredService<EquipmentTemplatesOverviewView>().ShowDialog();
    }
    
    [RelayCommand]
    private void OpenEquipments()
    {
        // Make factory for view resolve
        _serviceProvider.GetRequiredService<EquipmentsOverviewView>().ShowDialog();
    }
}


