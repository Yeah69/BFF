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

        IRxRelayCommand AdmitDate { get; }

        IRxRelayCommand DismissDate { get; }

        bool ShowLongDate { get; }

        IReactiveProperty<string> Payee { get; }

        IReactiveProperty<bool> HasPayee { get; }

        IReadOnlyReactiveProperty<bool> CreatePayeeIfNotExisting { get; }

        IRxRelayCommand AdmitPayee { get; }

        IRxRelayCommand DismissPayee { get; }

        IReadOnlyReactiveProperty<bool> PayeeExists { get; }

        IObservableReadOnlyList<IPayeeViewModel> ExistingPayees { get; }

        IReactiveProperty<string> Memo { get; }

        IReactiveProperty<bool> HasMemo { get; }

        IRxRelayCommand AdmitMemo { get; }

        IRxRelayCommand DismissMemo { get; }

        IReactiveProperty<long> Sum { get; }

        ISumEditViewModel SumEdit { get; }

        IReactiveProperty<bool> HasSum { get; }

        IRxRelayCommand AdmitSum { get; }

        IRxRelayCommand DismissSum { get; }
    }

    public class CsvBankStatementImportItemViewModel : ICsvBankStatementImportItemViewModel
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public CsvBankStatementImportItemViewModel(
            (DateTime? Date, string Payee, bool CreatePayeeIfNotExisting, string Memo, long? Sum) configuration,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            IPayeeViewModelService payeeService,
            IRxSchedulerProvider rxSchedulerProvider)
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

            AdmitDate = new RxRelayCommand(() => HasDate.Value = true)
                .AddHere(_compositeDisposable);

            AdmitPayee = new RxRelayCommand(() => HasPayee.Value = true)
                .AddHere(_compositeDisposable);

            PayeeExists = Payee
                .Select(p => payeeService.All.Any(payee => payee.Name == p))
                .ToReadOnlyReactivePropertySlim(payeeService.All.Any(payee => payee.Name == Payee.Value), ReactivePropertyMode.DistinctUntilChanged)
                .AddHere(_compositeDisposable);

            CreatePayeeIfNotExisting = new ReactiveProperty<bool>(configuration.CreatePayeeIfNotExisting, ReactivePropertyMode.DistinctUntilChanged);

            ExistingPayees = payeeService.All;

            AdmitMemo = new RxRelayCommand(() => HasMemo.Value = true)
                .AddHere(_compositeDisposable);

            AdmitSum = new RxRelayCommand(() => HasSum.Value = true)
                .AddHere(_compositeDisposable);

            DismissDate = new RxRelayCommand(() => HasDate.Value = false)
                .AddHere(_compositeDisposable);

            DismissPayee = new RxRelayCommand(() => HasPayee.Value = false)
                .AddHere(_compositeDisposable);

            DismissMemo = new RxRelayCommand(() => HasMemo.Value = false)
                .AddHere(_compositeDisposable);

            DismissSum = new RxRelayCommand(() => HasSum.Value = false)
                .AddHere(_compositeDisposable);
        }

        public IReactiveProperty<DateTime> Date { get; set; }
        public IReactiveProperty<bool> HasDate { get; }
        public IRxRelayCommand AdmitDate { get; }
        public IRxRelayCommand DismissDate { get; }

        public IReactiveProperty<string> Payee { get; set; }
        public IReactiveProperty<bool> HasPayee { get; }
        public IReadOnlyReactiveProperty<bool> CreatePayeeIfNotExisting { get; }
        public IRxRelayCommand AdmitPayee { get; }
        public IRxRelayCommand DismissPayee { get; }
        public IReadOnlyReactiveProperty<bool> PayeeExists { get; }
        public IObservableReadOnlyList<IPayeeViewModel> ExistingPayees { get; }

        public IReactiveProperty<string> Memo { get; set; }
        public IReactiveProperty<bool> HasMemo { get; }
        public IRxRelayCommand AdmitMemo { get; }
        public IRxRelayCommand DismissMemo { get; }

        public IReactiveProperty<long> Sum { get; set; }
        public ISumEditViewModel SumEdit { get; }
        public IReactiveProperty<bool> HasSum { get; }
        public IRxRelayCommand AdmitSum { get; }
        public IRxRelayCommand DismissSum { get; }

        public bool ShowLongDate => Settings.Default.Culture_DefaultDateLong;

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
