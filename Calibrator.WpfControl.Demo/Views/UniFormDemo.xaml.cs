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
using Calibrator.WpfControl.Validation;
using Calibrator.WpfControl.Validation.Validators;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfControl.Demo.Views;

// ============================================
// UserControl Code-Behind
// ============================================
public partial class UniFormDemo : UserControl
{
    public UniFormDemo()
    {
        InitializeComponent();
        DataContext = new UniFormDemoViewModel();
    }
}

// ============================================
// Main ViewModel
// ============================================
public class UniFormDemoViewModel : INotifyPropertyChanged
{
    private ObservableCollection<EquipmentBase> _equipment;

    public UniFormDemoViewModel()
    {
        InitializeData();
        InitializeCommands();
    }

    public ObservableCollection<EquipmentBase> Equipment
    {
        get => _equipment;
        set
        {
            _equipment = value;
            OnPropertyChanged();
        }
    }

    public List<UniTableColumn> Columns { get; set; }
    public List<UniTableBaseAction> TableOperations { get; set; }

    public ICommand OpenAddDialogCommand { get; private set; }
    public ICommand RefreshCommand { get; private set; }

    private void InitializeData()
    {
        // Sample equipment data
        Equipment = new ObservableCollection<EquipmentBase>
        {
            new Controller
            {
                Id = 1,
                Name = "Controller A1",
                SerialNumber = "CTRL-001",
                Manufacturer = "Siemens",
                YearOfManufacture = 2022,
                Location = "Building A",
                IsActive = true,
                ControllerType = "PLC",
                FirmwareVersion = "v2.1.5"
            },
            new MeasuringInstrument
            {
                Id = 2,
                Name = "Pressure Sensor PS-100",
                SerialNumber = "MSUR-002",
                Manufacturer = "Endress+Hauser",
                YearOfManufacture = 2021,
                Location = "Building B",
                IsActive = true,
                MeasurementType = "Pressure",
                Accuracy = 0.5,
                CalibrationDate = DateTime.Now.AddMonths(-6)
            },
            new Transducer
            {
                Id = 3,
                Name = "Temperature Transducer TT-50",
                SerialNumber = "TRNS-003",
                Manufacturer = "Omega",
                YearOfManufacture = 2023,
                Location = "Building A",
                IsActive = true,
                InputRange = "-50 to 200�C",
                OutputSignal = "4-20mA"
            }
        };

        // Define columns
        Columns = new List<UniTableColumn>
        {
            new UniTableRegularColumn<EquipmentBase>
            {
                ColumnName = "ID",
                PropertySelector = e => e.Id,
                Width = 60
            },
            new UniTableRegularColumn<EquipmentBase>
            {
                ColumnName = "Type",
                PropertySelector = e => e.GetType().Name,
                Width = 150
            },
            new UniTableRegularColumn<EquipmentBase>
            {
                ColumnName = "Name",
                PropertySelector = e => e.Name,
                Width = 200
            },
            new UniTableRegularColumn<EquipmentBase>
            {
                ColumnName = "Serial Number",
                PropertySelector = e => e.SerialNumber,
                Width = 150
            },
            new UniTableRegularColumn<EquipmentBase>
            {
                ColumnName = "Manufacturer",
                PropertySelector = e => e.Manufacturer,
                Width = 150
            },
            new UniTableRegularColumn<EquipmentBase>
            {
                ColumnName = "Active",
                PropertySelector = e => e.IsActive,
                IsCheckBox = true,
                Width = 80
            }
        };

        // Define operations
        TableOperations = new List<UniTableBaseAction>
        {
            new UniTableAction
            {
                Command = OpenEditDialog,
                ToolTip = "Edit",
                IconKind = PackIconMaterialKind.Pencil
            },
            new UniTableAction
            {
                Command = OpenViewDialog,
                ToolTip = "View Details",
                IconKind = PackIconMaterialKind.Eye
            },
            new UniTableAction
            {
                Command = DeleteEquipment,
                ToolTip = "Delete",
                IconKind = PackIconMaterialKind.Delete
            }
        };
    }

    private void InitializeCommands()
    {
        OpenAddDialogCommand = new RelayCommand(OpenAddDialog);
        RefreshCommand = new RelayCommand(Refresh);
    }

    private void OpenAddDialog()
    {
        // First, show type selection dialog
        var typeDialog = new SelectEquipmentTypeDialog();
        var typeViewModel = new SelectEquipmentTypeViewModel();
        typeDialog.DataContext = typeViewModel;

        if (typeDialog.ShowDialog() == true && typeViewModel.SelectedType != null)
        {
            // Create new equipment based on selected type
            EquipmentBase newEquipment = typeViewModel.SelectedType switch
            {
                "Controller" => new Controller(),
                "Measuring Instrument" => new MeasuringInstrument(),
                "Transducer" => new Transducer(),
                _ => null
            };

            if (newEquipment != null)
            {
                // Open edit dialog for new equipment
                var dialog = new EquipmentDialog();
                var viewModel = new EquipmentDialogViewModel(newEquipment, true);
                dialog.DataContext = viewModel;

                if (dialog.ShowDialog() == true)
                {
                    newEquipment.Id = Equipment.Max(e => e.Id) + 1;
                    Equipment.Add(newEquipment);
                }
            }
        }
    }

    private void OpenEditDialog(object item)
    {
        if (item is EquipmentBase equipment)
        {
            var dialog = new EquipmentDialog();
            var viewModel = new EquipmentDialogViewModel(equipment, false);
            dialog.DataContext = viewModel;
            dialog.ShowDialog();
        }
    }

    private void OpenViewDialog(object item)
    {
        if (item is EquipmentBase equipment)
        {
            var dialog = new EquipmentDialog();
            var viewModel = new EquipmentDialogViewModel(equipment, false, isReadOnly: true);
            dialog.DataContext = viewModel;
            dialog.ShowDialog();
        }
    }

    private void DeleteEquipment(object item)
    {
        if (item is EquipmentBase equipment)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete '{equipment.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Equipment.Remove(equipment);
            }
        }
    }

    private void Refresh()
    {
        // Reload data (in real app, would fetch from database)
        OnPropertyChanged(nameof(Equipment));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// ============================================
// Equipment Dialog ViewModel
// ============================================
public class EquipmentDialogViewModel : INotifyPropertyChanged
{
    private EquipmentBase _equipment;
    private List<UniFormField> _formFields;

    public EquipmentDialogViewModel(EquipmentBase equipment, bool isNew, bool isReadOnly = false)
    {
        Equipment = equipment;
        IsNew = isNew;
        IsReadOnly = isReadOnly;

        DialogTitle = isReadOnly ? $"View {equipment.GetType().Name} - {equipment.Name}" :
                     isNew ? $"Add New {equipment.GetType().Name}" :
                     $"Edit {equipment.GetType().Name} - {equipment.Name}";

        InitializeFormFields();
        InitializeCommands();
    }

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
                IsReadOnly = IsReadOnly,
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
                IsReadOnly = IsReadOnly,
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
                IsReadOnly = IsReadOnly
            },
            new UniFormNumericField<EquipmentBase>
            {
                Label = "Year of Manufacture",
                PropertySelector = e => e.YearOfManufacture,
                Order = 4,
                Minimum = 1900,
                Maximum = DateTime.Now.Year,
                IsReadOnly = IsReadOnly
            },
            new UniFormTextField<EquipmentBase>
            {
                Label = "Location",
                PropertySelector = e => e.Location,
                Order = 5,
                IsReadOnly = IsReadOnly
            },
            new UniFormCheckBoxField<EquipmentBase>
            {
                Label = "Active",
                PropertySelector = e => e.IsActive,
                Order = 6,
                IsReadOnly = IsReadOnly
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
                IsReadOnly = IsReadOnly
            },
            new UniFormTextField<Controller>
            {
                Label = "Firmware Version",
                PropertySelector = c => c.FirmwareVersion,
                Order = 11,
                IsReadOnly = IsReadOnly
            }
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
                IsReadOnly = IsReadOnly
            },
            new UniFormNumericField<MeasuringInstrument>
            {
                Label = "Accuracy (%)",
                PropertySelector = m => m.Accuracy,
                Order = 11,
                Minimum = 0,
                Maximum = 100,
                Step = 0.1,
                IsReadOnly = IsReadOnly
            }
        };
    }

    private List<UniFormField> GetTransducerFields()
    {
        return new List<UniFormField>
        {
            new UniFormTextField<Transducer>
            {
                Label = "Input Range",
                PropertySelector = t => t.InputRange,
                Order = 10,
                IsReadOnly = IsReadOnly
            },
            new UniFormTextField<Transducer>
            {
                Label = "Output Signal",
                PropertySelector = t => t.OutputSignal,
                Order = 11,
                IsReadOnly = IsReadOnly
            }
        };
    }

    private void InitializeCommands()
    {
        SaveCommand = new RelayCommand(Save, () => !IsReadOnly);
        CancelCommand = new RelayCommand(Cancel);
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

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

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
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Controller : EquipmentBase
{
    private string _controllerType;
    private string _firmwareVersion;

    public string ControllerType
    {
        get => _controllerType;
        set { _controllerType = value; OnPropertyChanged(); }
    }

    public string FirmwareVersion
    {
        get => _firmwareVersion;
        set { _firmwareVersion = value; OnPropertyChanged(); }
    }
}

public class MeasuringInstrument : EquipmentBase
{
    private string _measurementType;
    private double? _accuracy;
    private DateTime? _calibrationDate;

    public string MeasurementType
    {
        get => _measurementType;
        set { _measurementType = value; OnPropertyChanged(); }
    }

    public double? Accuracy
    {
        get => _accuracy;
        set { _accuracy = value; OnPropertyChanged(); }
    }

    public DateTime? CalibrationDate
    {
        get => _calibrationDate;
        set { _calibrationDate = value; OnPropertyChanged(); }
    }
}

public class Transducer : EquipmentBase
{
    private string _inputRange;
    private string _outputSignal;

    public string InputRange
    {
        get => _inputRange;
        set { _inputRange = value; OnPropertyChanged(); }
    }

    public string OutputSignal
    {
        get => _outputSignal;
        set { _outputSignal = value; OnPropertyChanged(); }
    }
}

// Simple RelayCommand for demo
//public class RelayCommand : ICommand
//{
//    private readonly Action _execute;
//    private readonly Func<bool> _canExecute;

//    public RelayCommand(Action execute, Func<bool> canExecute = null)
//    {
//        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
//        _canExecute = canExecute;
//    }

//    public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
//    public void Execute(object parameter) => _execute();

//    public event EventHandler CanExecuteChanged
//    {
//        add => CommandManager.RequerySuggested += value;
//        remove => CommandManager.RequerySuggested -= value;
//    }
//}