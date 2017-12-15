using System;
using System.Globalization;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Properties;
using MuVaViMo;

namespace BFF.MVVM.ViewModels
{
    public class AccountTabsViewModel : SessionViewModelBase
    {
        private readonly IBffOrm _orm;

        public IObservableReadOnlyList<IAccountViewModel> AllAccounts =>
            _orm.CommonPropertyProvider.AllAccountViewModels;

        public ISummaryAccountViewModel SummaryAccountViewModel => 
            _orm.CommonPropertyProvider.AccountViewModelService.SummaryAccountViewModel;

        public INewAccountViewModel NewAccountViewModel { get; }

        public AccountTabsViewModel(IBffOrm orm, INewAccountViewModel newAccountViewModel)
        {
            _orm = orm;
            NewAccountViewModel = newAccountViewModel;

            IDbSetting dbSetting = _orm.BffRepository.DbSettingRepository.Find(1);
            Settings.Default.Culture_SessionCurrency = CultureInfo.GetCultureInfo(dbSetting.CurrencyCultureName);
            Settings.Default.Culture_SessionDate = CultureInfo.GetCultureInfo(dbSetting.DateCultureName);
            ManageCultures();
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

        protected override void OnIsOpenChanged(bool isOpen)
        {
            if (isOpen && SummaryAccountViewModel.IsOpen.Value)
            {
                Messenger.Default.Send(SummaryAccountMessage.Refresh);
            }
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (IAccountViewModel accountViewModel in AllAccounts)
                {
                    (accountViewModel as IDisposable)?.Dispose();
                }
                (SummaryAccountViewModel as IDisposable)?.Dispose();
                _orm?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
