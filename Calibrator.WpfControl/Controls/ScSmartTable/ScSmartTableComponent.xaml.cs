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

    // Dictionary to store allowed filter operators per column
    private readonly Dictionary<string, List<FilterOperator>> _columnFilterOperators = new();

    /// <summary>
    /// Gets the command for executing table actions
    /// </summary>
    public AsyncRelayCommand<object> OnExecuteActionCommand { get; private set; }

    /// <summary>
    /// Initializes a new instance of the ScSmartTableComponent class
    /// </summary>
    public ScSmartTableComponent()
    {
        InitializeComponent();
        OnExecuteActionCommand = new(OnExecuteAction);

        // Set default FilteringMode to FilterRow for cleaner UI
        GridView.FilteringMode = Telerik.Windows.Controls.GridView.FilteringMode.FilterRow;

        // Subscribe to events
        GridView.Sorted += OnGridSorted;
        GridView.Filtered += OnGridFiltered;
        GridView.FilterOperatorsLoading += OnFilterOperatorsLoading; // Handle filter operators

        // Apply initial settings
        ApplyFilteringMode();
    }

    #region Dependency Properties

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
    /// Gets or sets whether grouping is enabled for the table
    /// </summary>
    public bool EnableGrouping
    {
        get => (bool)this.GetValue(EnableGroupingProperty);
        set => this.SetValue(EnableGroupingProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the search panel is visible
    /// </summary>
    public bool ShowSearchPanel
    {
        get => (bool)this.GetValue(ShowSearchPanelProperty);
        set => this.SetValue(ShowSearchPanelProperty, value);
    }

    /// <summary>
    /// Gets or sets whether users can reorder columns
    /// </summary>
    public bool EnableColumnReordering
    {
        get => (bool)this.GetValue(EnableColumnReorderingProperty);
        set => this.SetValue(EnableColumnReorderingProperty, value);
    }

    /// <summary>
    /// Gets or sets whether users can freeze columns
    /// </summary>
    public bool EnableColumnFreezing
    {
        get => (bool)this.GetValue(EnableColumnFreezingProperty);
        set => this.SetValue(EnableColumnFreezingProperty, value);
    }

    /// <summary>
    /// Gets or sets whether column footers are visible
    /// </summary>
    public bool ShowColumnFooters
    {
        get => (bool)this.GetValue(ShowColumnFootersProperty);
        set => this.SetValue(ShowColumnFootersProperty, value);
    }

    /// <summary>
    /// Gets or sets the filtering mode (Popup or FilterRow)
    /// </summary>
    public Telerik.Windows.Controls.GridView.FilteringMode FilteringMode
    {
        get => (Telerik.Windows.Controls.GridView.FilteringMode)this.GetValue(FilteringModeProperty);
        set => this.SetValue(FilteringModeProperty, value);
    }

    /// <summary>
    /// Identifies the Columns dependency property
    /// </summary>
    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(object),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnColumnsChanged));

    /// <summary>
    /// Identifies the ItemsSource dependency property
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnItemsSourceChanged));

    /// <summary>
    /// Identifies the TableOperations dependency property
    /// </summary>
    public static readonly DependencyProperty TableOperationsProperty =
        DependencyProperty.Register(nameof(TableOperations), typeof(ICollection<UniTableBaseAction>),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(new List<UniTableBaseAction>(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTableOperationsChange));

    /// <summary>
    /// Identifies the EnableFiltering dependency property
    /// </summary>
    public static readonly DependencyProperty EnableFilteringProperty =
        DependencyProperty.Register(nameof(EnableFiltering), typeof(bool),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Identifies the EnableSorting dependency property
    /// </summary>
    public static readonly DependencyProperty EnableSortingProperty =
        DependencyProperty.Register(nameof(EnableSorting), typeof(bool),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Identifies the EnableGrouping dependency property
    /// </summary>
    public static readonly DependencyProperty EnableGroupingProperty =
        DependencyProperty.Register(nameof(EnableGrouping), typeof(bool),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(false));

    /// <summary>
    /// Identifies the ShowSearchPanel dependency property
    /// </summary>
    public static readonly DependencyProperty ShowSearchPanelProperty =
        DependencyProperty.Register(nameof(ShowSearchPanel), typeof(bool),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(false));

    /// <summary>
    /// Identifies the EnableColumnReordering dependency property
    /// </summary>
    public static readonly DependencyProperty EnableColumnReorderingProperty =
        DependencyProperty.Register(nameof(EnableColumnReordering), typeof(bool),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Identifies the EnableColumnFreezing dependency property
    /// </summary>
    public static readonly DependencyProperty EnableColumnFreezingProperty =
        DependencyProperty.Register(nameof(EnableColumnFreezing), typeof(bool),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(false));

    /// <summary>
    /// Identifies the ShowColumnFooters dependency property
    /// </summary>
    public static readonly DependencyProperty ShowColumnFootersProperty =
        DependencyProperty.Register(nameof(ShowColumnFooters), typeof(bool),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(false));

    /// <summary>
    /// Identifies the FilteringMode dependency property
    /// </summary>
    public static readonly DependencyProperty FilteringModeProperty =
        DependencyProperty.Register(nameof(FilteringMode), typeof(Telerik.Windows.Controls.GridView.FilteringMode),
            typeof(ScSmartTableComponent),
            new FrameworkPropertyMetadata(Telerik.Windows.Controls.GridView.FilteringMode.FilterRow, OnFilteringModeChanged));

    #endregion

    /// <summary>
    /// Clears all active filters from the table
    /// </summary>
    public void ClearFilters()
    {
        GridView.FilterDescriptors.Clear();
    }

    #region Event Handlers

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

    private void OnGridSorted(object? sender, GridViewSortedEventArgs e)
    {
        // Event for external handling if needed
    }

    private void OnGridFiltered(object? sender, GridViewFilteredEventArgs e)
    {
        // Event for external handling if needed
    }

    /// <summary>
    /// Handle FilterOperatorsLoading event to customize available filter operators per column
    /// This is the recommended Telerik approach per: https://docs.telerik.com/devtools/wpf/controls/radgridview/filtering/how-to/howto-remove-some-of-the-available-filter-operators
    /// Note: AvailableOperators is a RemoveOnlyCollection - we can only remove unwanted operators, not add new ones
    /// </summary>
    private void OnFilterOperatorsLoading(object? sender, FilterOperatorsLoadingEventArgs e)
    {
        // Get column unique name
        var columnUniqueName = e.Column?.UniqueName;

        if (string.IsNullOrEmpty(columnUniqueName))
            return;

        // If we have defined operators for this column, remove the ones we don't want
        if (_columnFilterOperators.TryGetValue(columnUniqueName, out var allowedOperators))
        {
            // Create a list of operators to remove (those NOT in allowedOperators)
            var operatorsToRemove = new List<FilterOperator>();

            foreach (var availableOp in e.AvailableOperators)
            {
                if (!allowedOperators.Contains(availableOp))
                {
                    operatorsToRemove.Add(availableOp);
                }
            }

            // Remove unwanted operators
            foreach (var opToRemove in operatorsToRemove)
            {
                e.AvailableOperators.Remove(opToRemove);
            }
        }
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
        smartTable._columnFilterOperators.Clear(); // Clear old operators

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
                        ConfigureColumnFiltering(smartGridColumn, smartColumn, smartTable);
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

    private static void ConfigureColumnFiltering(
        Telerik.Windows.Controls.GridViewColumn gridColumn,
        SmartTableRegularColumn column,
        ScSmartTableComponent smartTable)
    {
        // Determine which filter operators to use
        ICollection<FilterOperatorType> allowedOperators;

        if (column.AllowedFilterOperators != null && column.AllowedFilterOperators.Count > 0)
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

        if (distinctOperators.Count > 0)
        {
            // Store operators in dictionary for use in FilterOperatorsLoading event
            smartTable._columnFilterOperators[gridColumn.UniqueName] = distinctOperators;

            // Set default operator
            var defaultOperator = column.DefaultFilterOperator ?? allowedOperators.FirstOrDefault();
            var telerikDefaultOp = FilterOperatorHelper.ToTelerikOperator(defaultOperator);
            gridColumn.ColumnFilterDescriptor.FieldFilter.Filter1.Operator = telerikDefaultOp;
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
        if (this.TableOperations.Count == 0)
        {
            return;
        }

        if (this.GridView.Columns.Count == 0)
        {
            return;
        }

        var gridColumns = this.GridView.Columns.Cast<Telerik.Windows.Controls.GridViewColumn>().ToList();

        if (gridColumns.Any(a => a.UniqueName == TableOperationsColumnName))
        {
            return;
        }

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
        var propertyName = column.GetPropertyName();

        var dataColumn = new GridViewDataColumn
        {
            Header = column.ColumnName,
            DataMemberBinding = new Binding(propertyName),
            MinWidth = column.Width,
            IsReadOnly = column.IsReadOnly,
            IsFilterable = enableFiltering,
            FilterMemberPath = propertyName, // CRITICAL: Tells Telerik which property to use for filtering
            UniqueName = propertyName
        };

        return dataColumn;
    }

    private static GridViewCheckBoxColumn ToCheckBoxColumn(UniTableRegularColumn column)
    {
        var propertyName = column.GetPropertyName();

        var checkBoxColumn = new GridViewCheckBoxColumn
        {
            Header = column.ColumnName,
            DataMemberBinding = new Binding(propertyName),
            MinWidth = column.Width,
            IsReadOnly = column.IsReadOnly,
            CellStyle = (Style)Application.Current.Resources[ResourceKeys.GridViewCellStyle],
            FilterMemberPath = propertyName, // CRITICAL: Tells Telerik which property to use for filtering
            UniqueName = propertyName
        };

        return checkBoxColumn;
    }

    #endregion
}