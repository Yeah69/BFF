using System;
using System.Globalization;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.Properties;

namespace BFF.MVVM.Managers
{
    public interface ICultureManager
    {
        IObservable<CultureMessage> RefreshSignal { get; }

        CultureInfo CurrencyCulture { get; set; }

        CultureInfo DateCulture { get; set; }

        bool ShowLongDate { get; set; }
    }

    public abstract class CultureManagerBase : ObservableObject, ICultureManager, IOncePerBackend, IDisposable
    {
        private readonly Subject<CultureMessage> _refreshSignal = new Subject<CultureMessage>();

        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        protected CultureManagerBase()
        {
            _refreshSignal.AddHere(CompositeDisposable);
        }

        public CultureInfo CurrencyCulture
        {
            get => Settings.Default.Culture_SessionCurrency;
            set
            {
                Settings.Default.Culture_SessionCurrency = value;
                ManageCultures();
                RefreshCurrency();
                OnPropertyChanged();
            }
        }

        public CultureInfo DateCulture
        {
            get => Settings.Default.Culture_SessionDate;
            set
            {
                Settings.Default.Culture_SessionDate = value;
                ManageCultures();
                RefreshDate();
                OnPropertyChanged();
            }
        }

        //todo: put DateLong into ShowLongDate, too?
        public bool ShowLongDate
        {
            get => Settings.Default.Culture_DefaultDateLong;
            set
            {
                Settings.Default.Culture_DefaultDateLong = value;
                Settings.Default.Save();
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
            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = customCulture;
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

    public class BackendCultureManager : CultureManagerBase, IBackendCultureManager
    {
        private readonly Subject<Unit> _saveDbSettingsSubject = new Subject<Unit>();

        public BackendCultureManager(IDbSettingRepository dbSettingRepository,
            IRxSchedulerProvider schedulerProvider)
        {
            _saveDbSettingsSubject.AddHere(CompositeDisposable);

            schedulerProvider.Task.MinimalScheduleAsync(async () =>
            {
                IDbSetting dbSetting = await dbSettingRepository.FindAsync(1);
                Settings.Default.Culture_SessionCurrency = CultureInfo.GetCultureInfo(dbSetting.CurrencyCultureName);
                Settings.Default.Culture_SessionDate = CultureInfo.GetCultureInfo(dbSetting.DateCultureName);
                ManageCultures();
            }).AddHere(CompositeDisposable);

            _saveDbSettingsSubject
                .ObserveOn(schedulerProvider.Task)
                .SelectMany(async _ => await dbSettingRepository.FindAsync(1))
                .Subscribe(dbSetting =>
                {
                    dbSetting.CurrencyCulture = Settings.Default.Culture_SessionCurrency;
                    dbSetting.DateCulture = Settings.Default.Culture_SessionDate;
                }).AddHere(CompositeDisposable);
        }

        protected override void SaveCultures()
        {
            Settings.Default.Save();

            _saveDbSettingsSubject.OnNext(Unit.Default);
        }

        protected override CultureInfo CreateCustomCulture()
        {
            CultureInfo customCulture = CultureInfo.CreateSpecificCulture(Settings.Default.Culture_DefaultLanguage.Name);
            customCulture.NumberFormat = Settings.Default.Culture_SessionCurrency.NumberFormat;
            customCulture.DateTimeFormat = Settings.Default.Culture_SessionDate.DateTimeFormat;
            return customCulture;
        }
    }

    public interface IEmptyCultureManager : ICultureManager
    {
    }

    public class EmptyCultureManager : CultureManagerBase, IEmptyCultureManager
    {
        public EmptyCultureManager()
        {
            Settings.Default.Culture_SessionCurrency = Settings.Default.Culture_DefaultCurrency;
            Settings.Default.Culture_SessionDate = Settings.Default.Culture_DefaultDate;
            ManageCultures();
        }

        protected override void SaveCultures()
        {
            Settings.Default.Culture_DefaultCurrency = CurrencyCulture;
            Settings.Default.Culture_DefaultDate = DateCulture;
            Settings.Default.Save();
        }

        protected override CultureInfo CreateCustomCulture()
        {
            CultureInfo customCulture = CultureInfo.CreateSpecificCulture(Settings.Default.Culture_DefaultLanguage.Name);
            customCulture.NumberFormat = Settings.Default.Culture_DefaultCurrency.NumberFormat;
            customCulture.DateTimeFormat = Settings.Default.Culture_DefaultDate.DateTimeFormat;
            return customCulture;
        }
    }
}
