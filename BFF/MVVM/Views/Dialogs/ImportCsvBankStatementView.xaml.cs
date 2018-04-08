using System.Windows;

namespace BFF.MVVM.Views.Dialogs
{
    public interface IImportCsvBankStatementView
    {

    }

    public partial class ImportCsvBankStatementView : IImportCsvBankStatementView
    {
        public ImportCsvBankStatementView()
        {
            InitializeComponent();
        }

        private void OpenProfileManagementMenu_OnClick(object sender, RoutedEventArgs e)
        {
            ProfileManagementMenu.IsOpen = true;
        }

        private void CloseProfileManagementMenu_OnClick(object sender, RoutedEventArgs e)
        {
            ProfileManagementMenu.IsOpen = false;
        }
    }
}
