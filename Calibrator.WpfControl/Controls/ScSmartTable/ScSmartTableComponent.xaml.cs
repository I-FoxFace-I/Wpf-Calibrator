using Calibrator.WpfControl.Controls.ScSmartTable.Models;
using Calibrator.WpfControl.Controls.UniTable;
using Calibrator.WpfControl.Resources;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Data;

namespace Calibrator.WpfControl.Controls.ScSmartTable;

/// <summary>
/// Smart table with built-in Telerik filtering, sorting, and grouping capabilities
/// Improved version with FilterRow mode and per-column filter configuration
/// Supports both UniTableRegularColumn (standard) and SmartTableRegularColumn (enhanced) 
/// SmartTableRegularColumn extends UniTableRegularColumn with additional filtering properties
/// </summary>
public partial class ScSmartTableComponent : UserControl
{
    private const string TableOperationsColumnName = "Operations";

    public AsyncRelayCommand<object> OnExecuteActionCommand { get; private set; }

    public ScSmartTableComponent()
    {
        InitializeComponent();
        OnExecuteActionCommand = new(OnExecuteAction);

        // Set default FilteringMode to FilterRow for cleaner UI
        GridView.FilteringMode = Telerik.Windows.Controls.GridView.FilteringMode.FilterRow;

        // Subscribe to events
        GridView.Sorted += OnGridSorted;
        GridView.Filtered += OnGridFiltered;

        // Apply initial settings
        ApplyFilteringMode();
    }

    #region Dependency Properties

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

    public bool EnableGrouping
    {
        get => (bool)GetValue(EnableGroupingProperty);
        set => SetValue(EnableGroupingProperty, value);
    }

    public bool ShowSearchPanel
    {
        get => (bool)GetValue(ShowSearchPanelProperty);
        set => SetValue(ShowSearchPanelProperty, value);
    }

    public bool EnableColumnReordering
    {
        get => (bool)GetValue(EnableColumnReorderingProperty);
        set => SetValue(EnableColumnReorderingProperty, value);
    }

    public bool EnableColumnFreezing
    {
        get => (bool)GetValue(EnableColumnFreezingProperty);
        set => SetValue(EnableColumnFreezingProperty, value);
    }

    public bool ShowColumnFooters
    {
        get => (bool)GetValue(ShowColumnFootersProperty);
        set => SetValue(ShowColumnFootersProperty, value);
    }

    public Telerik.Windows.Controls.GridView.FilteringMode FilteringMode
    {
        get => (Telerik.Windows.Controls.GridView.FilteringMode)GetValue(FilteringModeProperty);
        set => SetValue(FilteringModeProperty, value);
    }

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(object),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnColumnsChanged));

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnItemsSourceChanged));

    public static readonly DependencyProperty TableOperationsProperty =
        DependencyProperty.Register(nameof(TableOperations), typeof(List<UniTableBaseAction>),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(new List<UniTableBaseAction>(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTableOperationsChange));

    public static readonly DependencyProperty EnableFilteringProperty =
        DependencyProperty.Register(nameof(EnableFiltering), typeof(bool),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty EnableSortingProperty =
        DependencyProperty.Register(nameof(EnableSorting), typeof(bool),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty EnableGroupingProperty =
        DependencyProperty.Register(nameof(EnableGrouping), typeof(bool),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty ShowSearchPanelProperty =
        DependencyProperty.Register(nameof(ShowSearchPanel), typeof(bool),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty EnableColumnReorderingProperty =
        DependencyProperty.Register(nameof(EnableColumnReordering), typeof(bool),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty EnableColumnFreezingProperty =
        DependencyProperty.Register(nameof(EnableColumnFreezing), typeof(bool),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty ShowColumnFootersProperty =
        DependencyProperty.Register(nameof(ShowColumnFooters), typeof(bool),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty FilteringModeProperty =
        DependencyProperty.Register(nameof(FilteringMode), typeof(Telerik.Windows.Controls.GridView.FilteringMode),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(Telerik.Windows.Controls.GridView.FilteringMode.FilterRow, OnFilteringModeChanged));

    #endregion

    public void ClearFilters()
    {
        GridView.FilterDescriptors.Clear();
    }

    #region Event Handlers

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

    private void OnGridSorted(object sender, GridViewSortedEventArgs e)
    {
        // Event for external handling if needed
    }

    private void OnGridFiltered(object sender, GridViewFilteredEventArgs e)
    {
        // Event for external handling if needed
    }

    #endregion

    #region Property Changed Handlers

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScSmartTableComponent smartTable)
        {
            smartTable.GridView.ItemsSource = e.NewValue;
        }
    }

    private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScSmartTableComponent smartTable)
            return;

        if (e.NewValue is not IEnumerable<UniTableColumn> newColumns)
            return;

        smartTable.GridView.Columns.Clear();

        foreach (var column in newColumns)
        {
            switch (column)
            {
                case SmartTableRegularColumn smartColumn:
                    // SmartTableRegularColumn with enhanced filtering (derives from UniTableRegularColumn)
                    var smartGridColumn = RegularToGridColumn(smartColumn, smartTable.EnableFiltering);
                    ConfigureSmartColumnCapabilities(smartGridColumn, smartColumn, smartTable);

                    if (smartColumn.IsFilterable && smartTable.EnableFiltering)
                    {
                        ConfigureColumnFiltering(smartGridColumn, smartColumn);
                    }

                    smartTable.GridView.Columns.Add(smartGridColumn);
                    break;

                case UniTableRegularColumn regularColumn:
                    // Standard UniTableRegularColumn - basic support without enhanced filtering
                    var gridColumn = RegularToGridColumn(regularColumn, smartTable.EnableFiltering);
                    smartTable.GridView.Columns.Add(gridColumn);
                    break;
            }
        }

        smartTable.AssertTableOperationsColumn();
    }

    private static void OnTableOperationsChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScSmartTableComponent smartTable)
            return;

        if (e.NewValue is not IEnumerable<UniTableBaseAction>)
            return;

        smartTable.AssertTableOperationsColumn();
    }

    private static void OnFilteringModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScSmartTableComponent smartTable)
        {
            smartTable.ApplyFilteringMode();
        }
    }

    private void ApplyFilteringMode()
    {
        GridView.FilteringMode = FilteringMode;
    }

    #endregion

    #region Column Configuration

    private static void ConfigureSmartColumnCapabilities(
        Telerik.Windows.Controls.GridViewColumn gridColumn,
        SmartTableRegularColumn column,
        ScSmartTableComponent smartTable)
    {
        // Set individual column capabilities
        gridColumn.IsFilterable = column.IsFilterable && smartTable.EnableFiltering;
        gridColumn.IsSortable = column.IsSortable && smartTable.EnableSorting;
        gridColumn.IsGroupable = column.IsGroupable && smartTable.EnableGrouping;
    }

    private static void ConfigureColumnFiltering(Telerik.Windows.Controls.GridViewColumn gridColumn, SmartTableRegularColumn column)
    {
        // Determine which filter operators to use
        List<FilterOperatorType> allowedOperators;

        if (column.AllowedFilterOperators != null && column.AllowedFilterOperators.Any())
        {
            // Use explicitly defined operators
            allowedOperators = column.AllowedFilterOperators;
        }
        else
        {
            // Auto-detect based on column data type
            Type dataType = GetColumnDataType(column);
            allowedOperators = FilterOperatorHelper.GetDefaultForType(dataType);
        }

        // Convert to Telerik operators and remove duplicates
        var distinctOperators = new List<FilterOperator>();
        foreach (var op in allowedOperators)
        {
            var telerikOp = FilterOperatorHelper.ToTelerikOperator(op);
            if (!distinctOperators.Contains(telerikOp))
            {
                distinctOperators.Add(telerikOp);
            }
        }

        // Create column filter descriptor with available operators
        if (distinctOperators.Any())
        {
            // Set the first operator as default if not specified
            var defaultOperator = column.DefaultFilterOperator ?? allowedOperators.FirstOrDefault();
            var telerikDefaultOp = FilterOperatorHelper.ToTelerikOperator(defaultOperator);

            // Note: Telerik's GridViewDataColumn uses ColumnFilterDescriptor internally
            // We set the filter operators through the column's FilterMemberPath
            // The actual filtering UI will show these operators

            // Create a filter descriptor template
            // new Telerik.Windows.Controls.GridView.ColumnFilterDescriptor(gridColumn)
            //{
            //    FilterOperator1 = telerikDefaultOp
            //};
            gridColumn.ColumnFilterDescriptor.FieldFilter.Filter1.Operator = telerikDefaultOp;
            // The available operators will be shown in the dropdown
            // Telerik automatically provides the UI based on data type and these settings
        }
    }

    private static Type GetColumnDataType(SmartTableRegularColumn column)
    {
        // Try to get data type from column
        if (column.DataType != null)
            return column.DataType;

        // Try to infer from property selector using reflection
        // This is a best-effort approach since we can't easily get the type at runtime
        // from the expression without executing it

        // Default to string if we can't determine
        return typeof(string);
    }

    private void AssertTableOperationsColumn()
    {
        if (!TableOperations.Any())
            return;

        if (GridView.Columns.Count == 0)
            return;

        var gridColumns = GridView.Columns.Cast<Telerik.Windows.Controls.GridViewColumn>().ToList();

        if (gridColumns.Any(a => a.UniqueName == TableOperationsColumnName))
            return;

        var operationsColumn = new GridViewDataColumn
        {
            Header = TableOperationsColumnName,
            UniqueName = TableOperationsColumnName,
            CellTemplate = (DataTemplate)Resources["SmartTableOperationsTemplate"],
            MinWidth = 80,
            IsFilterable = false,
            IsSortable = false,
            IsGroupable = false
        };

        GridView.Columns.Add(operationsColumn);
    }

    private static Telerik.Windows.Controls.GridViewColumn RegularToGridColumn(UniTableRegularColumn column, bool enableFiltering)
    {
        if (column.IsCheckBox)
        {
            return ToCheckBoxColumn(column);
        }

        return ToDataGridColumn(column, enableFiltering);
    }

    private static GridViewDataColumn ToDataGridColumn(UniTableRegularColumn column, bool enableFiltering)
    {
        var dataColumn = new GridViewDataColumn
        {
            Header = column.ColumnName,
            DataMemberBinding = new Binding(column.GetPropertyName()),
            MinWidth = column.Width,
            IsReadOnly = column.IsReadOnly,
            IsFilterable = enableFiltering
        };

        // Set UniqueName for proper identification
        dataColumn.UniqueName = column.GetPropertyName();

        return dataColumn;
    }

    private static GridViewCheckBoxColumn ToCheckBoxColumn(UniTableRegularColumn column)
    {
        var checkBoxColumn = new GridViewCheckBoxColumn
        {
            Header = column.ColumnName,
            DataMemberBinding = new Binding(column.GetPropertyName()),
            MinWidth = column.Width,
            IsReadOnly = column.IsReadOnly,
            CellStyle = (Style)Application.Current.Resources[ResourceKeys.GridViewCellStyle]
        };

        // Set UniqueName for proper identification
        checkBoxColumn.UniqueName = column.GetPropertyName();

        return checkBoxColumn;
    }

    #endregion
}