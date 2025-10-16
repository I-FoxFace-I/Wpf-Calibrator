using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Calibrator.WpfControl.Controls.UniForm.Models;
using Calibrator.WpfControl.Sandbox.Commands;
using Calibrator.WpfControl.Sandbox.Models;
using Calibrator.WpfControl.Sandbox.Views;
using Calibrator.WpfControl.Validation;
using Calibrator.WpfControl.Validation.Validators;

namespace Calibrator.WpfControl.Sandbox.ViewModels;


/// <summary>
/// Equipment Dialog ViewModel.
/// </summary>
public class EquipmentDialogViewModel : INotifyPropertyChanged
{
    private EquipmentBase _equipment;
    private List<UniFormField> _formFields;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentDialogViewModel"/> class.
    /// </summary>
    /// <param name="equipment"></param>
    /// <param name="isNew"></param>
    /// <param name="isReadOnly"></param>
    public EquipmentDialogViewModel(EquipmentBase equipment, bool isNew, bool isReadOnly = false)
    {
        Equipment = equipment;
        IsNew = isNew;
        IsReadOnly = this.IsReadOnly;

        DialogTitle = this.IsReadOnly ? $"View {equipment.GetType().Name} - {equipment.Name}" :
                     isNew ? $"Add New {equipment.GetType().Name}" :
                     $"Edit {equipment.GetType().Name} - {equipment.Name}";

        InitializeFormFields();
        InitializeCommands();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    /// <summary>
    /// 
    /// </summary>
    public string DialogTitle { get; }

    public bool IsNew { get; }

    public bool IsReadOnly { get; }

    public EquipmentBase Equipment
    {
        get => _equipment;
        set
        {
            _equipment = value;
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

    public ICommand SaveCommand { get; private set; }
    public ICommand CancelCommand { get; private set; }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private void InitializeFormFields()
    {
        var fields = new List<UniFormField>();

        // Common fields for all equipment types
        fields.AddRange(GetCommonFields());

        // Type-specific fields
        fields.AddRange(Equipment switch
        {
            Controller c => GetControllerFields(),
            MeasuringInstrument m => GetMeasuringInstrumentFields(),
            Transducer t => GetTransducerFields(),
            _ => new List<UniFormField>()
        });

        FormFields = fields;
    }

    private List<UniFormField> GetCommonFields()
    {
        return new List<UniFormField>
        {
            new UniFormTextField<EquipmentBase>
            {
                Label = "Name",
                PropertySelector = e => e.Name,
                Order = 1,
                IsRequired = true,
                IsReadOnly = this.IsReadOnly,
                Validators = new List<IValidator<object>>
                {
                    new RequiredValidator("Name is required"),
                    new MinLengthValidator(3, "Name must be at least 3 characters")
                }
            },
            new UniFormTextField<EquipmentBase>
            {
                Label = "Serial Number",
                PropertySelector = e => e.SerialNumber,
                Order = 2,
                IsRequired = true,
                IsReadOnly = this.IsReadOnly,
                Validators = new List<IValidator<object>>
                {
                    new RequiredValidator("Serial number is required")
                }
            },
            new UniFormTextField<EquipmentBase>
            {
                Label = "Manufacturer",
                PropertySelector = e => e.Manufacturer,
                Order = 3,
                IsReadOnly = this.IsReadOnly
            },
            new UniFormNumericField<EquipmentBase>
            {
                Label = "Year of Manufacture",
                PropertySelector = e => e.YearOfManufacture,
                Order = 4,
                Minimum = 1900,
                Maximum = DateTime.Now.Year,
                IsReadOnly = this.IsReadOnly
            },
            new UniFormTextField<EquipmentBase>
            {
                Label = "Location",
                PropertySelector = e => e.Location,
                Order = 5,
                IsReadOnly = this.IsReadOnly
            },
            new UniFormCheckBoxField<EquipmentBase>
            {
                Label = "Active",
                PropertySelector = e => e.IsActive,
                Order = 6,
                IsReadOnly = this.IsReadOnly
            }
        };
    }

    private List<UniFormField> GetControllerFields()
    {
        return new List<UniFormField>
        {
            new UniFormTextField<Controller>
            {
                Label = "Controller Type",
                PropertySelector = c => c.ControllerType,
                Order = 10,
                IsReadOnly = this.IsReadOnly,
            },
            new UniFormTextField<Controller>
            {
                Label = "Firmware Version",
                PropertySelector = c => c.FirmwareVersion,
                Order = 11,
                IsReadOnly = this.IsReadOnly,
            },
        };
    }

    private List<UniFormField> GetMeasuringInstrumentFields()
    {
        return new List<UniFormField>
        {
            new UniFormTextField<MeasuringInstrument>
            {
                Label = "Measurement Type",
                PropertySelector = m => m.MeasurementType,
                Order = 10,
                IsReadOnly = this.IsReadOnly,
            },
            new UniFormNumericField<MeasuringInstrument>
            {
                Label = "Accuracy (%)",
                PropertySelector = m => m.Accuracy ?? default,
                Order = 11,
                Minimum = 0,
                Maximum = 100,
                Step = 0.1,
                IsReadOnly = this.IsReadOnly,
            },
        };
    }

    private List<UniFormField> GetTransducerFields()
    {
        return new List<UniFormField>
        {
            new UniFormTextField<Transducer>
            {
                Label = "Input Range",
                PropertySelector = t => t.InputRange ?? string.Empty,
                Order = 10,
                IsReadOnly = this.IsReadOnly,
            },
            new UniFormTextField<Transducer>
            {
                Label = "Output Signal",
                PropertySelector = t => t.OutputSignal ?? string.Empty,
                Order = 11,
                IsReadOnly = this.IsReadOnly,
            },
        };
    }

    private void InitializeCommands()
    {
        this.SaveCommand = new RelayCommand(Save, () => !IsReadOnly);
        this.CancelCommand = new RelayCommand(Cancel);
    }

    private void Save()
    {
        // In real app, validate and save to database
        var window = Application.Current.Windows.OfType<EquipmentDialog>().FirstOrDefault();
        if (window != null)
        {
            window.DialogResult = true;
            window.Close();
        }
    }

    private void Cancel()
    {
        var window = Application.Current.Windows.OfType<EquipmentDialog>().FirstOrDefault();
        window?.Close();
    }
}
