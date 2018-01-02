using System;
using System.Globalization;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Properties;
using MuVaViMo;

namespace BFF.MVVM.ViewModels
{
    public interface IAccountTabsViewModel
    {
        INewAccountViewModel NewAccountViewModel { get; }
        IObservableReadOnlyList<IAccountViewModel> AllAccounts { get; }
        ISummaryAccountViewModel SummaryAccountViewModel { get; }
        void ManageCultures();
    }

    public class AccountTabsViewModel : SessionViewModelBase, IAccountTabsViewModel
    {
        private readonly IAccountViewModelService _accountViewModelService;
        private readonly IDbSettingRepository _dbSettingRepository;

        public IObservableReadOnlyList<IAccountViewModel> AllAccounts =>
            _accountViewModelService.All;

        public ISummaryAccountViewModel SummaryAccountViewModel =>
            _accountViewModelService.SummaryAccountViewModel;

        public INewAccountViewModel NewAccountViewModel { get; }

        public AccountTabsViewModel(
            INewAccountViewModel newAccountViewModel,
            IAccountViewModelService accountViewModelService, 
            IDbSettingRepository dbSettingRepository)
        {
            _accountViewModelService = accountViewModelService;
            _dbSettingRepository = dbSettingRepository;
            NewAccountViewModel = newAccountViewModel;

            IDbSetting dbSetting = _dbSettingRepository.Find(1);
            Settings.Default.Culture_SessionCurrency = CultureInfo.GetCultureInfo(dbSetting.CurrencyCultureName);
            Settings.Default.Culture_SessionDate = CultureInfo.GetCultureInfo(dbSetting.DateCultureName);
            ManageCultures();

            IsOpen.Value = true;
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
            IDbSetting dbSetting = _dbSettingRepository.Find(1);
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
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
