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

