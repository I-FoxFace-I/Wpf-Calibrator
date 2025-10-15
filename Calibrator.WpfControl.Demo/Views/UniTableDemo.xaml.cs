using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Calibrator.WpfControl.Controls.UniTable;
using Calibrator.WpfControl.Controls.UniTable.Models;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfControl.Demo.Views;

public partial class UniTableDemo : UserControl
{
    private readonly UniTableDemoViewModel _viewModel;

    public UniTableDemo()
    {
        InitializeComponent();
        _viewModel = new UniTableDemoViewModel();
        DataContext = _viewModel;
    }

    private void OnFilterTextChanged(object sender, TextChangedEventArgs e)
    {
        var filterText = NameFilterTextBox.Text;
        
        if (string.IsNullOrWhiteSpace(filterText))
        {
            _viewModel.Filters = new List<UniTableFilter>();
        }
        else
        {
            _viewModel.Filters = new List<UniTableFilter>
            {
                new UniTableFilter
                {
                    ColumnName = "Name",
                    FilterText = filterText,
                    Type = FilterType.Contains
                }
            };
        }
    }

    private void ClearFilters(object sender, RoutedEventArgs e)
    {
        NameFilterTextBox.Text = string.Empty;
        _viewModel.Filters = new List<UniTableFilter>();
        _viewModel.Sort = null;
    }

    private void SortByNameAsc(object sender, RoutedEventArgs e)
    {
        _viewModel.Sort = new UniTableSort
        {
            ColumnName = "Name",
            Direction = SortDirection.Ascending
        };
    }

    private void SortByNameDesc(object sender, RoutedEventArgs e)
    {
        _viewModel.Sort = new UniTableSort
        {
            ColumnName = "Name",
            Direction = SortDirection.Descending
        };
    }

    private void SortByAgeAsc(object sender, RoutedEventArgs e)
    {
        _viewModel.Sort = new UniTableSort
        {
            ColumnName = "Age",
            Direction = SortDirection.Ascending
        };
    }

    private void SortByAgeDesc(object sender, RoutedEventArgs e)
    {
        _viewModel.Sort = new UniTableSort
        {
            ColumnName = "Age",
            Direction = SortDirection.Descending
        };
    }
}

public class UniTableDemoViewModel : INotifyPropertyChanged
{
    private List<UniTableFilter> _filters;
    private UniTableSort _sort;

    public UniTableDemoViewModel()
    {
        InitializeData();
    }

    public List<UniTableColumn> Columns { get; set; }
    public List<Person> People { get; set; }
    public List<UniTableBaseAction> TableOperations { get; set; }

    public List<UniTableFilter> Filters
    {
        get => _filters;
        set
        {
            _filters = value;
            OnPropertyChanged();
        }
    }

    public UniTableSort Sort
    {
        get => _sort;
        set
        {
            _sort = value;
            OnPropertyChanged();
        }
    }

    private void InitializeData()
    {
        // Sample data
        People = new List<Person>
        {
            new Person { Id = 1, Name = "John Doe", Age = 30, Email = "john@example.com", IsActive = true },
            new Person { Id = 2, Name = "Jane Smith", Age = 25, Email = "jane@example.com", IsActive = true },
            new Person { Id = 3, Name = "Bob Johnson", Age = 35, Email = "bob@example.com", IsActive = false },
            new Person { Id = 4, Name = "Alice Williams", Age = 28, Email = "alice@example.com", IsActive = true },
            new Person { Id = 5, Name = "Charlie Brown", Age = 42, Email = "charlie@example.com", IsActive = true },
            new Person { Id = 6, Name = "Diana Prince", Age = 31, Email = "diana@example.com", IsActive = false },
            new Person { Id = 7, Name = "Frank Miller", Age = 27, Email = "frank@example.com", IsActive = true },
            new Person { Id = 8, Name = "Grace Lee", Age = 33, Email = "grace@example.com", IsActive = true },
        };

        // Columns
        Columns = new List<UniTableColumn>
        {
            new UniTableRegularColumn<Person>
            {
                ColumnName = "ID",
                PropertySelector = p => p.Id,
                Width = 60
            },
            new UniTableRegularColumn<Person>
            {
                ColumnName = "Name",
                PropertySelector = p => p.Name,
                Width = 150
            },
            new UniTableRegularColumn<Person>
            {
                ColumnName = "Age",
                PropertySelector = p => p.Age,
                Width = 80
            },
            new UniTableRegularColumn<Person>
            {
                ColumnName = "Email",
                PropertySelector = p => p.Email,
                Width = 200
            },
            new UniTableRegularColumn<Person>
            {
                ColumnName = "Active",
                PropertySelector = p => p.IsActive,
                IsCheckBox = true,
                Width = 80
            }
        };

        // Operations
        TableOperations = new List<UniTableBaseAction>
        {
            new UniTableAction
            {
                Command = (item) => MessageBox.Show($"Edit: {((Person)item).Name}"),
                ToolTip = "Edit",
                IconKind = PackIconMaterialKind.Pencil
            },
            new UniTableAction
            {
                Command = (item) => MessageBox.Show($"Delete: {((Person)item).Name}"),
                ToolTip = "Delete",
                IconKind = PackIconMaterialKind.Delete
            }
        };

        Filters = new List<UniTableFilter>();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
}
