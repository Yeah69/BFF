using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.Dialogs;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels.ForModels
{
    public interface IAccountViewModel : IAccountBaseViewModel
    {
        DateTime StartingDate { get; set; }

        ISumEditViewModel StartingBalanceEdit { get; }
    }

    public interface IImportCsvBankStatement
    {
        IRxRelayCommand ImportCsvBankStatement { get; }
    }

    /// <summary>
    /// Trans' can be added to an Account
    /// </summary>
    internal class AccountViewModel : AccountBaseViewModel, IAccountViewModel, IImportCsvBankStatement
    {
        private readonly IAccount _account;
        private readonly ISummaryAccountViewModel _summaryAccountViewModel;
        private readonly IMainBffDialogCoordinator _mainBffDialogCoordinator;
        private readonly IConvertFromTransBaseToTransLikeViewModel _convertFromTransBaseToTransLikeViewModel;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ILocalizer _localizer;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public sealed override IReactiveProperty<long> StartingBalance { get; }


        public DateTime StartingDate
        {
            get => _account.StartingDate;
            set => _account.StartingDate = value;
        }
        public ISumEditViewModel StartingBalanceEdit { get; }

        /// <summary>
        /// Uses the OR mapper to insert the model into the database. Inner function for the Insert method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        public override async Task InsertAsync()
        {
            await base.InsertAsync();
            _summaryAccountViewModel.RefreshStartingBalance();
            _summaryAccountViewModel.RefreshBalance();
        }
        
        public AccountViewModel(
            IAccount account, 
            ISummaryAccountViewModel summaryAccountViewModel,
            Lazy<IAccountViewModelService> accountViewModelService,
            Lazy<IPayeeViewModelService> payeeService,
            Func<IPayee> payeeFactory,
            Func<ITransLikeViewModelPlaceholder> placeholderFactory,
            IMainBffDialogCoordinator mainBffDialogCoordinator,
            IConvertFromTransBaseToTransLikeViewModel convertFromTransBaseToTransLikeViewModel,
            IRxSchedulerProvider rxSchedulerProvider,
            IBackendCultureManager cultureManager,
            ILocalizer localizer,
            IBffChildWindowManager childWindowManager,
            ITransDataGridColumnManager transDataGridColumnManager,
            IBffSettings bffSettings,
            Func<IImportCsvBankStatementViewModel> importCsvBankStatementFactory,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            Func<IAccountBaseViewModel, ITransactionViewModel> transactionViewModelFactory,
            Func<IAccountBaseViewModel, ITransferViewModel> transferViewModelFactory,
            Func<IAccountBaseViewModel, IParentTransactionViewModel> parentTransactionViewModelFactory) 
            : base(
                account,
                accountViewModelService,
                rxSchedulerProvider,
                placeholderFactory,
                bffSettings,
                cultureManager,
                transDataGridColumnManager)
        {
            _account = account;
            _summaryAccountViewModel = summaryAccountViewModel;
            _mainBffDialogCoordinator = mainBffDialogCoordinator;
            _convertFromTransBaseToTransLikeViewModel = convertFromTransBaseToTransLikeViewModel;
            _rxSchedulerProvider = rxSchedulerProvider;
            _localizer = localizer;

            StartingBalance = account
                .ToReactivePropertyAsSynchronized(a => a.StartingBalance, ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            StartingBalanceEdit = createSumEdit(StartingBalance);

            account
                .ObservePropertyChanges(nameof(account.StartingBalance))
                .Subscribe(_ =>
                {
                    summaryAccountViewModel.RefreshStartingBalance();
                    summaryAccountViewModel.RefreshBalance();
                })
                .AddTo(CompositeDisposable);

            account
                .ObservePropertyChanges(nameof(account.StartingBalance))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(StartingBalance)))
                .AddTo(CompositeDisposable);

            NewTransactionCommand = new RxRelayCommand(() =>
            {
                var transactionViewModel = transactionViewModelFactory(this);
                if (MissingSum != null) transactionViewModel.Sum.Value = (long)MissingSum;
                NewTransList.Add(transactionViewModel);
            }).AddTo(CompositeDisposable);

            NewTransferCommand = new RxRelayCommand(() => NewTransList.Add(transferViewModelFactory(this))).AddTo(CompositeDisposable);

            NewParentTransactionCommand = new RxRelayCommand(() =>
            {
                var parentTransactionViewModel = parentTransactionViewModelFactory(this);
                NewTransList.Add(parentTransactionViewModel);
                if (MissingSum != null)
                {
                    parentTransactionViewModel.NewSubTransactionCommand?.Execute(null);
                    parentTransactionViewModel.NewSubTransactions.First().Sum.Value = (long)MissingSum;
                }
            }).AddTo(CompositeDisposable);

            ApplyCommand = new AsyncRxRelayCommand(async () => await ApplyTrans(), 
                NewTransList
                    .ToReadOnlyReactivePropertyAsSynchronized(collection => collection.Count)
                    .Select(count => count > 0),
                NewTransList.Count > 0);

            ImportCsvBankStatement = new AsyncRxRelayCommand(async () =>
            {
                var importCsvBankStatementViewModel = importCsvBankStatementFactory();
                await childWindowManager.OpenOkCancelDialog(importCsvBankStatementViewModel);
                foreach (var item in importCsvBankStatementViewModel.Items)
                {
                    var transactionViewModel = transactionViewModelFactory(this);

                    if (item.HasDate)
                        transactionViewModel.Date = item.Date;
                    if (item.HasPayee)
                    {
                        if (payeeService.Value.All.Any(p => p.Name == item.Payee))
                            transactionViewModel.Payee =
                                payeeService.Value.All.FirstOrDefault(p => p.Name == item.Payee);
                        else if (item.CreatePayeeIfNotExisting)
                        {
                            IPayee newPayee = payeeFactory();
                            newPayee.Name = item.Payee.Trim();
                            await newPayee.InsertAsync();
                            transactionViewModel.Payee = payeeService.Value.GetViewModel(newPayee);
                        }
                    }

                    if (item.HasMemo)
                        transactionViewModel.Memo = item.Memo;
                    if (item.HasSum.Value)
                        transactionViewModel.Sum.Value = item.Sum.Value;

                    NewTransList.Add(transactionViewModel);
                }
            });

            if (BffSettings.OpenAccountTab == Name)
                IsOpen = true;
        }

        protected override async Task<ITransLikeViewModel[]> PageFetcher (int offset, int pageSize)
        {
            var transLikeViewModels = _convertFromTransBaseToTransLikeViewModel
                .Convert(await _account.GetTransPageAsync(offset, pageSize), this)
                .ToArray();
            return transLikeViewModels;
        }

        protected override async Task<int> CountFetcher ()
            => (int) await _account.GetTransCountAsync();

        protected override long? CalculateNewPartOfIntermediateBalance()
        {
            long? sum = 0;
            foreach (var transLikeViewModel in NewTransList)
            {
                switch (transLikeViewModel)
                {
                    case TransferViewModel transfer:
                        sum += transfer.ToAccount == this ? transfer.SumAbsolute : -transfer.SumAbsolute;
                        break;
                    case ParentTransactionViewModel parent:
                        sum += parent.TotalSum.Value;
                        break;
                    default:
                        sum += transLikeViewModel.Sum.Value;
                        break;
                }
            }

            return sum;
        }

        public override Task DeleteAsync()
        {
            TaskCompletionSource<Unit> source = new TaskCompletionSource<Unit>();
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    _localizer.Localize("ConfirmationDialog_Title"),
                    _localizer.Localize("Account_Delete_ConfirmationMessage"),
                    BffMessageDialogStyle.AffirmativeAndNegative)
                .ToObservable()
                .ObserveOn(_rxSchedulerProvider.UI)
                .Subscribe(async r =>
                {
                    if (r == BffMessageDialogResult.Affirmative)
                    {
                        await base.DeleteAsync();
                        foreach (var accountViewModel in AllAccounts)
                        {
                            accountViewModel.RefreshTransCollection();
                            accountViewModel.RefreshBalance();
                        }
                        _summaryAccountViewModel.RefreshStartingBalance();
                        _summaryAccountViewModel.RefreshBalance();
                        _summaryAccountViewModel.RefreshTransCollection();
                    }
                    source.SetResult(Unit.Default);
                });
            return source.Task;
        }
        
        public sealed override IRxRelayCommand NewTransactionCommand { get; }
        
        public sealed override IRxRelayCommand NewTransferCommand { get; }
        
        public sealed override IRxRelayCommand NewParentTransactionCommand { get; }
        
        public sealed override IRxRelayCommand ApplyCommand { get; }

        public override IRxRelayCommand ImportCsvBankStatement { get; }
    }
}
