using System.IO;
using System.Windows.Input;
using BFF.DB;
using BFF.Helper.Import;
using BFF.Properties;
using BFF.WPFStuff;
using Microsoft.Win32;

namespace BFF.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly IBffOrm _orm;
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

        public MainWindowViewModel(IBffOrm orm)
        {
            _orm = orm;
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
                _orm.DbPath = saveFileDialog.FileName;
                _orm.CreateNewDatabase();
                Reset();
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
                _orm.DbPath = openFileDialog.FileName;
                Reset();
            }
        }

        protected void ImportBudgetPlan(object importableObject)
        {
            string savePath = ((IImportable)importableObject).Import();
            _orm.DbPath = savePath; //todo: maybe remove, because the import does that already
            Reset();
        }

        protected void Reset()
        {
            if (File.Exists(_orm.DbPath) && ContentViewModel is TitContentViewModel)
            {
                ContentViewModel.Refresh();
                Title = $"{new FileInfo(_orm.DbPath).Name} - BFF";
            }
            else if (File.Exists(_orm.DbPath) && !(ContentViewModel is TitContentViewModel))
            {
                ContentViewModel = new TitContentViewModel(_orm);
                Title = $"{new FileInfo(_orm.DbPath).Name} - BFF";
            }
            else
            {
                ContentViewModel = new EmptyContentViewModel();
                Title = "BFF";
            }
        }
    }
}
