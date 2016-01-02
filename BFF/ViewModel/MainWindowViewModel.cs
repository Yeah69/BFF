using System.IO;
using System.Threading;
using System.Windows.Input;
using BFF.DB;
using BFF.Helper;
using BFF.Helper.Import;
using BFF.Properties;
using BFF.WPFStuff;
using Microsoft.Win32;
using Ninject;

namespace BFF.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        protected bool _fileFlyoutIsOpen;
        protected string _title;
        private EmptyContentViewModel _contentViewModel;

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

        public EmptyContentViewModel ContentViewModel
        {
            get { return _contentViewModel; }
            set
            {
                _contentViewModel = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {
            Reset();
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
                using (StandardKernel kernel = new StandardKernel(new BffNinjectModule()))
                {
                    kernel.Get<IBffOrm>().DbPath = openFileDialog.FileName;
                }

                Reset();
                //SqLiteHelper.OpenDatabase(openFileDialog.FileName); todo
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
            using (StandardKernel kernel = new StandardKernel(new BffNinjectModule()))
            {
                IBffOrm orm = kernel.Get<IBffOrm>();
                if (File.Exists(orm.DbPath))
                {
                    ContentViewModel = new TitContentViewModel(orm);
                    Title = $"{(new FileInfo(orm.DbPath)).Name} - BFF";
                }
                else
                {
                    ContentViewModel = new EmptyContentViewModel();
                    Title = "BFF";
                }
            }
        }
    }
}
