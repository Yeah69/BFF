using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using BFF.DB;
using BFF.Model.Native;
using BFF.Properties;
using BFF.ViewModel.ForModels;
using BFF.WPFStuff;

namespace BFF.ViewModel
{
    public class AccountTabsViewModel : SessionViewModelBase
    {
        protected readonly IBffOrm _orm;

        public IBffOrm Orm => _orm;

        public ObservableCollection<AccountViewModel> AllAccounts => _orm.CommonPropertyProvider.AccountViewModels;

        public ObservableCollection<Category> AllCategories => _orm.AllCategories; 

        public AllAccountsViewModel AllAccountsViewModel
        {
            get { return _orm.CommonPropertyProvider.AllAccountsViewModel; }
            set
            {
                OnPropertyChanged();
            }
        }

        public Account NewAccount { get; set; } = new Account {Id = -1, Name = "", StartingBalance = 0L};
        
        public ICommand NewAccountCommand => new RelayCommand(param =>
        {
            //Insert Account to Database
            _orm.Insert(NewAccount);
            //Refresh all relevant properties
            //NewAccount.RefreshBalance(); todo
            //if (Account.allAccounts != null) todo
            //{
            //    Account.allAccounts.RefreshBalance(); todo
            //    Account.allAccounts.RefreshStartingBalance(); todo
            //}
            //Refresh dummy-Account
            NewAccount = new Account { Id = -1, Name = "", StartingBalance = 0L };
            OnPropertyChanged(nameof(NewAccount));
        }
        , param => !string.IsNullOrEmpty(NewAccount.Name));
        
        public Func<string, Payee> CreatePayeeFunc => name => 
        {
            Payee ret = new Payee {Name = name};
            _orm.Insert(ret);
            return ret;
        }; 

        public AccountTabsViewModel(IBffOrm orm)
        {
            _orm = orm;

            DbSetting dbSetting = orm.Get<DbSetting>(1);
            Settings.Default.Culture_SessionCurrency = CultureInfo.GetCultureInfo(dbSetting.CurrencyCultrureName);
            Settings.Default.Culture_SessionDate = CultureInfo.GetCultureInfo(dbSetting.DateCultureName);
            ManageCultures();

            Messenger.Default.Register<CutlureMessage>(this, message =>
            {
                switch(message)
                {
                    case CutlureMessage.Refresh:
                    case CutlureMessage.RefreshCurrency:
                        OnPropertyChanged(nameof(NewAccount));
                        break;
                    case CutlureMessage.RefreshDate:
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }
            });
        }

        #region Overrides of SessionViewModelBase

        protected override CultureInfo CreateCustomCulture()
        {
            CultureInfo customCulture = CultureInfo.CreateSpecificCulture(Settings.Default.Culture_DefaultLanguage.Name);
            customCulture.NumberFormat = Settings.Default.Culture_SessionCurrency.NumberFormat;
            customCulture.DateTimeFormat = Settings.Default.Culture_SessionDate.DateTimeFormat;
            return customCulture;
        }

        protected override void SaveCultures()
        {
            Settings.Default.Save();
            DbSetting dbSetting = _orm.Get<DbSetting>(1);
            dbSetting.CurrencyCulture = Settings.Default.Culture_SessionCurrency;
            dbSetting.DateCulture = Settings.Default.Culture_SessionDate;
            _orm.Update(dbSetting);
        }

        #endregion
    }
}
