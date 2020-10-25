using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BFF.View.Extensions;

namespace BFF.View.Views
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
                int offset = 0;
                DataGridCell cell;
                while ((cell = e.Row.GetCell(offset++)) is null) { } // Collapsed columns are null
                cell.Focus();
                Keyboard.Focus(cell);
                cell.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                e.Row.Loaded -= OnLoaded;
            }

            e.Row.Loaded += OnLoaded;
        }
    }
}
