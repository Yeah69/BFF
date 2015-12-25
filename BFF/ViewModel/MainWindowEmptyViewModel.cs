using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using BFF.DB;
using BFF.DB.SQLite;
using BFF.Helper.Import;
using BFF.Model.Native;
using BFF.Properties;
using BFF.WPFStuff;
using Microsoft.Win32;

namespace BFF.ViewModel
{
    public class MainWindowEmptyViewModel : ObservableObject
    {
        protected bool _fileFlyoutIsOpen;
        protected string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public ICommand NewBudgetPlanCommand => new RelayCommand(param => NewBudgetPlan(), param => true);
        public ICommand OpenBudgetPlanCommand => new RelayCommand(param => OpenBudgetPlan(), param => true);
        public ICommand ImportBudgetPlanCommand => new RelayCommand(ImportBudgetPlan, param => true);

        public MainWindowEmptyViewModel()
        {

        }

        protected void NewBudgetPlan()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Create a new Budget Plan", /* todo: Localize */
                Filter = "BFF Budget Plan (*.sqlite)|*.sqlite", /* todo: Localize? */
                DefaultExt = "*.sqlite"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                Reset();
                //SqLiteHelper.CreateNewDatabase(saveFileDialog.FileName, CultureInfo.CurrentCulture); todo

                Settings.Default.DBLocation = saveFileDialog.FileName;
                Settings.Default.Save();
            }
        }

        protected void OpenBudgetPlan()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Open an existing Budget Plan", /* todo: Localize */
                Filter = "BFF Budget Plan (*.sqlite)|*.sqlite", /* todo: Localize? */
                DefaultExt = "*.sqlite"
            }; ;
            if (openFileDialog.ShowDialog() == true)
            {
                Reset();
                //SqLiteHelper.OpenDatabase(openFileDialog.FileName); todo

                Settings.Default.DBLocation = openFileDialog.FileName;
                Settings.Default.Save();
            }
        }

        protected void ImportBudgetPlan(object importableObject)
        {
            Reset();
            string savePath = ((IImportable) importableObject).Import();
            //SqLiteHelper.OpenDatabase(savePath); todo

            Settings.Default.DBLocation = savePath;
            Settings.Default.Save();
        }

        protected void Reset()
        {
            Title = "BFF";
        }
    }
}
