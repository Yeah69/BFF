using System.Globalization;
using System.IO;
using System.Windows.Input;
using BFF.DB;
using BFF.Helper;
using BFF.Helper.Import;
using BFF.Model.Native;
using BFF.WPFStuff;
using Microsoft.Win32;

namespace BFF.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly IBffOrm _orm;
        protected bool FileFlyoutIsOpen;
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

        public CultureInfo CurrencyCulture => BffEnvironment.CultureProvider.CurrencyCulture;
        public CultureInfo DateCulture => BffEnvironment.CultureProvider.DateCulture;
        public bool DateLong
        {
            get { return BffEnvironment.CultureProvider.DateLong; }
            set { BffEnvironment.CultureProvider.DateLong = value; }
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
                Title = (string) WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("OpenSaveDialog_TitleNew", null, BffEnvironment.CultureProvider.LanguageCulture), 
                Filter = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("OpenSaveDialog_Filter", null, BffEnvironment.CultureProvider.LanguageCulture), 
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
                Title = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("OpenSaveDialog_TitleOpen", null, BffEnvironment.CultureProvider.LanguageCulture),
                Filter = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("OpenSaveDialog_Filter", null, BffEnvironment.CultureProvider.LanguageCulture),
                DefaultExt = "*.sqlite"
            };
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
            if (File.Exists(_orm.DbPath) && ContentViewModel is AccountTabsViewModel)
            {
                ResetCultures();
                ContentViewModel.Refresh();
                _orm.Reset();
                Title = $"{new FileInfo(_orm.DbPath).Name} - BFF";
            }
            else if (File.Exists(_orm.DbPath) && !(ContentViewModel is AccountTabsViewModel))
            {
                ResetCultures();
                ContentViewModel = new AccountTabsViewModel(_orm);
                _orm.Reset();
                Title = $"{new FileInfo(_orm.DbPath).Name} - BFF";
            }
            else
            {
                ResetCultures(true);
                ContentViewModel = new EmptyContentViewModel();
                Title = "BFF";
            }
        }

        protected void ResetCultures(bool noDb = false)
        {
            if (noDb)
            {
                BffEnvironment.CultureProvider.CurrencyCulture = CultureInfo.GetCultureInfo("de-DE");
                BffEnvironment.CultureProvider.DateCulture = CultureInfo.GetCultureInfo("de-DE");
            }
            else
            {
                DbSetting dbSetting = _orm.Get<DbSetting>(1);
                BffEnvironment.CultureProvider.CurrencyCulture = CultureInfo.GetCultureInfo(dbSetting.CurrencyCultrureName);
                BffEnvironment.CultureProvider.DateCulture = CultureInfo.GetCultureInfo(dbSetting.DateCultureName);
            }
            OnPropertyChanged(nameof(CurrencyCulture));
            OnPropertyChanged(nameof(DateCulture));
        }
    }
}
