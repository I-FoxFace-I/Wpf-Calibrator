using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Calibrator.WpfControl.Controls.ScSmartTable;
using Calibrator.WpfControl.Controls.ScSmartTable.Models;
using Calibrator.WpfControl.Controls.UniTable;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfControl.Demo.Views;

public partial class SmartTableDemo : UserControl
{
    public SmartTableDemo()
    {
        InitializeComponent();
        DataContext = new SmartTableDemoViewModel(SmartTable);
    }
}

public class SmartTableDemoViewModel : INotifyPropertyChanged
{
    private ObservableCollection<PersonModel> _people = new();
    private readonly ScSmartTableComponent? _smartTableComponent;

    public SmartTableDemoViewModel(ScSmartTableComponent smartTableComponent)
    {
        _smartTableComponent = smartTableComponent;
        InitializeData();
        InitializeColumns();
        InitializeCommands();
    }

    public ObservableCollection<PersonModel> People
    {
        get => _people;
        set
        {
            _people = value;
            OnPropertyChanged();
        }
    }

    // Using UniTableColumn as base type allows mixing both:
    // - SmartTableRegularColumn<T> (enhanced with filtering)
    // - UniTableRegularColumn<T> (standard)
    public List<UniTableColumn> Columns { get; set; } = new();
    public List<UniTableBaseAction> TableOperations { get; set; } = new();

    public ICommand? AddPersonCommand { get; private set; }
    public ICommand? ClearFiltersCommand { get; private set; }
    public ICommand? RefreshCommand { get; private set; }

    private void InitializeData()
    {
        // Sample data with various data types for testing filters
        People = new ObservableCollection<PersonModel>
        {
            new PersonModel { Id = 1, Name = "John Doe", Age = 30, Email = "john.doe@example.com", City = "New York", IsActive = true, Salary = 75000, HireDate = new DateTime(2020, 3, 15) },
            new PersonModel { Id = 2, Name = "Jane Smith", Age = 25, Email = "jane.smith@example.com", City = "London", IsActive = true, Salary = 68000, HireDate = new DateTime(2021, 7, 22) },
            new PersonModel { Id = 3, Name = "Bob Johnson", Age = 35, Email = "bob.johnson@example.com", City = "Paris", IsActive = false, Salary = 82000, HireDate = new DateTime(2019, 1, 10) },
            new PersonModel { Id = 4, Name = "Alice Williams", Age = 28, Email = "alice.williams@example.com", City = "Tokyo", IsActive = true, Salary = 71000, HireDate = new DateTime(2020, 11, 5) },
            new PersonModel { Id = 5, Name = "Charlie Brown", Age = 42, Email = "charlie.brown@example.com", City = "Berlin", IsActive = true, Salary = 95000, HireDate = new DateTime(2018, 5, 18) },
            new PersonModel { Id = 6, Name = "Diana Prince", Age = 31, Email = "diana.prince@example.com", City = "Madrid", IsActive = false, Salary = 79000, HireDate = new DateTime(2019, 9, 30) },
            new PersonModel { Id = 7, Name = "Frank Miller", Age = 27, Email = "frank.miller@example.com", City = "Rome", IsActive = true, Salary = 65000, HireDate = new DateTime(2022, 2, 14) },
            new PersonModel { Id = 8, Name = "Grace Lee", Age = 33, Email = "grace.lee@example.com", City = "Seoul", IsActive = true, Salary = 88000, HireDate = new DateTime(2019, 6, 25) },
            new PersonModel { Id = 9, Name = "Henry Davis", Age = 29, Email = "henry.davis@example.com", City = "Sydney", IsActive = true, Salary = 73000, HireDate = new DateTime(2021, 4, 12) },
            new PersonModel { Id = 10, Name = "Ivy Wilson", Age = 26, Email = "ivy.wilson@example.com", City = "Toronto", IsActive = false, Salary = 67000, HireDate = new DateTime(2022, 8, 7) },
        };
    }

    private void InitializeColumns()
    {
        // Using UniTableColumn as base type allows mixing both column types
        // All columns below are SmartTableRegularColumn (enhanced filtering)
        Columns = new List<UniTableColumn>
        {
            // ID Column - No filtering, only sorting
            // Demonstrates: IsFilterable = false
            new SmartTableRegularColumn<PersonModel>
            {
                ColumnName = "ID",
                PropertySelector = p => p.Id,
                Width = 60,
                IsFilterable = false,  // IDs typically don't need filtering
                IsSortable = true,
                IsReadOnly = true,
                DataType = typeof(int)
            },
            
            // Name Column - Simplified text filtering
            // Demonstrates: Essential text operators only
            new SmartTableRegularColumn<PersonModel>
            {
                ColumnName = "Name",
                PropertySelector = p => p.Name,
                Width = 150,
                IsFilterable = true,
                IsSortable = true,
                AllowedFilterOperators = new List<FilterOperatorType>
                {
                    FilterOperatorType.Contains,     // Primary search
                    FilterOperatorType.IsEqualTo,    // Exact match
                    FilterOperatorType.IsEmpty,      // Empty check
                    FilterOperatorType.IsNotEmpty    // Not empty check
                },
                DefaultFilterOperator = FilterOperatorType.Contains,
                DataType = typeof(string)
            },
            
            // Age Column - Simplified numeric filtering
            // Demonstrates: 3 operators cover all numeric cases
            new SmartTableRegularColumn<PersonModel>
            {
                ColumnName = "Age",
                PropertySelector = p => p.Age,
                Width = 80,
                IsFilterable = true,
                IsSortable = true,
                AllowedFilterOperators = new List<FilterOperatorType>
                {
                    FilterOperatorType.Equals,          // Age = 30
                    FilterOperatorType.GreaterOrEqual,  // Age ≥ 30
                    FilterOperatorType.LessOrEqual      // Age ≤ 30
                },
                DefaultFilterOperator = FilterOperatorType.Equals,
                DataType = typeof(int)
            },
            
            // Email Column - Simplified text filtering
            // Demonstrates: Essential text operators for email
            new SmartTableRegularColumn<PersonModel>
            {
                ColumnName = "Email",
                PropertySelector = p => p.Email,
                Width = 200,
                IsFilterable = true,
                IsSortable = true,
                AllowedFilterOperators = new List<FilterOperatorType>
                {
                    FilterOperatorType.Contains,     // Primary search (domain, name part)
                    FilterOperatorType.IsEqualTo     // Exact email match
                },
                DefaultFilterOperator = FilterOperatorType.Contains,
                DataType = typeof(string)
            },
            
            // City Column - Simplified text filtering + groupable
            // Demonstrates: IsGroupable = true for categorical data
            new SmartTableRegularColumn<PersonModel>
            {
                ColumnName = "City",
                PropertySelector = p => p.City,
                Width = 120,
                IsFilterable = true,
                IsSortable = true,
                IsGroupable = true,  // Cities are good for grouping
                AllowedFilterOperators = new List<FilterOperatorType>
                {
                    FilterOperatorType.IsEqualTo,    // Exact city match (primary for cities)
                    FilterOperatorType.Contains      // Partial city search
                },
                DefaultFilterOperator = FilterOperatorType.IsEqualTo,
                DataType = typeof(string)
            },
            
            // Salary Column - Simplified numeric filtering
            // Demonstrates: 3 operators for financial data (covers all ranges)
            new SmartTableRegularColumn<PersonModel>
            {
                ColumnName = "Salary",
                PropertySelector = p => p.Salary,
                Width = 100,
                IsFilterable = true,
                IsSortable = true,
                AllowedFilterOperators = new List<FilterOperatorType>
                {
                    FilterOperatorType.Equals,          // Exact salary
                    FilterOperatorType.GreaterOrEqual,  // Salary ≥ 75000 (min salary)
                    FilterOperatorType.LessOrEqual      // Salary ≤ 85000 (max salary)
                },
                DefaultFilterOperator = FilterOperatorType.GreaterOrEqual,
                DataType = typeof(decimal)
            },
            
            // Hire Date Column - Simplified date filtering
            // Demonstrates: 3 operators for date ranges (same as numeric)
            new SmartTableRegularColumn<PersonModel>
            {
                ColumnName = "Hire Date",
                PropertySelector = p => p.HireDate,
                Width = 120,
                IsFilterable = true,
                IsSortable = true,
                AllowedFilterOperators = new List<FilterOperatorType>
                {
                    FilterOperatorType.Equals,          // Exact date match
                    FilterOperatorType.GreaterOrEqual,  // Date ≥ 2020-01-01 (hired after)
                    FilterOperatorType.LessOrEqual      // Date ≤ 2021-12-31 (hired before)
                },
                DefaultFilterOperator = FilterOperatorType.GreaterOrEqual,
                DataType = typeof(DateTime)
            },
            
            // Active Column - Boolean filtering
            // Demonstrates: Boolean operators (IsTrue/IsFalse)
            new SmartTableRegularColumn<PersonModel>
            {
                ColumnName = "Active",
                PropertySelector = p => p.IsActive,
                Width = 80,
                IsCheckBox = true,
                IsFilterable = true,
                IsSortable = true,
                AllowedFilterOperators = new List<FilterOperatorType>
                {
                    FilterOperatorType.IsTrue,
                    FilterOperatorType.IsFalse
                },
                DefaultFilterOperator = FilterOperatorType.IsTrue,
                DataType = typeof(bool)
            }
        };
    }

    private void InitializeCommands()
    {
        // Define table operations (action buttons for each row)
        TableOperations = new List<UniTableBaseAction>
        {
            new UniTableAction
            {
                ToolTip = "Detail",
                Command = entity => ViewDetails((PersonModel)entity),
                IconKind = PackIconMaterialKind.Eye
            },
            new UniTableAction
            {
                ToolTip = "Edit",
                Command = entity => EditPerson((PersonModel)entity),
                IconKind = PackIconMaterialKind.Pencil
            },
            new UniTableAction
            {
                ToolTip = "Delete",
                Command = entity => DeletePerson((PersonModel)entity),
                IconKind = PackIconMaterialKind.Delete
            },
            
        };

        AddPersonCommand = new RelayCommand(AddPerson);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        RefreshCommand = new RelayCommand(RefreshData);
    }

    private void EditPerson(PersonModel person)
    {
        MessageBox.Show($"Editing: {person.Name}", "Edit Person", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void DeletePerson(PersonModel person)
    {
        var result = MessageBox.Show(
            $"Are you sure you want to delete {person.Name}?",
            "Delete Person",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            People.Remove(person);
        }
    }

    private void ViewDetails(PersonModel person)
    {
        var details = $"Name: {person.Name}\n" +
                     $"Age: {person.Age}\n" +
                     $"Email: {person.Email}\n" +
                     $"City: {person.City}\n" +
                     $"Salary: ${person.Salary:N0}\n" +
                     $"Hire Date: {person.HireDate:d}\n" +
                     $"Active: {person.IsActive}";

        MessageBox.Show(details, "Person Details", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void AddPerson()
    {
        var newId = People.Any() ? People.Max(p => p.Id) + 1 : 1;
        var newPerson = new PersonModel
        {
            Id = newId,
            Name = $"New Person {newId}",
            Age = 25,
            Email = $"person{newId}@example.com",
            City = "Prague",
            IsActive = true,
            Salary = 70000,
            HireDate = DateTime.Now
        };

        People.Add(newPerson);
    }

    private void ClearFilters()
    {
        // Clear all Telerik filter descriptors
        _smartTableComponent?.ClearFilters();
    }

    private void RefreshData()
    {
        // Simulate data refresh by recreating the collection
        var tempData = People.ToList();
        People.Clear();
        foreach (var person in tempData)
        {
            People.Add(person);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Model for demo purposes
public class PersonModel : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    private int _age;
    private string _email = string.Empty;
    private string _city = string.Empty;
    private bool _isActive;
    private decimal _salary;
    private DateTime _hireDate;

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

    public int Age
    {
        get => _age;
        set { _age = value; OnPropertyChanged(); }
    }

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    public string City
    {
        get => _city;
        set { _city = value; OnPropertyChanged(); }
    }

    public bool IsActive
    {
        get => _isActive;
        set { _isActive = value; OnPropertyChanged(); }
    }

    public decimal Salary
    {
        get => _salary;
        set { _salary = value; OnPropertyChanged(); }
    }

    public DateTime HireDate
    {
        get => _hireDate;
        set { _hireDate = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Simple RelayCommand implementation for demo
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();
}