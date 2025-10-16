using Calibrator.WpfControl.Controls.UniForm.Models;
using Calibrator.WpfControl.Validation;
using Calibrator.WpfControl.Validation.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Calibrator.WpfControl.Sandbox.Models;

// ============================================
// Equipment Models
// ============================================
public abstract class EquipmentBase : INotifyPropertyChanged
{
    private int _id;
    private string _name;
    private string _serialNumber;
    private string _manufacturer;
    private double? _yearOfManufacture;
    private string _location;
    private bool _isActive;

    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(); }
    }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public string SerialNumber
    {
        get => _serialNumber;
        set { _serialNumber = value; OnPropertyChanged(); }
    }

    public string Manufacturer
    {
        get => _manufacturer;
        set { _manufacturer = value; OnPropertyChanged(); }
    }

    public double? YearOfManufacture
    {
        get => _yearOfManufacture;
        set { _yearOfManufacture = value; OnPropertyChanged(); }
    }

    public string Location
    {
        get => _location;
        set { _location = value; OnPropertyChanged(); }
    }

    public bool IsActive
    {
        get => _isActive;
        set { _isActive = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName="")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
