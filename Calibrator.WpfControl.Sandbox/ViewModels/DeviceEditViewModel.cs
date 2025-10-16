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
using Calibrator.WpfControl.Validation;
using Calibrator.WpfControl.Validation.Validators;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfControl.Sandbox.ViewModels;

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

