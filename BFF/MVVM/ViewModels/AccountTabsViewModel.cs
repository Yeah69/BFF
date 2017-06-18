using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Properties;

namespace BFF.MVVM.ViewModels
{
    public class AccountTabsViewModel : SessionViewModelBase, IDisposable
    {
        private readonly IBffOrm _orm;

        public ObservableCollection<IAccountViewModel> AllAccounts => _orm.CommonPropertyProvider.AllAccountViewModels;

        public ISummaryAccountViewModel SummaryAccountViewModel
        {
            get => _orm.CommonPropertyProvider.SummaryAccountViewModel;
            set => OnPropertyChanged();
        }

        public IAccount NewAccount { get; set; }
        
        public ICommand NewAccountCommand => new RelayCommand(param =>
        {
            //Insert Account to Database
            NewAccount.Insert();
            //Refresh all relevant properties
            Messenger.Default.Send(AccountMessage.RefreshBalance, NewAccount);
            Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
            Messenger.Default.Send(SummaryAccountMessage.RefreshStartingBalance);
            //Refresh dummy-Account
            NewAccount = _orm.BffRepository.AccountRepository.Create();
            OnPropertyChanged(nameof(NewAccount));
        }
        , param => !string.IsNullOrEmpty(NewAccount.Name));

        public AccountTabsViewModel(IBffOrm orm)
        {
            _orm = orm;
            NewAccount = _orm.BffRepository.AccountRepository.Create();

            IDbSetting dbSetting = _orm.BffRepository.DbSettingRepository.Find(1);
            Settings.Default.Culture_SessionCurrency = CultureInfo.GetCultureInfo(dbSetting.CurrencyCultureName);
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
            IDbSetting dbSetting = _orm.BffRepository.DbSettingRepository.Find(1);
            dbSetting.CurrencyCulture = Settings.Default.Culture_SessionCurrency;
            dbSetting.DateCulture = Settings.Default.Culture_SessionDate;
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            foreach(IAccountViewModel accountViewModel in AllAccounts)
            {
                (accountViewModel as IDisposable)?.Dispose();
            }
            AllAccounts.Clear();
            (SummaryAccountViewModel as IDisposable)?.Dispose();
            Messenger.Default.Unregister<CutlureMessage>(this);
        }

        #endregion
    }
}
