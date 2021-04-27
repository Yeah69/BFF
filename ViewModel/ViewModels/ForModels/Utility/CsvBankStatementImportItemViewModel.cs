using BFF.ViewModel.Extensions;
using System;
using System.Linq;
using System.Reactive.Disposables;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Services;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.Windows;
using MuVaViMo;
using Reactive.Bindings;
using System.Windows.Input;

namespace BFF.ViewModel.ViewModels.ForModels.Utility
{
    public interface ICsvBankStatementImportItemViewModel : IDisposable
    {
        DateTime Date { get; }

        bool HasDate { get; }

        ICommand AdmitDate { get; }

        ICommand DismissDate { get; }

        bool ShowLongDate { get; }

        string? Payee { get; }

        bool HasPayee { get; }

        bool CreatePayeeIfNotExisting { get; }

        ICommand AdmitPayee { get; }

        ICommand DismissPayee { get; }

        bool PayeeExists { get; }

        IObservableReadOnlyList<IPayeeViewModel>? ExistingPayees { get; }

        string Memo { get; }

        bool HasMemo { get; }

        ICommand AdmitMemo { get; }

        ICommand DismissMemo { get; }

        IReactiveProperty<long> Sum { get; }

        ISumEditViewModel SumEdit { get; }

        IReactiveProperty<bool> HasSum { get; }

        ICommand AdmitSum { get; }

        ICommand DismissSum { get; }
    }

    public class CsvBankStatementImportItemViewModel : ViewModelBase, ICsvBankStatementImportItemViewModel
    {
        private readonly IBffSettings _bffSettings;
        private readonly CompositeDisposable _compositeDisposable = new();
        private bool _hasDate;
        private bool _hasPayee;
        private bool _hasMemo;
        private bool _payeeExists;
        private string? _payee;

        public CsvBankStatementImportItemViewModel(
            (DateTime? Date, string Payee, bool CreatePayeeIfNotExisting, string Memo, long? Sum) configuration,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            IBffSettings bffSettings,
            IPayeeViewModelService payeeService)
        {
            _bffSettings = bffSettings;
            Date     = configuration.Date ?? DateTime.Today;
            Payee    = configuration.Payee;
            Memo     = configuration.Memo;
            Sum      = new ReactivePropertySlim<long>(configuration.Sum ?? 0L, ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(_compositeDisposable);
            HasDate  = configuration.Date.HasValue;
            HasPayee = configuration.Payee is not null;
            HasMemo  = configuration.Memo is not null;
            HasSum   = new ReactivePropertySlim<bool>(configuration.Sum.HasValue, ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(_compositeDisposable);

            SumEdit = createSumEdit(Sum).CompositeDisposalWith(_compositeDisposable);

            this
                .ObservePropertyChanged(nameof(Payee))
                .Subscribe(_ => PayeeExists = payeeService.All?.Any(payee => payee.Name == Payee) ?? false)
                .CompositeDisposalWith(_compositeDisposable);

            CreatePayeeIfNotExisting = configuration.CreatePayeeIfNotExisting;

            ExistingPayees = payeeService.All;

            AdmitDate = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    _compositeDisposable,
                    () => HasDate = true);
            
            AdmitPayee = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    _compositeDisposable,
                    () => HasPayee = true);
            
            AdmitMemo = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    _compositeDisposable,
                    () => HasMemo = true);
            
            AdmitSum = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    _compositeDisposable,
                    () => HasSum.Value = true);
            
            DismissDate = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    _compositeDisposable,
                    () => HasDate = false);
            
            DismissPayee = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    _compositeDisposable,
                    () => HasPayee = false);
            
            DismissMemo = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    _compositeDisposable,
                    () => HasMemo = false);
            
            DismissSum = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    _compositeDisposable,
                    () => HasSum.Value = false);
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

        public ICommand AdmitDate { get; }
        public ICommand DismissDate { get; }

        public string? Payee
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
        public ICommand AdmitPayee { get; }
        public ICommand DismissPayee { get; }

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

        public IObservableReadOnlyList<IPayeeViewModel>? ExistingPayees { get; }

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

        public ICommand AdmitMemo { get; }
        public ICommand DismissMemo { get; }

        public IReactiveProperty<long> Sum { get; set; }
        public ISumEditViewModel SumEdit { get; }
        public IReactiveProperty<bool> HasSum { get; }
        public ICommand AdmitSum { get; }
        public ICommand DismissSum { get; }

        public bool ShowLongDate => _bffSettings.Culture_DefaultDateLong;

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
