using System;
using System.Globalization;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.Windows;

namespace BFF.ViewModel.Managers
{
    public interface ICultureManager : IScopeInstance
    {
        IObservable<CultureMessage> RefreshSignal { get; }

        CultureInfo CurrencyCulture { get; set; }

        CultureInfo DateCulture { get; set; }

        bool ShowLongDate { get; set; }
    }

    internal abstract class CultureManagerBase : ObservableObject, ICultureManager, IDisposable
    {
        private readonly IBffSettings _bffSettings;
        private readonly Subject<CultureMessage> _refreshSignal = new();

        protected readonly CompositeDisposable CompositeDisposable = new();

        protected CultureManagerBase(
            IBffSettings bffSettings)
        {
            _bffSettings = bffSettings;
            _refreshSignal.CompositeDisposalWith(CompositeDisposable);
        }

        public CultureInfo CurrencyCulture
        {
            get => _bffSettings.Culture_SessionCurrency;
            set
            {
                _bffSettings.Culture_SessionCurrency = value;
                ManageCultures();
                RefreshCurrency();
                OnPropertyChanged();
            }
        }

        public CultureInfo DateCulture
        {
            get => _bffSettings.Culture_SessionDate;
            set
            {
                _bffSettings.Culture_SessionDate = value;
                ManageCultures();
                RefreshDate();
                OnPropertyChanged();
            }
        }

        //todo: put DateLong into ShowLongDate, too?
        public bool ShowLongDate
        {
            get => _bffSettings.Culture_DefaultDateLong;
            set
            {
                _bffSettings.Culture_DefaultDateLong = value;
                RefreshDate();
                OnPropertyChanged();
            }
        }

        public IObservable<CultureMessage> RefreshSignal => _refreshSignal.AsObservable();

        protected void Refresh()
        {
            _refreshSignal.OnNext(CultureMessage.Refresh);
        }

        protected void RefreshCurrency()
        {
            _refreshSignal.OnNext(CultureMessage.RefreshCurrency);
        }

        protected void RefreshDate()
        {
            _refreshSignal.OnNext(CultureMessage.RefreshDate);
        }

        public void Dispose()
        {
            CompositeDisposable.Dispose();
        }

        protected void ManageCultures()
        {
            CultureInfo customCulture = CreateCustomCulture();
            Thread.CurrentThread.CurrentCulture = customCulture;
            Thread.CurrentThread.CurrentUICulture = customCulture;
            SaveCultures();
        }

        protected abstract void SaveCultures();

        protected abstract CultureInfo CreateCustomCulture();
    }

    public interface IBackendCultureManager : ICultureManager
    {
    }

    internal class BackendCultureManager : CultureManagerBase, IBackendCultureManager
    {
        private readonly IBffSettings _bffSettings;
        private readonly Subject<Unit> _saveDbSettingsSubject = new();

        public BackendCultureManager(
            IDbSettingRepository dbSettingRepository,
            IBffSettings bffSettings,
            IRxSchedulerProvider schedulerProvider)
            : base(bffSettings)
        {
            _bffSettings = bffSettings;
            _saveDbSettingsSubject.CompositeDisposalWith(CompositeDisposable);

            schedulerProvider.Task.MinimalScheduleAsync(async () =>
            {
                IDbSetting dbSetting = await dbSettingRepository.GetSetting().ConfigureAwait(false);
                _bffSettings.Culture_SessionCurrency = CultureInfo.GetCultureInfo(dbSetting.CurrencyCultureName);
                _bffSettings.Culture_SessionDate = CultureInfo.GetCultureInfo(dbSetting.DateCultureName);
                ManageCultures();
            }).CompositeDisposalWith(CompositeDisposable);

            _saveDbSettingsSubject
                .ObserveOn(schedulerProvider.Task)
                .SelectMany(_ => dbSettingRepository.GetSetting())
                .Subscribe(dbSetting =>
                {
                    dbSetting.CurrencyCulture = _bffSettings.Culture_SessionCurrency;
                    dbSetting.DateCulture = _bffSettings.Culture_SessionDate;
                }).CompositeDisposalWith(CompositeDisposable);
        }

        protected override void SaveCultures()
        {
            _saveDbSettingsSubject.OnNext(Unit.Default);
        }

        protected override CultureInfo CreateCustomCulture()
        {
            CultureInfo customCulture = CultureInfo.CreateSpecificCulture(_bffSettings.Culture_DefaultLanguage.Name);
            customCulture.NumberFormat = _bffSettings.Culture_SessionCurrency.NumberFormat;
            customCulture.DateTimeFormat = _bffSettings.Culture_SessionDate.DateTimeFormat;
            return customCulture;
        }
    }

    public interface IEmptyCultureManager : ICultureManager
    {
    }

    internal class EmptyCultureManager : CultureManagerBase, IEmptyCultureManager
    {
        private readonly IBffSettings _bffSettings;

        public EmptyCultureManager(
            IBffSettings bffSettings)
            : base(bffSettings)
        {
            _bffSettings = bffSettings;
            _bffSettings.Culture_SessionCurrency = _bffSettings.Culture_DefaultCurrency;
            _bffSettings.Culture_SessionDate = _bffSettings.Culture_DefaultDate;
            ManageCultures();
        }

        protected override void SaveCultures()
        {
            _bffSettings.Culture_DefaultCurrency = CurrencyCulture;
            _bffSettings.Culture_DefaultDate = DateCulture;
        }

        protected override CultureInfo CreateCustomCulture()
        {
            CultureInfo customCulture = CultureInfo.CreateSpecificCulture(_bffSettings.Culture_DefaultLanguage.Name);
            customCulture.NumberFormat = _bffSettings.Culture_DefaultCurrency.NumberFormat;
            customCulture.DateTimeFormat = _bffSettings.Culture_DefaultDate.DateTimeFormat;
            return customCulture;
        }
    }
}
