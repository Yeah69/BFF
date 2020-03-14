using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using BFF.Core.Extensions;
using BFF.ViewModel.ViewModels;

namespace BFF.AttachedBehaviors
{
    public class DynamicTableDataGridBehavior : Behavior<DataGrid>
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        private ISubject<Unit> _onTableChanged;

        public static readonly DependencyProperty TableProperty = DependencyProperty.Register(
            nameof(Table),
            typeof(IBudgetOverviewTableViewModel),
            typeof(DynamicTableDataGridBehavior),
            new PropertyMetadata(default(IBudgetOverviewTableViewModel), (o, args) =>
            {
                (o as DynamicTableDataGridBehavior)?._onTableChanged.OnNextUnit();
            }));

        public IBudgetOverviewTableViewModel Table
        {
            get => (IBudgetOverviewTableViewModel)GetValue(TableProperty);
            set => SetValue(TableProperty, value);
        }

        public static readonly DependencyProperty ColumnHeaderTemplateProperty = DependencyProperty.Register(
            nameof(ColumnHeaderTemplate),
            typeof(DataTemplate),
            typeof(DynamicTableDataGridBehavior),
            new PropertyMetadata(default(DataTemplate)));

        

        public DataTemplate ColumnHeaderTemplate
        {
            get => (DataTemplate) GetValue(ColumnHeaderTemplateProperty);
            set => SetValue(ColumnHeaderTemplateProperty, value);
        }

        public static readonly DependencyProperty RowHeaderTemplateProperty = DependencyProperty.Register(
            nameof(RowHeaderTemplate),
            typeof(DataTemplate),
            typeof(DynamicTableDataGridBehavior),
            new PropertyMetadata(default(DataTemplate)));

        

        public DataTemplate RowHeaderTemplate
        {
            get => (DataTemplate) GetValue(RowHeaderTemplateProperty);
            set => SetValue(RowHeaderTemplateProperty, value);
        }

        public static readonly DependencyProperty CellTemplateProperty = DependencyProperty.Register(
            nameof(CellTemplate),
            typeof(DataTemplate),
            typeof(DynamicTableDataGridBehavior),
            new PropertyMetadata(default(DataTemplate)));

        

        public DataTemplate CellTemplate
        {
            get => (DataTemplate) GetValue(CellTemplateProperty);
            set => SetValue(CellTemplateProperty, value);
        }

        public static readonly DependencyProperty CornerHeaderTemplateProperty = DependencyProperty.Register(
            nameof(CornerHeaderTemplate),
            typeof(DataTemplate),
            typeof(DynamicTableDataGridBehavior),
            new PropertyMetadata(default(DataTemplate)));

        

        public DataTemplate CornerHeaderTemplate
        {
            get => (DataTemplate) GetValue(CornerHeaderTemplateProperty);
            set => SetValue(CornerHeaderTemplateProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            _onTableChanged = new Subject<Unit>();

            Observable.FromEventPattern<DependencyPropertyChangedEventArgs>(
                AssociatedObject,
                nameof(AssociatedObject.DataContextChanged))
                .ToUnitObservable()
                .Merge(_onTableChanged)
                .Subscribe(_ =>
                {
                    AssociatedObject.Columns.Clear();

                    if (AssociatedObject.DataContext is null || Table is null) return;

                    SetupRowHeaderColumn(AssociatedObject);

                    //AssociatedObject.SetBinding(ItemsControl.ItemsSourceProperty, Table.Rows);

                    void SetupRowHeaderColumn(DataGrid dataGrid)
                    {
                        var cornerHeaderContentControl = new FrameworkElementFactory(typeof(ContentControl));
                        cornerHeaderContentControl.SetValue(FrameworkElement.DataContextProperty, dataGrid.DataContext);
                        cornerHeaderContentControl.SetBinding(ContentControl.ContentProperty, new Binding("."));
                        cornerHeaderContentControl.SetValue(ContentControl.ContentTemplateProperty, CornerHeaderTemplate);
                        var cornerHeaderDataTemplate = new DataTemplate { VisualTree = cornerHeaderContentControl };

                        var rowHeaderColumnDataGridTemplateColumn = new DataGridTemplateColumn();
                        rowHeaderColumnDataGridTemplateColumn.SetValue(DataGridColumn.HeaderTemplateProperty, cornerHeaderDataTemplate);
                        
                        var rowHeaderContentControl = new FrameworkElementFactory(typeof(ContentControl));
                        rowHeaderContentControl.SetBinding(ContentControl.ContentProperty, new Binding("RowHeader"));
                        rowHeaderContentControl.SetValue(ContentControl.ContentTemplateProperty, RowHeaderTemplate);
                        var rowHeaderDataTemplate = new DataTemplate { VisualTree = rowHeaderContentControl };

                        rowHeaderColumnDataGridTemplateColumn.SetValue(DataGridTemplateColumn.CellTemplateProperty, rowHeaderDataTemplate);

                        dataGrid.Columns.Add(rowHeaderColumnDataGridTemplateColumn);

                        for (var i = 0; i < Table.ColumnCount; i++)
                        {
                            var columnHeaderContentControl = new FrameworkElementFactory(typeof(ContentControl));
                            columnHeaderContentControl.SetValue(FrameworkElement.DataContextProperty, Table);
                            columnHeaderContentControl.SetBinding(ContentControl.ContentProperty, new Binding($"{nameof(Table.ColumnHeaders)}[{i}]"));
                            columnHeaderContentControl.SetValue(ContentControl.ContentTemplateProperty, ColumnHeaderTemplate);
                            var columnHeaderDataTemplate = new DataTemplate { VisualTree = columnHeaderContentControl };

                            var columnDataGridTemplateColumn = new DataGridTemplateColumn();
                            columnDataGridTemplateColumn.SetValue(DataGridColumn.HeaderTemplateProperty, columnHeaderDataTemplate);
                        
                            var cellContentControl = new FrameworkElementFactory(typeof(ContentControl));
                            cellContentControl.SetBinding(ContentControl.ContentProperty, new Binding($"{nameof(ITableRowViewModel<object, object>.Cells)}.Item[{i}]"));
                            cellContentControl.SetValue(ContentControl.ContentTemplateProperty, CellTemplate);
                            var cellDataTemplate = new DataTemplate { VisualTree = cellContentControl };

                            columnDataGridTemplateColumn.SetValue(DataGridTemplateColumn.CellTemplateProperty, cellDataTemplate);

                            dataGrid.Columns.Add(columnDataGridTemplateColumn);
                        }
                    }
                });
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            _compositeDisposable.Dispose();
        }
    }
}
