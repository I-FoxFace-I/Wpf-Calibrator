using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Calibrator.WpfControl.Controls.UniForm.Models;
using Calibrator.WpfControl.Controls.UniTable;
using Calibrator.WpfControl.Sandbox.Commands;
using Calibrator.WpfControl.Sandbox.Models;
using Calibrator.WpfControl.Sandbox.Views;
using Calibrator.WpfControl.Validation;
using Calibrator.WpfControl.Validation.Validators;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfControl.Sandbox.ViewModels;

// ============================================
// Main ViewModel
// ============================================
public class SmartContainerDemoViewModel : INotifyPropertyChanged
{
    private ObservableCollection<IndustrialDevice> _devices;

    public SmartContainerDemoViewModel()
    {
        InitializeData();
        InitializeCommands();
    }

    public ObservableCollection<IndustrialDevice> Devices
    {
        get => _devices;
        set
        {
            _devices = value;
            OnPropertyChanged();
        }
    }

    public List<UniTableColumn> Columns { get; set; }
    public List<UniTableBaseAction> TableOperations { get; set; }

    public ICommand OpenAddDialogCommand { get; private set; }
    public ICommand RefreshCommand { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void InitializeData()
    {
        // Sample device data
        Devices = new ObservableCollection<IndustrialDevice>
        {
            new Valve
            {
                Id = 1,
                DeviceName = "Main Control Valve V-101",
                SerialNumber = "VLV-2023-001",
                Manufacturer = "Emerson",
                ModelNumber = "Fisher EZ 2000",
                InstallationDate = new DateTime(2023, 3, 15),
                InstallationLocation = "Process Area A - Line 1",
                IsOperational = true,
                ValveType = "Ball Valve",
                NominalDiameter = 150,
                MaxPressure = 40,
                MinPressure = 0,
                MaxTemperature = 180,
                MinTemperature = -20,
                BodyMaterial = "Stainless Steel 316",
                SealMaterial = "PTFE",
                ActuatorType = "Pneumatic",
                FlowCoefficient = 210,
                ConnectionType = "Flanged",
                HasPositionIndicator = true,
                LastMaintenanceDate = DateTime.Now.AddMonths(-3),
                MaintenanceIntervalDays = 180
            },
            new Pump
            {
                Id = 2,
                DeviceName = "Centrifugal Pump P-201",
                SerialNumber = "PMP-2022-045",
                Manufacturer = "Grundfos",
                ModelNumber = "CR 64-3",
                InstallationDate = new DateTime(2022, 11, 20),
                InstallationLocation = "Pump Station B",
                IsOperational = true,
                PumpType = "Centrifugal - Multistage",
                RatedFlowRate = 120,
                RatedHead = 85,
                MotorPower = 37,
                RatedSpeed = 2900,
                Efficiency = 82.5,
                ImpellerMaterial = "Cast Iron",
                CasingMaterial = "Cast Steel",
                ShaftSealType = "Mechanical Seal",
                MaxFlowRate = 150,
                MinFlowRate = 30,
                HasVariableSpeedDrive = true,
                LastInspectionDate = DateTime.Now.AddMonths(-1),
                OperatingHours = 12500
            },
            new HeatExchanger
            {
                Id = 3,
                DeviceName = "Shell & Tube Heat Exchanger HX-301",
                SerialNumber = "HEX-2021-078",
                Manufacturer = "Alfa Laval",
                ModelNumber = "AC 500 EQ",
                InstallationDate = new DateTime(2021, 6, 10),
                InstallationLocation = "Heat Recovery Unit",
                IsOperational = true,
                ExchangerType = "Shell and Tube",
                HeatTransferArea = 125,
                HeatTransferCapacity = 850,
                DesignPressure = 25,
                DesignTemperature = 200,
                TubeMaterial = "Stainless Steel 304",
                ShellMaterial = "Carbon Steel",
                NumberOfTubes = 248,
                TubeLength = 6.0,
                TubeDiameter = 19,
                FluidType = "Water/Glycol",
                HasInsulation = true,
                LastPressureTestDate = DateTime.Now.AddMonths(-12),
                TestIntervalMonths = 24
            }
        };

        InitializeColumns();
        InitializeTableOperations();
    }

    private void InitializeColumns()
    {
        Columns = new List<UniTableColumn>
        {
            new UniTableRegularColumn<IndustrialDevice>
            {
                ColumnName = "Type",
                PropertySelector = d => d.DeviceType,
                Width = 150
            },
            new UniTableRegularColumn<IndustrialDevice>
            {
                ColumnName = "Device Name",
                PropertySelector = d => d.DeviceName,
                Width = 250
            },
            new UniTableRegularColumn<IndustrialDevice>
            {
                ColumnName = "Serial Number",
                PropertySelector = d => d.SerialNumber,
                Width = 150
            },
            new UniTableRegularColumn<IndustrialDevice>
            {
                ColumnName = "Manufacturer",
                PropertySelector = d => d.Manufacturer,
                Width = 150
            },
            new UniTableRegularColumn<IndustrialDevice>
            {
                ColumnName = "Location",
                PropertySelector = d => d.InstallationLocation,
                Width = 200
            },
            new UniTableRegularColumn<IndustrialDevice>
            {
                ColumnName = "Operational",
                PropertySelector = d => d.IsOperational,
                IsCheckBox = true,
                Width = 120
            }
        };
    }

    private void InitializeTableOperations()
    {
        TableOperations = new List<UniTableBaseAction>
        {
            new UniTableAction
            {
                Command = EditDevice,
                ToolTip = "Edit",
                IconKind = PackIconMaterialKind.Pencil
            },
            new UniTableAction
            {
                Command = DeleteDevice,
                ToolTip = "Delete",
                IconKind = PackIconMaterialKind.Delete
            }
        };
    }

    private void EditDevice(object? parameter)
    {
        if (parameter is IndustrialDevice device)
        {
            var dialog = new DeviceEditDialog(device);
            if (dialog.ShowDialog() == true)
            {
                // Update device in collection
                var index = Devices.IndexOf(device);
                if (index >= 0)
                {
                    Devices[index] = dialog.UpdatedDevice;
                }
            }
        }
    }

    private void DeleteDevice(object? parameter)
    {
        if (parameter is IndustrialDevice device)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete device '{device.DeviceName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Devices.Remove(device);
            }
        }
    }

    private void InitializeCommands()
    {
        OpenAddDialogCommand = new RelayCommand(OpenAddDialog);
        RefreshCommand = new RelayCommand(RefreshData);
    }

    private void OpenAddDialog()
    {
        var selectionDialog = new DeviceTypeSelectionDialog();
        if (selectionDialog.ShowDialog() == true)
        {
            IndustrialDevice? newDevice = selectionDialog.SelectedDeviceType switch
            {
                "Valve" => new Valve(),
                "Pump" => new Pump(),
                "Heat Exchanger" => new HeatExchanger(),
                _ => null
            };

            if (newDevice != null)
            {
                newDevice.Id = Devices.Any() ? Devices.Max(d => d.Id) + 1 : 1;
                newDevice.InstallationDate = DateTime.Now;

                var editDialog = new DeviceEditDialog(newDevice);
                if (editDialog.ShowDialog() == true)
                {
                    Devices.Add(editDialog.UpdatedDevice);
                }
            }
        }
    }

    private void RefreshData()
    {
        MessageBox.Show("Data refreshed!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

