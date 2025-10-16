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

/// <summary>
/// A table component with support for custom columns, row actions, filtering and sorting
/// </summary>
public partial class UniTableComponent
{
    private const string TableOperationsColumnName = "Operations";
    private object? _originalItemsSource;

    /// <summary>
    /// Gets the command for executing table actions
    /// </summary>
    public AsyncRelayCommand<object> OnExecuteActionCommand { get; private set; }

    /// <summary>
    /// Initializes a new instance of the UniTableComponent class
    /// </summary>
    public UniTableComponent()
    {
        InitializeComponent();
        OnExecuteActionCommand = new(OnExecuteAction);
    }

    /// <summary>
    /// Gets or sets the collection of column definitions for the table
    /// </summary>
    public object Columns
    {
        get => GetValue(ColumnsProperty);
        set => this.SetValue(ColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets the data source for the table
    /// </summary>
    public object ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => this.SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of row actions available in the table
    /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
    public ICollection<UniTableBaseAction> TableOperations
    {
        get => (ICollection<UniTableBaseAction>)this.GetValue(TableOperationsProperty);
        set => this.SetValue(TableOperationsProperty, value);
    }
#pragma warning restore CA2227

    /// <summary>
    /// Gets or sets whether filtering is enabled for the table
    /// </summary>
    public bool EnableFiltering
    {
        get => (bool)this.GetValue(EnableFilteringProperty);
        set => this.SetValue(EnableFilteringProperty, value);
    }

    /// <summary>
    /// Gets or sets whether sorting is enabled for the table
    /// </summary>
    public bool EnableSorting
    {
        get => (bool)this.GetValue(EnableSortingProperty);
        set => this.SetValue(EnableSortingProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of active filters
    /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
    public ICollection<UniTableFilter> Filters
    {
        get => (ICollection<UniTableFilter>)this.GetValue(FiltersProperty);
        set => this.SetValue(FiltersProperty, value);
    }
#pragma warning restore CA2227

    /// <summary>
    /// Gets or sets the current sort configuration
    /// </summary>
    public UniTableSort Sort
    {
        get => (UniTableSort)this.GetValue(SortProperty);
        set => this.SetValue(SortProperty, value);
    }

    /// <summary>
    /// Identifies the Columns dependency property
    /// </summary>
    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(object),
            typeof(UniTableComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnColumnsChanged));

    /// <summary>
    /// Identifies the ItemsSource dependency property
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object),
            typeof(UniTableComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnItemsSourceChanged));

    /// <summary>
    /// Identifies the TableOperations dependency property
    /// </summary>
    public static readonly DependencyProperty TableOperationsProperty =
        DependencyProperty.Register(nameof(TableOperations), typeof(ICollection<UniTableBaseAction>),
            typeof(UniTableComponent),
            new FrameworkPropertyMetadata(new List<UniTableBaseAction>(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTableOperationsChange));

    /// <summary>
    /// Identifies the EnableFiltering dependency property
    /// </summary>
    public static readonly DependencyProperty EnableFilteringProperty =
        DependencyProperty.Register(nameof(EnableFiltering), typeof(bool),
            typeof(UniTableComponent),
            new FrameworkPropertyMetadata(false, OnFilteringChanged));

    /// <summary>
    /// Identifies the EnableSorting dependency property
    /// </summary>
    public static readonly DependencyProperty EnableSortingProperty =
        DependencyProperty.Register(nameof(EnableSorting), typeof(bool),
            typeof(UniTableComponent),
            new FrameworkPropertyMetadata(false, OnSortingChanged));

    /// <summary>
    /// Identifies the Filters dependency property
    /// </summary>
    public static readonly DependencyProperty FiltersProperty =
        DependencyProperty.Register(nameof(Filters), typeof(ICollection<UniTableFilter>),
            typeof(UniTableComponent),
            new FrameworkPropertyMetadata(new List<UniTableFilter>(), OnFiltersChanged));

    /// <summary>
    /// Identifies the Sort dependency property
    /// </summary>
    public static readonly DependencyProperty SortProperty =
        DependencyProperty.Register(nameof(Sort), typeof(UniTableSort),
            typeof(UniTableComponent),
            new FrameworkPropertyMetadata(null, OnSortChanged));

    private static async Task OnExecuteAction(object? parameter)
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
                await asyncAction.Command(entity).ConfigureAwait(false);
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
        if (this.EnableFiltering && this.Filters != null && this.Filters.Count > 0)
        {
            source = this.ApplyFilters(source);
        }

        // Apply sorting
        if (this.EnableSorting && this.Sort != null)
        {
            source = this.ApplySorting(source);
        }

        this.GridView.ItemsSource = source;
    }

    private List<object> ApplyFilters(List<object> source)
    {
        var filtered = source;

        foreach (var filter in this.Filters)
        {
            if (string.IsNullOrWhiteSpace(filter.FilterText))
            {
                continue;
            }

            filtered = filtered.Where(item =>
            {
                var property = item.GetType().GetProperty(filter.ColumnName);
                if (property == null)
                {
                    return true;
                }

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

        return this.Sort.Direction == SortDirection.Ascending
            ? source.OrderBy(item => propertyInfo.GetValue(item)).ToList()
            : source.OrderByDescending(item => propertyInfo.GetValue(item)).ToList();
    }

    private static int CompareNumeric(string value1, string value2)
    {
        if (double.TryParse(value1, out double num1) && double.TryParse(value2, out double num2))
        {
            return num1.CompareTo(num2);
        }
        return 0;
    }

    private void AssertTableOperationsColumn()
    {
        if (this.TableOperations.Count == 0)
        {
            return;
        }

        if (this.GridView.Columns.Count == 0)
        {
            return;
        }

        var gridColumns = this.GridView.Columns.Cast<Telerik.Windows.Controls.GridViewColumn>().ToList();

        if (gridColumns.Any(a => a.Name == TableOperationsColumnName))
        {
            return;
        }

        var operationsColumn = new GridViewDataColumn
        {
            Header = TableOperationsColumnName,
            CellTemplate = (DataTemplate)this.Resources[ResourceKeys.UniTableOperationsTemplate],
            MinWidth = 80,
        };

        this.GridView.Columns.Add(operationsColumn);
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