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
using Calibrator.WpfControl.Demo.Models;
using Calibrator.WpfControl.Validation;
using Calibrator.WpfControl.Validation.Validators;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfControl.Demo.Views;

// ============================================
// UserControl Code-Behind
// ============================================
public partial class SmartContainerDemo : UserControl
{
    public SmartContainerDemo()
    {
        InitializeComponent();
        DataContext = new SmartContainerDemoViewModel();
    }
}

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

// ============================================
// Device Type Selection Dialog
// ============================================
public class DeviceTypeSelectionDialog : Window
{
    public string? SelectedDeviceType { get; private set; }

    public DeviceTypeSelectionDialog()
    {
        Title = "Select Device Type";
        Width = 400;
        Height = 300;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        var viewModel = new DeviceTypeSelectionViewModel();
        viewModel.ContinueRequested += (type) =>
        {
            SelectedDeviceType = type;
            DialogResult = true;
            Close();
        };
        viewModel.CancelRequested += () =>
        {
            DialogResult = false;
            Close();
        };

        Content = new DeviceTypeSelectionView { DataContext = viewModel };
    }
}

public class DeviceTypeSelectionViewModel : INotifyPropertyChanged
{
    private string? _selectedType;
    private string? _typeDescription;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<string>? ContinueRequested;
    public event Action? CancelRequested;

    public List<string> DeviceTypes { get; } = new() { "Valve", "Pump", "Heat Exchanger" };

    public string? SelectedType
    {
        get => _selectedType;
        set
        {
            _selectedType = value;
            OnPropertyChanged();
            UpdateTypeDescription();
        }
    }

    public string? TypeDescription
    {
        get => _typeDescription;
        set
        {
            _typeDescription = value;
            OnPropertyChanged();
        }
    }

    public ICommand ContinueCommand => new RelayCommand(OnContinue, () => !string.IsNullOrEmpty(SelectedType));
    public ICommand CancelCommand => new RelayCommand(() => CancelRequested?.Invoke());

    private void OnContinue()
    {
        if (!string.IsNullOrEmpty(SelectedType))
        {
            ContinueRequested?.Invoke(SelectedType);
        }
    }

    private void UpdateTypeDescription()
    {
        TypeDescription = SelectedType switch
        {
            "Valve" => "Flow control device with various actuation types and materials",
            "Pump" => "Fluid transport device with specific flow and pressure characteristics",
            "Heat Exchanger" => "Thermal energy transfer device with tube/shell configuration",
            _ => "Please select a device type"
        };
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class DeviceTypeSelectionView : UserControl
{
    public DeviceTypeSelectionView()
    {
        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Title
        var title = new TextBlock
        {
            Text = "Select Device Type",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 20)
        };
        Grid.SetRow(title, 0);
        grid.Children.Add(title);

        // ListBox
        var listBox = new ListBox { Margin = new Thickness(0, 0, 0, 10) };
        listBox.SetBinding(ItemsControl.ItemsSourceProperty, new System.Windows.Data.Binding("DeviceTypes"));
        listBox.SetBinding(System.Windows.Controls.Primitives.Selector.SelectedItemProperty,
            new System.Windows.Data.Binding("SelectedType") { Mode = System.Windows.Data.BindingMode.TwoWay });
        Grid.SetRow(listBox, 1);
        grid.Children.Add(listBox);

        // Description
        var description = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 20)
        };
        description.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("TypeDescription"));
        Grid.SetRow(description, 2);
        grid.Children.Add(description);

        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var continueButton = new Button
        {
            Content = "Continue",
            Width = 100,
            Height = 35,
            Margin = new Thickness(0, 0, 10, 0)
        };
        continueButton.SetBinding(Button.CommandProperty, new System.Windows.Data.Binding("ContinueCommand"));

        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 100,
            Height = 35
        };
        cancelButton.SetBinding(Button.CommandProperty, new System.Windows.Data.Binding("CancelCommand"));

        buttonPanel.Children.Add(continueButton);
        buttonPanel.Children.Add(cancelButton);
        Grid.SetRow(buttonPanel, 3);
        grid.Children.Add(buttonPanel);

        Content = grid;
    }
}

// ============================================
// Device Edit Dialog
// ============================================
public class DeviceEditDialog : Window
{
    public IndustrialDevice UpdatedDevice { get; private set; }

    public DeviceEditDialog(IndustrialDevice device)
    {
        Title = $"Edit {device.DeviceType}";
        Width = 900;
        Height = 700;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var viewModel = new DeviceEditViewModel(device);
        viewModel.SaveRequested += (updatedDevice) =>
        {
            UpdatedDevice = updatedDevice;
            DialogResult = true;
            Close();
        };
        viewModel.CancelRequested += () =>
        {
            DialogResult = false;
            Close();
        };

        Content = new DeviceEditView { DataContext = viewModel };
        UpdatedDevice = device;
    }
}

// ============================================
// Device Edit ViewModel
// ============================================
public class DeviceEditViewModel : INotifyPropertyChanged
{
    private IndustrialDevice _device;
    private List<UniFormField> _formFields;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<IndustrialDevice>? SaveRequested;
    public event Action? CancelRequested;

    public DeviceEditViewModel(IndustrialDevice device)
    {
        _device = device;
        _formFields = CreateFormFields(device);
    }

    public IndustrialDevice Device
    {
        get => _device;
        set
        {
            _device = value;
            OnPropertyChanged();
        }
    }

    public List<UniFormField> FormFields
    {
        get => _formFields;
        set
        {
            _formFields = value;
            OnPropertyChanged();
        }
    }

    public ICommand SaveCommand => new RelayCommand(() => SaveRequested?.Invoke(Device));
    public ICommand CancelCommand => new RelayCommand(() => CancelRequested?.Invoke());

    private List<UniFormField> CreateFormFields(IndustrialDevice device)
    {
        var fields = new List<UniFormField>();

        // Basic Information fields (common for all devices)
        fields.AddRange(GetBasicInformationFields(device));

        // Type-specific fields
        fields.AddRange(device switch
        {
            Valve valve => GetValveFields(valve),
            Pump pump => GetPumpFields(pump),
            HeatExchanger heatExchanger => GetHeatExchangerFields(heatExchanger),
            _ => new List<UniFormField>()
        });

        return fields;
    }

    private List<UniFormField> GetBasicInformationFields(IndustrialDevice device)
    {
        return new List<UniFormField>
        {
            new UniFormTextField<IndustrialDevice>
            {
                Label = "Device Name",
                PropertySelector = x => x.DeviceName,
                Category = "Basic Information",
                Order = 1,
                IsRequired = true,
                Validators = new List<IValidator<object>>
                {
                    new RequiredValidator("Device name is required"),
                    new MinLengthValidator(3, "Name must be at least 3 characters")
                }
            },
            new UniFormTextField<IndustrialDevice>
            {
                Label = "Serial Number",
                PropertySelector = x => x.SerialNumber,
                Category = "Basic Information",
                Order = 2,
                IsRequired = true,
                Validators = new List<IValidator<object>>
                {
                    new RequiredValidator("Serial number is required")
                }
            },
            new UniFormTextField<IndustrialDevice>
            {
                Label = "Manufacturer",
                PropertySelector = x => x.Manufacturer,
                Category = "Basic Information",
                Order = 3,
                IsRequired = true
            },
            new UniFormTextField<IndustrialDevice>
            {
                Label = "Model Number",
                PropertySelector = x => x.ModelNumber,
                Category = "Basic Information",
                Order = 4,
                IsRequired = true
            },
            new UniFormTextField<IndustrialDevice>
            {
                Label = "Installation Location",
                PropertySelector = x => x.InstallationLocation,
                Category = "Installation Information",
                Order = 10
            },
            new UniFormCheckBoxField<IndustrialDevice>
            {
                Label = "Is Operational",
                PropertySelector = x => x.IsOperational,
                Category = "Installation Information",
                Order = 11
            },
            new UniFormTextField<IndustrialDevice>
            {
                Label = "Notes",
                PropertySelector = x => x.Notes,
                Category = "Installation Information",
                Order = 12,
                IsMultiline = true
            }
        };
    }

    private List<UniFormField> GetValveFields(Valve valve)
    {
        var valveTypes = new List<string> { "Ball Valve", "Gate Valve", "Globe Valve", "Butterfly Valve", "Check Valve" };
        var actuatorTypes = new List<string> { "Manual", "Pneumatic", "Electric", "Hydraulic" };
        var connectionTypes = new List<string> { "Flanged", "Threaded", "Welded", "Socket Weld" };

        return new List<UniFormField>
        {
            new UniFormComboBoxField<Valve>
            {
                Label = "Valve Type",
                PropertySelector = x => x.ValveType,
                Category = "Technical Specifications",
                Order = 20,
                ItemsSource = valveTypes,
                IsRequired = true
            },
            new UniFormNumericField<Valve>
            {
                Label = "Nominal Diameter (mm)",
                PropertySelector = x => x.NominalDiameter,
                Category = "Technical Specifications",
                Order = 21,
                Minimum = 10,
                Maximum = 1000,
                Step = 10
            },
            new UniFormNumericField<Valve>
            {
                Label = "Max Pressure (bar)",
                PropertySelector = x => x.MaxPressure,
                Category = "Technical Specifications",
                Order = 22,
                Minimum = 0,
                Maximum = 500,
                Step = 1
            },
            new UniFormNumericField<Valve>
            {
                Label = "Min Pressure (bar)",
                PropertySelector = x => x.MinPressure,
                Category = "Technical Specifications",
                Order = 23,
                Minimum = 0,
                Maximum = 500,
                Step = 1
            },
            new UniFormNumericField<Valve>
            {
                Label = "Max Temperature (°C)",
                PropertySelector = x => x.MaxTemperature,
                Category = "Technical Specifications",
                Order = 24,
                Minimum = -200,
                Maximum = 1000,
                Step = 10
            },
            new UniFormNumericField<Valve>
            {
                Label = "Min Temperature (°C)",
                PropertySelector = x => x.MinTemperature,
                Category = "Technical Specifications",
                Order = 25,
                Minimum = -200,
                Maximum = 1000,
                Step = 10
            },
            new UniFormTextField<Valve>
            {
                Label = "Body Material",
                PropertySelector = x => x.BodyMaterial,
                Category = "Technical Specifications",
                Order = 26
            },
            new UniFormTextField<Valve>
            {
                Label = "Seal Material",
                PropertySelector = x => x.SealMaterial,
                Category = "Technical Specifications",
                Order = 27
            },
            new UniFormComboBoxField<Valve>
            {
                Label = "Actuator Type",
                PropertySelector = x => x.ActuatorType,
                Category = "Operational Parameters",
                Order = 30,
                ItemsSource = actuatorTypes
            },
            new UniFormNumericField<Valve>
            {
                Label = "Flow Coefficient (Cv)",
                PropertySelector = x => x.FlowCoefficient,
                Category = "Operational Parameters",
                Order = 31,
                Minimum = 0,
                Maximum = 10000,
                Step = 1
            },
            new UniFormComboBoxField<Valve>
            {
                Label = "Connection Type",
                PropertySelector = x => x.ConnectionType,
                Category = "Operational Parameters",
                Order = 32,
                ItemsSource = connectionTypes
            },
            new UniFormCheckBoxField<Valve>
            {
                Label = "Has Position Indicator",
                PropertySelector = x => x.HasPositionIndicator,
                Category = "Operational Parameters",
                Order = 33
            },
            new UniFormNumericField<Valve>
            {
                Label = "Maintenance Interval (days)",
                PropertySelector = x => x.MaintenanceIntervalDays,
                Category = "Maintenance Information",
                Order = 40,
                Minimum = 1,
                Maximum = 3650,
                Step = 1
            }
        };
    }

    private List<UniFormField> GetPumpFields(Pump pump)
    {
        var pumpTypes = new List<string> { "Centrifugal - Single Stage", "Centrifugal - Multistage", "Positive Displacement", "Submersible", "Turbine" };
        var sealTypes = new List<string> { "Mechanical Seal", "Gland Packing", "Magnetic Drive", "Canned" };

        return new List<UniFormField>
        {
            new UniFormComboBoxField<Pump>
            {
                Label = "Pump Type",
                PropertySelector = x => x.PumpType,
                Category = "Technical Specifications",
                Order = 20,
                ItemsSource = pumpTypes,
                IsRequired = true
            },
            new UniFormNumericField<Pump>
            {
                Label = "Rated Flow Rate (m³/h)",
                PropertySelector = x => x.RatedFlowRate,
                Category = "Technical Specifications",
                Order = 21,
                Minimum = 0,
                Maximum = 10000,
                Step = 1
            },
            new UniFormNumericField<Pump>
            {
                Label = "Rated Head (m)",
                PropertySelector = x => x.RatedHead,
                Category = "Technical Specifications",
                Order = 22,
                Minimum = 0,
                Maximum = 1000,
                Step = 1
            },
            new UniFormNumericField<Pump>
            {
                Label = "Motor Power (kW)",
                PropertySelector = x => x.MotorPower,
                Category = "Technical Specifications",
                Order = 23,
                Minimum = 0,
                Maximum = 10000,
                Step = 0.1
            },
            new UniFormNumericField<Pump>
            {
                Label = "Rated Speed (rpm)",
                PropertySelector = x => x.RatedSpeed,
                Category = "Technical Specifications",
                Order = 24,
                Minimum = 0,
                Maximum = 10000,
                Step = 10
            },
            new UniFormNumericField<Pump>
            {
                Label = "Efficiency (%)",
                PropertySelector = x => x.Efficiency,
                Category = "Technical Specifications",
                Order = 25,
                Minimum = 0,
                Maximum = 100,
                Step = 0.1
            },
            new UniFormTextField<Pump>
            {
                Label = "Impeller Material",
                PropertySelector = x => x.ImpellerMaterial,
                Category = "Technical Specifications",
                Order = 26
            },
            new UniFormTextField<Pump>
            {
                Label = "Casing Material",
                PropertySelector = x => x.CasingMaterial,
                Category = "Technical Specifications",
                Order = 27
            },
            new UniFormComboBoxField<Pump>
            {
                Label = "Shaft Seal Type",
                PropertySelector = x => x.ShaftSealType,
                Category = "Operational Parameters",
                Order = 30,
                ItemsSource = sealTypes
            },
            new UniFormNumericField<Pump>
            {
                Label = "Max Flow Rate (m³/h)",
                PropertySelector = x => x.MaxFlowRate,
                Category = "Operational Parameters",
                Order = 31,
                Minimum = 0,
                Maximum = 10000,
                Step = 1
            },
            new UniFormNumericField<Pump>
            {
                Label = "Min Flow Rate (m³/h)",
                PropertySelector = x => x.MinFlowRate,
                Category = "Operational Parameters",
                Order = 32,
                Minimum = 0,
                Maximum = 10000,
                Step = 1
            },
            new UniFormCheckBoxField<Pump>
            {
                Label = "Has Variable Speed Drive",
                PropertySelector = x => x.HasVariableSpeedDrive,
                Category = "Operational Parameters",
                Order = 33
            },
            new UniFormNumericField<Pump>
            {
                Label = "Operating Hours",
                PropertySelector = x => x.OperatingHours,
                Category = "Maintenance Information",
                Order = 40,
                Minimum = 0,
                Maximum = 1000000,
                Step = 1,
                IsReadOnly = true
            }
        };
    }

    private List<UniFormField> GetHeatExchangerFields(HeatExchanger heatExchanger)
    {
        var exchangerTypes = new List<string> { "Shell and Tube", "Plate", "Spiral", "Air Cooled", "Double Pipe" };
        var fluidTypes = new List<string> { "Water", "Water/Glycol", "Steam", "Thermal Oil", "Refrigerant" };

        return new List<UniFormField>
        {
            new UniFormComboBoxField<HeatExchanger>
            {
                Label = "Exchanger Type",
                PropertySelector = x => x.ExchangerType,
                Category = "Technical Specifications",
                Order = 20,
                ItemsSource = exchangerTypes,
                IsRequired = true
            },
            new UniFormNumericField<HeatExchanger>
            {
                Label = "Heat Transfer Area (m²)",
                PropertySelector = x => x.HeatTransferArea,
                Category = "Technical Specifications",
                Order = 21,
                Minimum = 0,
                Maximum = 10000,
                Step = 0.1
            },
            new UniFormNumericField<HeatExchanger>
            {
                Label = "Heat Transfer Capacity (kW)",
                PropertySelector = x => x.HeatTransferCapacity,
                Category = "Technical Specifications",
                Order = 22,
                Minimum = 0,
                Maximum = 100000,
                Step = 1
            },
            new UniFormNumericField<HeatExchanger>
            {
                Label = "Design Pressure (bar)",
                PropertySelector = x => x.DesignPressure,
                Category = "Technical Specifications",
                Order = 23,
                Minimum = 0,
                Maximum = 500,
                Step = 1
            },
            new UniFormNumericField<HeatExchanger>
            {
                Label = "Design Temperature (°C)",
                PropertySelector = x => x.DesignTemperature,
                Category = "Technical Specifications",
                Order = 24,
                Minimum = -200,
                Maximum = 1000,
                Step = 10
            },
            new UniFormTextField<HeatExchanger>
            {
                Label = "Tube Material",
                PropertySelector = x => x.TubeMaterial,
                Category = "Technical Specifications",
                Order = 25
            },
            new UniFormTextField<HeatExchanger>
            {
                Label = "Shell Material",
                PropertySelector = x => x.ShellMaterial,
                Category = "Technical Specifications",
                Order = 26
            },
            new UniFormNumericField<HeatExchanger>
            {
                Label = "Number of Tubes",
                PropertySelector = x => x.NumberOfTubes,
                Category = "Construction Details",
                Order = 27,
                Minimum = 1,
                Maximum = 10000,
                Step = 1
            },
            new UniFormNumericField<HeatExchanger>
            {
                Label = "Tube Length (m)",
                PropertySelector = x => x.TubeLength,
                Category = "Construction Details",
                Order = 28,
                Minimum = 0.1,
                Maximum = 50,
                Step = 0.1
            },
            new UniFormNumericField<HeatExchanger>
            {
                Label = "Tube Diameter (mm)",
                PropertySelector = x => x.TubeDiameter,
                Category = "Construction Details",
                Order = 29,
                Minimum = 1,
                Maximum = 200,
                Step = 1
            },
            new UniFormComboBoxField<HeatExchanger>
            {
                Label = "Fluid Type",
                PropertySelector = x => x.FluidType,
                Category = "Operational Parameters",
                Order = 30,
                ItemsSource = fluidTypes
            },
            new UniFormCheckBoxField<HeatExchanger>
            {
                Label = "Has Insulation",
                PropertySelector = x => x.HasInsulation,
                Category = "Operational Parameters",
                Order = 31
            },
            new UniFormNumericField<HeatExchanger>
            {
                Label = "Test Interval (months)",
                PropertySelector = x => x.TestIntervalMonths,
                Category = "Maintenance Information",
                Order = 40,
                Minimum = 1,
                Maximum = 120,
                Step = 1
            }
        };
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Device Edit View
public class DeviceEditView : UserControl
{
    public DeviceEditView()
    {
        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Title
        var title = new TextBlock
        {
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 20)
        };
        title.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Device.DeviceType")
        {
            StringFormat = "Edit {0}"
        });
        Grid.SetRow(title, 0);
        grid.Children.Add(title);

        // SmartContainer
        var smartContainer = new Controls.ScSmartContainer.ScSmartContainerComponent
        {
            Columns = 2,
            HorizontalSpacing = 16,
            VerticalSpacing = 12
        };
        smartContainer.SetBinding(Controls.ScSmartContainer.ScSmartContainerComponent.FieldsProperty,
            new System.Windows.Data.Binding("FormFields"));
        smartContainer.SetBinding(Controls.ScSmartContainer.ScSmartContainerComponent.DataContextProperty,
            new System.Windows.Data.Binding("Device"));

        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(0, 0, 0, 20)
        };
        scrollViewer.Content = smartContainer;
        Grid.SetRow(scrollViewer, 1);
        grid.Children.Add(scrollViewer);

        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var saveButton = new Button
        {
            Content = "Save",
            Width = 100,
            Height = 35,
            Margin = new Thickness(0, 0, 10, 0)
        };
        saveButton.SetBinding(Button.CommandProperty, new System.Windows.Data.Binding("SaveCommand"));

        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 100,
            Height = 35
        };
        cancelButton.SetBinding(Button.CommandProperty, new System.Windows.Data.Binding("CancelCommand"));

        buttonPanel.Children.Add(saveButton);
        buttonPanel.Children.Add(cancelButton);
        Grid.SetRow(buttonPanel, 2);
        grid.Children.Add(buttonPanel);

        Content = grid;
    }
}

