using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Calibrator.WpfApplication.Models;
using Calibrator.WpfApplication.Services;
using Calibrator.WpfApplication.Views;
using Calibrator.WpfApplication.Views.Components.UniTable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.DependencyInjection;

namespace Calibrator.WpfApplication.ViewModels;

public partial class MainWindowViewModel : BaseViewModel
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty] private List<UniTableColumn> _columns;
    [ObservableProperty] private List<Calibration> _calibrations;
    [ObservableProperty] private List<UniTableBaseAction> _tableOperations;
    [ObservableProperty] private string _loggedUserName = "Demo User";

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        // Initialize with sample data
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
            Date = DateTime.Now.AddDays(-num)
        }).ToList();

        InitializeColumns();
        InitializeTableOperations();
    }

    public override Task InitializeAsync()
    {
        // No async initialization needed for demo
        return Task.CompletedTask;
    }

    private void InitializeColumns()
    {
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
    }

    private void InitializeTableOperations()
    {
        TableOperations = new()
        {
            new UniTableAction
            {
                Command = (item) => MessageBox.Show($"View calibration: {((Calibration)item).ControllerId}"),
                ToolTip = "View",
                IconKind = PackIconMaterialKind.Eye
            },
            new UniTableAction
            {
                Command = (item) => MessageBox.Show($"Delete calibration: {((Calibration)item).ControllerId}"),
                ToolTip = "Delete",
                IconKind = PackIconMaterialKind.Delete
            }
        };
    }

    [RelayCommand]
    private void OpenControllers()
    {
        var view = _serviceProvider.GetRequiredService<ControllersOverviewView>();
        view.ShowDialog();
    }

    [RelayCommand]
    private void OpenMeasuringInstruments()
    {
        var view = _serviceProvider.GetRequiredService<MeasuringInstrumentsOverviewView>();
        view.ShowDialog();
    }

    [RelayCommand]
    private void OpenTransducers()
    {
        var view = _serviceProvider.GetRequiredService<TransducersOverviewView>();
        view.ShowDialog();
    }
    
    [RelayCommand]
    private void OpenEquipmentTemplates()
    {
        var view = _serviceProvider.GetRequiredService<EquipmentTemplatesOverviewView>();
        view.ShowDialog();
    }
    
    [RelayCommand]
    private void OpenEquipments()
    {
        var view = _serviceProvider.GetRequiredService<EquipmentsOverviewView>();
        view.ShowDialog();
    }

    [RelayCommand]
    private void ShowMessage()
    {
        MessageBox.Show("Demo functionality");
    }
}


// Demo class for calibration data
public class Calibration
{
    public bool Result { get; set; }
    public string EquipmentId { get; set; } = string.Empty;
    public string EquipmentCategory { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Process { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string ControllerId { get; set; } = string.Empty;
    public DateTime Date { get; set; }

}


