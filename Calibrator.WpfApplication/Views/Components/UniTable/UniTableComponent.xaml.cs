using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Calibrator.WpfApplication.Resources;
using CommunityToolkit.Mvvm.Input;
using Telerik.Windows.Controls;

namespace Calibrator.WpfApplication.Views.Components.UniTable;

public partial class UniTableComponent
{
    private const string TableOperationsColumnName = "Operations";

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
                OnTableOperationsChange)
        );

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
            uniTable.GridView.ItemsSource = e.NewValue;
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

    private void AssertTableOperationsColumn()
    {
        if (!TableOperations.Any())
            return;

        if (GridView.Columns.Count == 0)
            return;

        var gridColumns = GridView.Columns.Cast<GridViewColumn>().ToList();

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

    private static GridViewColumn RegularToGridColumn(UniTableRegularColumn column)
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

