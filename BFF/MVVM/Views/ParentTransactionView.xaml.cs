using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BFF.Helper.Extensions;

namespace BFF.MVVM.Views
{
    public partial class ParentTransactionView
    {
        public ParentTransactionView()
        {
            InitializeComponent();
        }

        private void NewSubTransactionsDataGrid_OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            void OnLoaded(object _, RoutedEventArgs __)
            {
                var dataGridCell = e.Row.GetCell(0);
                dataGridCell.Focus();
                Keyboard.Focus(dataGridCell);
                dataGridCell.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                e.Row.Loaded -= OnLoaded;
            }

            e.Row.Loaded += OnLoaded;
        }
    }
}
