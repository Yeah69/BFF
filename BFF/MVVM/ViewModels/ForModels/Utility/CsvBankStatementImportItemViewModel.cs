using System;
using System.Linq;
using System.Reactive.Disposables;
using BFF.Helper.Extensions;
using BFF.MVVM.Services;
using BFF.Properties;
using MuVaViMo;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels.Utility
{
    public interface ICsvBankStatementImportItemViewModel : IDisposable
    {
        DateTime Date { get; }

        bool HasDate { get; }

        IRxRelayCommand AdmitDate { get; }

        IRxRelayCommand DismissDate { get; }

        bool ShowLongDate { get; }

        string Payee { get; }

        bool HasPayee { get; }

        bool CreatePayeeIfNotExisting { get; }

        IRxRelayCommand AdmitPayee { get; }

        IRxRelayCommand DismissPayee { get; }

        bool PayeeExists { get; }

        IObservableReadOnlyList<IPayeeViewModel> ExistingPayees { get; }

        string Memo { get; }

        bool HasMemo { get; }

        IRxRelayCommand AdmitMemo { get; }

        IRxRelayCommand DismissMemo { get; }

        IReactiveProperty<long> Sum { get; }

        ISumEditViewModel SumEdit { get; }

        IReactiveProperty<bool> HasSum { get; }

        IRxRelayCommand AdmitSum { get; }

        IRxRelayCommand DismissSum { get; }
    }

    public class CsvBankStatementImportItemViewModel : ViewModelBase, ICsvBankStatementImportItemViewModel
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private bool _hasDate;
        private bool _hasPayee;
        private bool _hasMemo;
        private bool _payeeExists;
        private string _payee;

        public CsvBankStatementImportItemViewModel(
            (DateTime? Date, string Payee, bool CreatePayeeIfNotExisting, string Memo, long? Sum) configuration,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            IPayeeViewModelService payeeService)
        {
            Date     = configuration.Date ?? DateTime.Today;
            Payee    = configuration.Payee;
            Memo     = configuration.Memo;
            Sum      = new ReactivePropertySlim<long>(configuration.Sum ?? 0L, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            HasDate  = configuration.Date.HasValue;
            HasPayee = configuration.Payee != null;
            HasMemo  = configuration.Memo != null;
            HasSum   = new ReactivePropertySlim<bool>(configuration.Sum.HasValue, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);

            SumEdit = createSumEdit(Sum).AddHere(_compositeDisposable);

            AdmitDate = new RxRelayCommand(() => HasDate = true)
                .AddHere(_compositeDisposable);

            AdmitPayee = new RxRelayCommand(() => HasPayee = true)
                .AddHere(_compositeDisposable);

            this
                .ObservePropertyChanges(nameof(Payee))
                .Subscribe(_ => PayeeExists = payeeService.All.Any(payee => payee.Name == Payee))
                .AddHere(_compositeDisposable);

            CreatePayeeIfNotExisting = configuration.CreatePayeeIfNotExisting;

            ExistingPayees = payeeService.All;

            AdmitMemo = new RxRelayCommand(() => HasMemo = true)
                .AddHere(_compositeDisposable);

            AdmitSum = new RxRelayCommand(() => HasSum.Value = true)
                .AddHere(_compositeDisposable);

            DismissDate = new RxRelayCommand(() => HasDate = false)
                .AddHere(_compositeDisposable);

            DismissPayee = new RxRelayCommand(() => HasPayee = false)
                .AddHere(_compositeDisposable);

            DismissMemo = new RxRelayCommand(() => HasMemo = false)
                .AddHere(_compositeDisposable);

            DismissSum = new RxRelayCommand(() => HasSum.Value = false)
                .AddHere(_compositeDisposable);
        }

        public DateTime Date { get; set; }

        public bool HasDate
        {
            get => _hasDate;
            private set
            {
                if (_hasDate == value) return;
                _hasDate = value;
                OnPropertyChanged();
            }
        }

        public IRxRelayCommand AdmitDate { get; }
        public IRxRelayCommand DismissDate { get; }

        public string Payee
        {
            get => _payee;
            set
            {
                if (_payee == value) return;
                _payee = value;
                OnPropertyChanged();
            }
        }

        public bool HasPayee
        {
            get => _hasPayee;
            private set
            {
                if (_hasPayee == value) return;
                _hasPayee = value;
                OnPropertyChanged();
            }
        }

        public bool CreatePayeeIfNotExisting { get; }
        public IRxRelayCommand AdmitPayee { get; }
        public IRxRelayCommand DismissPayee { get; }

        public bool PayeeExists
        {
            get => _payeeExists;
            private set
            {
                if (_payeeExists == value) return;
                _payeeExists = value;
                OnPropertyChanged();
            }
        }

        public IObservableReadOnlyList<IPayeeViewModel> ExistingPayees { get; }

        public string Memo { get; set; }

        public bool HasMemo
        {
            get => _hasMemo;
            private set
            {
                if (_hasMemo == value) return;
                _hasMemo = value;
                OnPropertyChanged();
            }
        }

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
