using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.AllCultures).ToList().OrderBy(x => x.Name))
            {
                SumDropDown.Items.Add(culture);
                DateDropDown.Items.Add(culture);
            }
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
