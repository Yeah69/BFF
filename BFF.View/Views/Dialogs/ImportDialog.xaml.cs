using System.Windows;

namespace BFF.View.Views.Dialogs
{
    public partial class ImportDialog
    {
        public ImportDialog()
        {
            InitializeComponent();
        }

        private void ImportDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            //TransactionTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            //BudgetTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            //SavePathTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }
    }
}
