using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Services;
using BFF.Properties;
using MuVaViMo;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels.Utility
{
    public interface ICsvBankStatementImportItemViewModel : IDisposable
    {
        IReactiveProperty<DateTime> Date { get; }

        IReactiveProperty<bool> HasDate { get; }

        ReactiveCommand AdmitDate { get; }

        ReactiveCommand DismissDate { get; }

        bool IsDateFormatLong { get; }

        IReactiveProperty<string> Payee { get; }

        IReactiveProperty<bool> HasPayee { get; }

        IReadOnlyReactiveProperty<bool> CreatePayeeIfNotExisting { get; }

        ReactiveCommand AdmitPayee { get; }

        ReactiveCommand DismissPayee { get; }

        IReadOnlyReactiveProperty<bool> PayeeExists { get; }

        IObservableReadOnlyList<IPayeeViewModel> ExistingPayees { get; }

        IReactiveProperty<string> Memo { get; }

        IReactiveProperty<bool> HasMemo { get; }

        ReactiveCommand AdmitMemo { get; }

        ReactiveCommand DismissMemo { get; }

        IReactiveProperty<long> Sum { get; }

        ISumEditViewModel SumEdit { get; }

        IReactiveProperty<bool> HasSum { get; }

        ReactiveCommand AdmitSum { get; }

        ReactiveCommand DismissSum { get; }
    }

    public class CsvBankStatementImportItemViewModel : ICsvBankStatementImportItemViewModel
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public CsvBankStatementImportItemViewModel(
            (DateTime? Date, string Payee, bool CreatePayeeIfNotExisting, string Memo, long? Sum) configuration,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            IPayeeViewModelService payeeService,
            IRxSchedulerProvider schedulerProvider)
        {
            Date     = new ReactivePropertySlim<DateTime>(configuration.Date ?? DateTime.Today, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            Payee    = new ReactivePropertySlim<string>(configuration.Payee, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            Memo     = new ReactivePropertySlim<string>(configuration.Memo, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            Sum      = new ReactivePropertySlim<long>(configuration.Sum ?? 0L, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            HasDate  = new ReactivePropertySlim<bool>(configuration.Date.HasValue, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            HasPayee = new ReactivePropertySlim<bool>(configuration.Payee != null, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            HasMemo  = new ReactivePropertySlim<bool>(configuration.Memo != null, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            HasSum   = new ReactivePropertySlim<bool>(configuration.Sum.HasValue, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);

            SumEdit = createSumEdit(Sum).AddHere(_compositeDisposable);

            AdmitDate = new ReactiveCommand().AddHere(_compositeDisposable);
            AdmitDate
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(_ => HasDate.Value = true)
                .AddHere(_compositeDisposable);

            AdmitPayee = new ReactiveCommand().AddHere(_compositeDisposable);
            AdmitPayee
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(_ => HasPayee.Value = true)
                .AddHere(_compositeDisposable);

            PayeeExists = Payee
                .Select(p => payeeService.All.Any(payee => payee.Name.Value == p))
                .ToReadOnlyReactivePropertySlim(payeeService.All.Any(payee => payee.Name.Value == Payee.Value), ReactivePropertyMode.DistinctUntilChanged)
                .AddHere(_compositeDisposable);

            CreatePayeeIfNotExisting = new ReactiveProperty<bool>(configuration.CreatePayeeIfNotExisting, ReactivePropertyMode.DistinctUntilChanged);

            ExistingPayees = payeeService.All;

            AdmitMemo = new ReactiveCommand().AddHere(_compositeDisposable);
            AdmitMemo
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(_ => HasMemo.Value = true)
                .AddHere(_compositeDisposable);

            AdmitSum = new ReactiveCommand().AddHere(_compositeDisposable);
            AdmitSum
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(_ => HasSum.Value = true)
                .AddHere(_compositeDisposable);

            DismissDate = new ReactiveCommand().AddHere(_compositeDisposable);
            DismissDate
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(_ => HasDate.Value = false)
                .AddHere(_compositeDisposable);

            DismissPayee = new ReactiveCommand().AddHere(_compositeDisposable);
            DismissPayee
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(_ => HasPayee.Value = false)
                .AddHere(_compositeDisposable);

            DismissMemo = new ReactiveCommand().AddHere(_compositeDisposable);
            DismissMemo
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(_ => HasMemo.Value = false)
                .AddHere(_compositeDisposable);

            DismissSum = new ReactiveCommand().AddHere(_compositeDisposable);
            DismissSum
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(_ => HasSum.Value = false)
                .AddHere(_compositeDisposable);
        }

        public IReactiveProperty<DateTime> Date { get; set; }
        public IReactiveProperty<bool> HasDate { get; }
        public ReactiveCommand AdmitDate { get; }
        public ReactiveCommand DismissDate { get; }

        public IReactiveProperty<string> Payee { get; set; }
        public IReactiveProperty<bool> HasPayee { get; }
        public IReadOnlyReactiveProperty<bool> CreatePayeeIfNotExisting { get; }
        public ReactiveCommand AdmitPayee { get; }
        public ReactiveCommand DismissPayee { get; }
        public IReadOnlyReactiveProperty<bool> PayeeExists { get; }
        public IObservableReadOnlyList<IPayeeViewModel> ExistingPayees { get; }

        public IReactiveProperty<string> Memo { get; set; }
        public IReactiveProperty<bool> HasMemo { get; }
        public ReactiveCommand AdmitMemo { get; }
        public ReactiveCommand DismissMemo { get; }

        public IReactiveProperty<long> Sum { get; set; }
        public ISumEditViewModel SumEdit { get; }
        public IReactiveProperty<bool> HasSum { get; }
        public ReactiveCommand AdmitSum { get; }
        public ReactiveCommand DismissSum { get; }

        public bool IsDateFormatLong => Settings.Default.Culture_DefaultDateLong;

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
