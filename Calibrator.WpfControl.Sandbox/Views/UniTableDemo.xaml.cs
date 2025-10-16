using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Calibrator.WpfControl.Controls.UniTable;
using Calibrator.WpfControl.Controls.UniTable.Models;
using Calibrator.WpfControl.Sandbox.ViewModels;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfControl.Sandbox.Views;

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
