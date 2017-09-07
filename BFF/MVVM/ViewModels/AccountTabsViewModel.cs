using System;
using System.ComponentModel;
using System.Globalization;
using System.Reactive.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Properties;
using MuVaViMo;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels
{
    public class AccountTabsViewModel : SessionViewModelBase, IDisposable
    {
        private readonly IBffOrm _orm;

        public IObservableReadOnlyList<IAccountViewModel> AllAccounts =>
            _orm.CommonPropertyProvider.AllAccountViewModels;

        public ISummaryAccountViewModel SummaryAccountViewModel => 
            _orm.CommonPropertyProvider.AccountViewModelService.SummaryAccountViewModel;

        public IAccountViewModel NewAccount { get; set; }
        
        public ReactiveCommand NewAccountCommand { get; }

        public AccountTabsViewModel(IBffOrm orm, AccountViewModelService accountViewModelService)
        {
            _orm = orm;
            NewAccount = accountViewModelService.GetNewNonInsertedViewModel();

            IDbSetting dbSetting = _orm.BffRepository.DbSettingRepository.Find(1);
            Settings.Default.Culture_SessionCurrency = CultureInfo.GetCultureInfo(dbSetting.CurrencyCultureName);
            Settings.Default.Culture_SessionDate = CultureInfo.GetCultureInfo(dbSetting.DateCultureName);
            ManageCultures();

            Messenger.Default.Register<CultureMessage>(this, message =>
            {
                switch(message)
                {
                    case CultureMessage.Refresh:
                    case CultureMessage.RefreshCurrency:
                        OnPropertyChanged(nameof(NewAccount));
                        break;
                    case CultureMessage.RefreshDate:
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }
            });

            NewAccountCommand = NewAccount.Name.Select(name => !string.IsNullOrEmpty(name)).ToReactiveCommand();
            NewAccountCommand.Subscribe(_ =>
            {
                //Insert Account to Database
                NewAccount.Insert();
                //Refresh all relevant properties
                NewAccount.RefreshBalance();
                Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
                Messenger.Default.Send(SummaryAccountMessage.RefreshStartingBalance);
                //Refresh dummy-Account
                NewAccount = accountViewModelService.GetNewNonInsertedViewModel();
                OnPropertyChanged(nameof(NewAccount));
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
            (SummaryAccountViewModel as IDisposable)?.Dispose();
            Messenger.Default.Unregister<CultureMessage>(this);
        }

        #endregion
    }
}
