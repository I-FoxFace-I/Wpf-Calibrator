using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Calibrator.WpfControl.Demo.Models;

// Base class for all industrial devices
public abstract class IndustrialDevice : INotifyPropertyChanged
{
    private int _id;
    private string _deviceName = string.Empty;
    private string _serialNumber = string.Empty;
    private string _manufacturer = string.Empty;
    private string _modelNumber = string.Empty;
    private DateTime _installationDate;
    private string _installationLocation = string.Empty;
    private bool _isOperational;
    private string _notes = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    // Basic Information Category
    public int Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public string DeviceName
    {
        get => _deviceName;
        set => SetField(ref _deviceName, value);
    }

    public string SerialNumber
    {
        get => _serialNumber;
        set => SetField(ref _serialNumber, value);
    }

    public string Manufacturer
    {
        get => _manufacturer;
        set => SetField(ref _manufacturer, value);
    }

    public string ModelNumber
    {
        get => _modelNumber;
        set => SetField(ref _modelNumber, value);
    }

    // Installation Information Category
    public DateTime InstallationDate
    {
        get => _installationDate;
        set => SetField(ref _installationDate, value);
    }

    public string InstallationLocation
    {
        get => _installationLocation;
        set => SetField(ref _installationLocation, value);
    }

    public bool IsOperational
    {
        get => _isOperational;
        set => SetField(ref _isOperational, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetField(ref _notes, value);
    }

    public abstract string DeviceType { get; }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

// Valve - ventil
public class Valve : IndustrialDevice
{
    private string _valveType = string.Empty;
    private double _nominalDiameter;
    private double _maxPressure;
    private double _minPressure;
    private double _maxTemperature;
    private double _minTemperature;
    private string _bodyMaterial = string.Empty;
    private string _sealMaterial = string.Empty;
    private string _actuatorType = string.Empty;
    private double _flowCoefficient;
    private string _connectionType = string.Empty;
    private bool _hasPositionIndicator;
    private DateTime? _lastMaintenanceDate;
    private int _maintenanceIntervalDays;

    public override string DeviceType => "Valve";

    // Technical Specifications Category
    public string ValveType
    {
        get => _valveType;
        set => SetField(ref _valveType, value);
    }

    public double NominalDiameter
    {
        get => _nominalDiameter;
        set => SetField(ref _nominalDiameter, value);
    }

    public double MaxPressure
    {
        get => _maxPressure;
        set => SetField(ref _maxPressure, value);
    }

    public double MinPressure
    {
        get => _minPressure;
        set => SetField(ref _minPressure, value);
    }

    public double MaxTemperature
    {
        get => _maxTemperature;
        set => SetField(ref _maxTemperature, value);
    }

    public double MinTemperature
    {
        get => _minTemperature;
        set => SetField(ref _minTemperature, value);
    }

    public string BodyMaterial
    {
        get => _bodyMaterial;
        set => SetField(ref _bodyMaterial, value);
    }

    public string SealMaterial
    {
        get => _sealMaterial;
        set => SetField(ref _sealMaterial, value);
    }

    // Operational Parameters Category
    public string ActuatorType
    {
        get => _actuatorType;
        set => SetField(ref _actuatorType, value);
    }

    public double FlowCoefficient
    {
        get => _flowCoefficient;
        set => SetField(ref _flowCoefficient, value);
    }

    public string ConnectionType
    {
        get => _connectionType;
        set => SetField(ref _connectionType, value);
    }

    public bool HasPositionIndicator
    {
        get => _hasPositionIndicator;
        set => SetField(ref _hasPositionIndicator, value);
    }

    // Maintenance Information Category
    public DateTime? LastMaintenanceDate
    {
        get => _lastMaintenanceDate;
        set => SetField(ref _lastMaintenanceDate, value);
    }

    public int MaintenanceIntervalDays
    {
        get => _maintenanceIntervalDays;
        set => SetField(ref _maintenanceIntervalDays, value);
    }
}

// Pump - čerpadlo
public class Pump : IndustrialDevice
{
    private string _pumpType = string.Empty;
    private double _ratedFlowRate;
    private double _ratedHead;
    private double _motorPower;
    private int _ratedSpeed;
    private double _efficiency;
    private string _impellerMaterial = string.Empty;
    private string _casingMaterial = string.Empty;
    private string _shaftSealType = string.Empty;
    private double _maxFlowRate;
    private double _minFlowRate;
    private bool _hasVariableSpeedDrive;
    private DateTime? _lastInspectionDate;
    private int _operatingHours;

    public override string DeviceType => "Pump";

    // Technical Specifications Category
    public string PumpType
    {
        get => _pumpType;
        set => SetField(ref _pumpType, value);
    }

    public double RatedFlowRate
    {
        get => _ratedFlowRate;
        set => SetField(ref _ratedFlowRate, value);
    }

    public double RatedHead
    {
        get => _ratedHead;
        set => SetField(ref _ratedHead, value);
    }

    public double MotorPower
    {
        get => _motorPower;
        set => SetField(ref _motorPower, value);
    }

    public int RatedSpeed
    {
        get => _ratedSpeed;
        set => SetField(ref _ratedSpeed, value);
    }

    public double Efficiency
    {
        get => _efficiency;
        set => SetField(ref _efficiency, value);
    }

    public string ImpellerMaterial
    {
        get => _impellerMaterial;
        set => SetField(ref _impellerMaterial, value);
    }

    public string CasingMaterial
    {
        get => _casingMaterial;
        set => SetField(ref _casingMaterial, value);
    }

    // Operational Parameters Category
    public string ShaftSealType
    {
        get => _shaftSealType;
        set => SetField(ref _shaftSealType, value);
    }

    public double MaxFlowRate
    {
        get => _maxFlowRate;
        set => SetField(ref _maxFlowRate, value);
    }

    public double MinFlowRate
    {
        get => _minFlowRate;
        set => SetField(ref _minFlowRate, value);
    }

    public bool HasVariableSpeedDrive
    {
        get => _hasVariableSpeedDrive;
        set => SetField(ref _hasVariableSpeedDrive, value);
    }

    // Maintenance Information Category
    public DateTime? LastInspectionDate
    {
        get => _lastInspectionDate;
        set => SetField(ref _lastInspectionDate, value);
    }

    public int OperatingHours
    {
        get => _operatingHours;
        set => SetField(ref _operatingHours, value);
    }
}

// Heat Exchanger - výměník tepla
public class HeatExchanger : IndustrialDevice
{
    private string _exchangerType = string.Empty;
    private double _heatTransferArea;
    private double _heatTransferCapacity;
    private double _designPressure;
    private double _designTemperature;
    private string _tubeMaterial = string.Empty;
    private string _shellMaterial = string.Empty;
    private int _numberOfTubes;
    private double _tubeLength;
    private double _tubeDiameter;
    private string _fluidType = string.Empty;
    private bool _hasInsulation;
    private DateTime? _lastPressureTestDate;
    private int _testIntervalMonths;

    public override string DeviceType => "Heat Exchanger";

    // Technical Specifications Category
    public string ExchangerType
    {
        get => _exchangerType;
        set => SetField(ref _exchangerType, value);
    }

    public double HeatTransferArea
    {
        get => _heatTransferArea;
        set => SetField(ref _heatTransferArea, value);
    }

    public double HeatTransferCapacity
    {
        get => _heatTransferCapacity;
        set => SetField(ref _heatTransferCapacity, value);
    }

    public double DesignPressure
    {
        get => _designPressure;
        set => SetField(ref _designPressure, value);
    }

    public double DesignTemperature
    {
        get => _designTemperature;
        set => SetField(ref _designTemperature, value);
    }

    public string TubeMaterial
    {
        get => _tubeMaterial;
        set => SetField(ref _tubeMaterial, value);
    }

    public string ShellMaterial
    {
        get => _shellMaterial;
        set => SetField(ref _shellMaterial, value);
    }

    // Construction Details Category
    public int NumberOfTubes
    {
        get => _numberOfTubes;
        set => SetField(ref _numberOfTubes, value);
    }

    public double TubeLength
    {
        get => _tubeLength;
        set => SetField(ref _tubeLength, value);
    }

    public double TubeDiameter
    {
        get => _tubeDiameter;
        set => SetField(ref _tubeDiameter, value);
    }

    // Operational Parameters Category
    public string FluidType
    {
        get => _fluidType;
        set => SetField(ref _fluidType, value);
    }

    public bool HasInsulation
    {
        get => _hasInsulation;
        set => SetField(ref _hasInsulation, value);
    }

    // Maintenance Information Category
    public DateTime? LastPressureTestDate
    {
        get => _lastPressureTestDate;
        set => SetField(ref _lastPressureTestDate, value);
    }

    public int TestIntervalMonths
    {
        get => _testIntervalMonths;
        set => SetField(ref _testIntervalMonths, value);
    }
}

