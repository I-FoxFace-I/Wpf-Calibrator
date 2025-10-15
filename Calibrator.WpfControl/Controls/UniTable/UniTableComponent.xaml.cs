using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Calibrator.WpfControl.Controls.UniTable.Models;
using Calibrator.WpfControl.Resources;
using CommunityToolkit.Mvvm.Input;
using Telerik.Windows.Controls;

namespace Calibrator.WpfControl.Controls.UniTable;

public partial class UniTableComponent
{
    private const string TableOperationsColumnName = "Operations";
    private object _originalItemsSource;

    public AsyncRelayCommand<object> OnExecuteActionCommand { get; private set; }

    public UniTableComponent()
    {
        InitializeComponent();
        OnExecuteActionCommand = new(OnExecuteAction);
    }

    public object Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public object ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public List<UniTableBaseAction> TableOperations
    {
        get => (List<UniTableBaseAction>)GetValue(TableOperationsProperty);
        set => SetValue(TableOperationsProperty, value);
    }

    public bool EnableFiltering
    {
        get => (bool)GetValue(EnableFilteringProperty);
        set => SetValue(EnableFilteringProperty, value);
    }

    public bool EnableSorting
    {
        get => (bool)GetValue(EnableSortingProperty);
        set => SetValue(EnableSortingProperty, value);
    }

    public List<UniTableFilter> Filters
    {
        get => (List<UniTableFilter>)GetValue(FiltersProperty);
        set => SetValue(FiltersProperty, value);
    }

    public UniTableSort Sort
    {
        get => (UniTableSort)GetValue(SortProperty);
        set => SetValue(SortProperty, value);
    }

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(object),
            typeof(UniTableComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnColumnsChanged));

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object),
            typeof(UniTableComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnItemsSourceChanged));

    public static readonly DependencyProperty TableOperationsProperty =
        DependencyProperty.Register(nameof(TableOperations), typeof(List<UniTableBaseAction>),
            typeof(UniTableComponent),
            new FrameworkPropertyMetadata(new List<UniTableBaseAction>(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTableOperationsChange));

    public static readonly DependencyProperty EnableFilteringProperty =
        DependencyProperty.Register(nameof(EnableFiltering), typeof(bool),
            typeof(UniTableComponent),
            new FrameworkPropertyMetadata(false, OnFilteringChanged));

    public static readonly DependencyProperty EnableSortingProperty =
        DependencyProperty.Register(nameof(EnableSorting), typeof(bool),
            typeof(UniTableComponent),
            new FrameworkPropertyMetadata(false, OnSortingChanged));

    public static readonly DependencyProperty FiltersProperty =
        DependencyProperty.Register(nameof(Filters), typeof(List<UniTableFilter>),
            typeof(UniTableComponent),
            new FrameworkPropertyMetadata(new List<UniTableFilter>(), OnFiltersChanged));

    public static readonly DependencyProperty SortProperty =
        DependencyProperty.Register(nameof(Sort), typeof(UniTableSort),
            typeof(UniTableComponent),
            new FrameworkPropertyMetadata(null, OnSortChanged));

    private async Task OnExecuteAction(object? parameter)
    {
        var (baseAction, entity) =
            parameter as Tuple<UniTableBaseAction, object>
            ?? throw new InvalidOperationException("Wrong table action provided");

        switch (baseAction)
        {
            case UniTableAction action:
                action.Command.Invoke(entity);
                break;
            case UniTableAsyncAction asyncAction:
                await asyncAction.Command(entity);
                break;
        }
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UniTableComponent uniTable)
        {
            uniTable._originalItemsSource = e.NewValue;
            uniTable.ApplyFiltersAndSorting();
        }
    }

    private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UniTableComponent uniTable)
            return;

        if (e.NewValue is not IEnumerable<UniTableColumn> newColumns)
            return;

        uniTable.GridView.Columns.Clear();

        foreach (var column in newColumns)
        {
            switch (column)
            {
                case UniTableRegularColumn regularColumn:
                    uniTable.GridView.Columns.Add(RegularToGridColumn(regularColumn));
                    break;
            }
        }

        uniTable.AssertTableOperationsColumn();
    }

    private static void OnTableOperationsChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UniTableComponent uniTable)
            return;

        if (e.NewValue is not IEnumerable<UniTableBaseAction>)
            return;

        uniTable.AssertTableOperationsColumn();
    }

    private static void OnFilteringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UniTableComponent uniTable)
        {
            uniTable.ApplyFiltersAndSorting();
        }
    }

    private static void OnSortingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UniTableComponent uniTable)
        {
            uniTable.ApplyFiltersAndSorting();
        }
    }

    private static void OnFiltersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UniTableComponent uniTable)
        {
            uniTable.ApplyFiltersAndSorting();
        }
    }

    private static void OnSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UniTableComponent uniTable)
        {
            uniTable.ApplyFiltersAndSorting();
        }
    }

    private void ApplyFiltersAndSorting()
    {
        if (_originalItemsSource == null)
            return;

        var source = (_originalItemsSource as IEnumerable)?.Cast<object>().ToList();
        if (source == null)
            return;

        // Apply filters
        if (EnableFiltering && Filters != null && Filters.Any())
        {
            source = ApplyFilters(source);
        }

        // Apply sorting
        if (EnableSorting && Sort != null)
        {
            source = ApplySorting(source);
        }

        GridView.ItemsSource = source;
    }

    private List<object> ApplyFilters(List<object> source)
    {
        var filtered = source;

        foreach (var filter in Filters)
        {
            if (string.IsNullOrWhiteSpace(filter.FilterText))
                continue;

            filtered = filtered.Where(item =>
            {
                var property = item.GetType().GetProperty(filter.ColumnName);
                if (property == null) return true;

                var value = property.GetValue(item)?.ToString() ?? string.Empty;

                return filter.Type switch
                {
                    FilterType.Contains => value.Contains(filter.FilterText, StringComparison.OrdinalIgnoreCase),
                    FilterType.Equals => value.Equals(filter.FilterText, StringComparison.OrdinalIgnoreCase),
                    FilterType.NotEquals => !value.Equals(filter.FilterText, StringComparison.OrdinalIgnoreCase),
                    FilterType.StartsWith => value.StartsWith(filter.FilterText, StringComparison.OrdinalIgnoreCase),
                    FilterType.EndsWith => value.EndsWith(filter.FilterText, StringComparison.OrdinalIgnoreCase),
                    FilterType.GreaterThan => CompareNumeric(value, filter.FilterText) > 0,
                    FilterType.LessThan => CompareNumeric(value, filter.FilterText) < 0,
                    _ => true
                };
            }).ToList();
        }

        return filtered;
    }

    private List<object> ApplySorting(List<object> source)
    {
        if (Sort == null || string.IsNullOrWhiteSpace(Sort.ColumnName))
            return source;

        var propertyInfo = source.FirstOrDefault()?.GetType().GetProperty(Sort.ColumnName);
        if (propertyInfo == null)
            return source;

        return Sort.Direction == SortDirection.Ascending
            ? source.OrderBy(item => propertyInfo.GetValue(item)).ToList()
            : source.OrderByDescending(item => propertyInfo.GetValue(item)).ToList();
    }

    private int CompareNumeric(string value1, string value2)
    {
        if (double.TryParse(value1, out double num1) && double.TryParse(value2, out double num2))
        {
            return num1.CompareTo(num2);
        }
        return 0;
    }

    private void AssertTableOperationsColumn()
    {
        if (!TableOperations.Any())
            return;

        if (GridView.Columns.Count == 0)
            return;

        var gridColumns = GridView.Columns.Cast<Telerik.Windows.Controls.GridViewColumn>().ToList();

        if (gridColumns.Any(a => a.Name == TableOperationsColumnName))
            return;

        var operationsColumn = new GridViewDataColumn
        {
            Header = TableOperationsColumnName,
            CellTemplate = (DataTemplate)Resources[ResourceKeys.UniTableOperationsTemplate],
            MinWidth = 80
        };

        GridView.Columns.Add(operationsColumn);
    }

    private static Telerik.Windows.Controls.GridViewColumn RegularToGridColumn(UniTableRegularColumn column)
    {
        if (column.IsCheckBox)
        {
            return ToCheckBoxColumn(column);
        }

        return ToDataGridColumn(column);
    }

    private static GridViewDataColumn ToDataGridColumn(UniTableRegularColumn column)
    {
        return new()
        {
            Header = column.ColumnName,
            DataMemberBinding = new Binding(column.GetPropertyName()),
            MinWidth = column.Width,
            IsReadOnly = column.IsReadOnly,
        };
    }

    private static GridViewCheckBoxColumn ToCheckBoxColumn(UniTableRegularColumn column)
    {
        return new()
        {
            Header = column.ColumnName,
            DataMemberBinding = new Binding(column.GetPropertyName()),
            MinWidth = column.Width,
            CellStyle = (Style)Application.Current.Resources[ResourceKeys.GridViewCellStyle]
        };
    }
}