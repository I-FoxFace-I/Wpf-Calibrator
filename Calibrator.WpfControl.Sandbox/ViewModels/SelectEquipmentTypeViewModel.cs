using Calibrator.WpfControl.Controls.UniForm.Models;
using Calibrator.WpfControl.Sandbox.Commands;
using Calibrator.WpfControl.Sandbox.Views;
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

namespace Calibrator.WpfControl.Sandbox.ViewModels;

// ============================================
// Select Type Dialog ViewModel
// ============================================
public class SelectEquipmentTypeViewModel : INotifyPropertyChanged
{
    private string _selectedType;
    private string _typeDescription;

    public SelectEquipmentTypeViewModel()
    {
        AvailableTypes = new List<string>
        {
            "Controller",
            "Measuring Instrument",
            "Transducer"
        };

        InitializeCommands();
    }

    public List<string> AvailableTypes { get; }

    public string SelectedType
    {
        get => _selectedType;
        set
        {
            _selectedType = value;
            UpdateTypeDescription();
            OnPropertyChanged();
        }
    }

    public string TypeDescription
    {
        get => _typeDescription;
        set
        {
            _typeDescription = value;
            OnPropertyChanged();
        }
    }

    public ICommand ContinueCommand { get; private set; }
    public ICommand CancelCommand { get; private set; }

    private void InitializeCommands()
    {
        ContinueCommand = new RelayCommand(Continue, () => !string.IsNullOrEmpty(SelectedType));
        CancelCommand = new RelayCommand(Cancel);
    }

    private void UpdateTypeDescription()
    {
        TypeDescription = SelectedType switch
        {
            "Controller" => "A controller is a device that manages and regulates the operation of equipment or systems.",
            "Measuring Instrument" => "A measuring instrument is a device used to measure physical quantities such as pressure, temperature, or flow.",
            "Transducer" => "A transducer converts one form of energy or physical quantity into another, typically an electrical signal.",
            _ => "Please select an equipment type."
        };
    }

    private void Continue()
    {
        var window = Application.Current.Windows.OfType<SelectEquipmentTypeDialog>().FirstOrDefault();
        if (window != null)
        {
            window.DialogResult = true;
            window.Close();
        }
    }

    private void Cancel()
    {
        var window = Application.Current.Windows.OfType<SelectEquipmentTypeDialog>().FirstOrDefault();
        window?.Close();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
