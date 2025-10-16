using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Calibrator.WpfControl.Controls.UniTable;
using Calibrator.WpfControl.Sandbox.Commands;
using Calibrator.WpfControl.Sandbox.Models;
using Calibrator.WpfControl.Sandbox.Views;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfControl.Sandbox.ViewModels;

// ============================================
// Main ViewModel
// ============================================

/// <summary>
/// 
/// </summary>
public class UniFormDemoViewModel : INotifyPropertyChanged
{
    private ObservableCollection<EquipmentBase> _equipment = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UniFormDemoViewModel"/> class.
    /// </summary>
    public UniFormDemoViewModel()
    {
        this.InitializeData();
        this.InitializeCommands();
    }

    /// <summary>
    /// Property changed event handler.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets list of equipements.
    /// </summary>
    public ObservableCollection<EquipmentBase> Equipment
    {
        get => this._equipment;
        set
        {
            this._equipment = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets list of table columns.
    /// </summary>
    public List<UniTableColumn> Columns { get; set; } = new();

    /// <summary>
    /// Gets or sets list of table opperations.
    /// </summary>
    public List<UniTableBaseAction> TableOperations { get; set; } = new();

    /// <summary>
    /// Gets open dialog command.
    /// </summary>
    public ICommand OpenAddDialogCommand { get; private set; }

    /// <summary>
    /// Gets refresh command.
    /// </summary>
    public ICommand RefreshCommand { get; private set; }

    /// <summary>
    /// Property changed event delegate.
    /// </summary>
    /// <param name="propertyName">Name of changed property</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void InitializeData()
    {
        // Sample equipment data
        this.Equipment = new ObservableCollection<EquipmentBase>
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
                FirmwareVersion = "v2.1.5",
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
                CalibrationDate = DateTime.Now.AddMonths(-6),
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
                OutputSignal = "4-20mA",
            },
        };

        // Define columns
        this.Columns = new List<UniTableColumn>
        {
            new UniTableRegularColumn<EquipmentBase>
            {
                ColumnName = "ID",
                PropertySelector = e => e.Id,
                Width = 60,
            },
            new UniTableRegularColumn<EquipmentBase>
            {
                ColumnName = "Type",
                PropertySelector = e => e.GetType().Name,
                Width = 150,
            },
            new UniTableRegularColumn<EquipmentBase>
            {
                ColumnName = "Name",
                PropertySelector = e => e.Name,
                Width = 200,
            },
            new UniTableRegularColumn<EquipmentBase>
            {
                ColumnName = "Serial Number",
                PropertySelector = e => e.SerialNumber,
                Width = 150,
            },
            new UniTableRegularColumn<EquipmentBase>
            {
                ColumnName = "Manufacturer",
                PropertySelector = e => e.Manufacturer,
                Width = 150,
            },
            new UniTableRegularColumn<EquipmentBase>
            {
                ColumnName = "Active",
                PropertySelector = e => e.IsActive,
                IsCheckBox = true,
                Width = 80,
            },
        };

        // Define operations
        this.TableOperations = new List<UniTableBaseAction>
        {
            new UniTableAction
            {
                Command = this.OpenEditDialog,
                ToolTip = "Edit",
                IconKind = PackIconMaterialKind.Pencil,
            },
            new UniTableAction
            {
                Command = this.OpenViewDialog,
                ToolTip = "View Details",
                IconKind = PackIconMaterialKind.Eye,
            },
            new UniTableAction
            {
                Command = this.DeleteEquipment,
                ToolTip = "Delete",
                IconKind = PackIconMaterialKind.Delete,
            },
        };
    }

    private void InitializeCommands()
    {
        this.RefreshCommand = new RelayCommand(this.Refresh);
        this.OpenAddDialogCommand = new RelayCommand(this.OpenAddDialog);
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
            EquipmentBase? newEquipment = typeViewModel.SelectedType switch
            {
                "Controller" => new Controller(),
                "Measuring Instrument" => new MeasuringInstrument(),
                "Transducer" => new Transducer(),
                _ => null,
            };

            if (newEquipment != null)
            {
                // Open edit dialog for new equipment
                var dialog = new EquipmentDialog();
                var viewModel = new EquipmentDialogViewModel(newEquipment, true);
                dialog.DataContext = viewModel;

                if (dialog.ShowDialog() == true)
                {
                    newEquipment.Id = this.Equipment.Max(e => e.Id) + 1;
                    this.Equipment.Add(newEquipment);
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
                this.Equipment.Remove(equipment);
            }
        }
    }

    private void Refresh()
    {
        // Reload data (in real app, would fetch from database)
        this.OnPropertyChanged(nameof(this.Equipment));
    }
}