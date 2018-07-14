using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Managers;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.Dialogs;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.MVVM.ViewModels.ForModels.Utility;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
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
    /// Tits can be added to an Account
    /// </summary>
    public class AccountViewModel : AccountBaseViewModel, IAccountViewModel, IImportCsvBankStatement
    {
        private readonly IAccount _account;
        private readonly ISummaryAccountViewModel _summaryAccountViewModel;
        private readonly ITransRepository _transRepository;
        private readonly Func<ITransLikeViewModelPlaceholder> _placeholderFactory;
        private readonly IMainBffDialogCoordinator _mainBffDialogCoordinator;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;

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
            ITransRepository transRepository,
            IAccountRepository accountRepository,
            Lazy<IAccountViewModelService> accountViewModelService,
            Lazy<IPayeeViewModelService> payeeService,
            Func<IPayee> payeeFactory,
            Func<ITransLikeViewModelPlaceholder> placeholderFactory,
            IMainBffDialogCoordinator mainBffDialogCoordinator,
            IRxSchedulerProvider rxSchedulerProvider,
            IBackendCultureManager cultureManager,
            IBffChildWindowManager childWindowManager,
            IAccountModuleColumnManager accountModuleColumnManager,
            Func<Action<IList<ICsvBankStatementImportItemViewModel>>, IImportCsvBankStatementViewModel> importCsvBankStatementFactory,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            Func<IAccountBaseViewModel, ITransactionViewModel> transactionViewModelFactory,
            Func<IAccountBaseViewModel, ITransferViewModel> transferViewModelFactory,
            Func<IAccountBaseViewModel, IParentTransactionViewModel> parentTransactionViewModelFactory,
            Func<ITransaction, IAccountBaseViewModel, ITransactionViewModel> dependingTransactionViewModelFactory,
            Func<IParentTransaction, IAccountBaseViewModel, IParentTransactionViewModel> dependingParentTransactionViewModelFactory,
            Func<ITransfer, IAccountBaseViewModel, ITransferViewModel> dependingTransferViewModelFactory) 
            : base(
                account,
                accountViewModelService,
                accountRepository,
                rxSchedulerProvider,
                cultureManager,
                dependingTransactionViewModelFactory, 
                dependingParentTransactionViewModelFactory,
                dependingTransferViewModelFactory,
                accountModuleColumnManager)
        {
            _account = account;
            _summaryAccountViewModel = summaryAccountViewModel;
            _transRepository = transRepository;
            _placeholderFactory = placeholderFactory;
            _mainBffDialogCoordinator = mainBffDialogCoordinator;
            _rxSchedulerProvider = rxSchedulerProvider;

            StartingBalance = account
                .ToReactivePropertyAsSynchronized(a => a.StartingBalance, ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            StartingBalanceEdit = createSumEdit(StartingBalance);

            account
                .ObservePropertyChanges(a => a.StartingBalance)
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

            NewTransactionCommand = new RxRelayCommand(() => NewTransList.Add(transactionViewModelFactory(this))).AddTo(CompositeDisposable);

            NewTransferCommand = new RxRelayCommand(() => NewTransList.Add(transferViewModelFactory(this))).AddTo(CompositeDisposable);

            NewParentTransactionCommand = new RxRelayCommand(() => NewTransList.Add(parentTransactionViewModelFactory(this))).AddTo(CompositeDisposable);

            ApplyCommand = new AsyncRxRelayCommand(async () => await ApplyTits(), 
                NewTransList
                    .ToReadOnlyReactivePropertyAsSynchronized(collection => collection.Count)
                    .Select(count => count > 0),
                NewTransList.Count > 0);

            ImportCsvBankStatement = new AsyncRxRelayCommand(async () => await childWindowManager.OpenImportCsvBankStatementDialogAsync(importCsvBankStatementFactory(
                async items =>
                {
                    foreach (var item in items)
                    {
                        var transactionViewModel = transactionViewModelFactory(this);

                        if (item.HasDate.Value)
                            transactionViewModel.Date = item.Date.Value;
                        if (item.HasPayee.Value)
                        {
                            if (payeeService.Value.All.Any(p => p.Name == item.Payee.Value))
                                transactionViewModel.Payee = payeeService.Value.All.FirstOrDefault(p => p.Name == item.Payee.Value);
                            else if (item.CreatePayeeIfNotExisting.Value)
                            {
                                IPayee newPayee = payeeFactory();
                                newPayee.Name = item.Payee.Value.Trim();
                                await newPayee.InsertAsync();
                                transactionViewModel.Payee = payeeService.Value.GetViewModel(newPayee);
                            }
                        }
                        if (item.HasMemo.Value)
                            transactionViewModel.Memo = item.Memo.Value;
                        if (item.HasSum.Value)
                            transactionViewModel.Sum.Value = item.Sum.Value;

                        NewTransList.Add(transactionViewModel);
                    }
                })));
        }

        protected override IBasicTaskBasedAsyncDataAccess<ITransLikeViewModel> BasicAccess
            => new RelayBasicTaskBasedAsyncDataAccess<ITransLikeViewModel>(
                async (offset, pageSize) => CreatePacket( await _transRepository.GetPageAsync(offset, pageSize, _account)),
                async () => (int) await _transRepository.GetCountAsync(_account),
                () => _placeholderFactory());

        public override Task DeleteAsync()
        {
            TaskCompletionSource<Unit> source = new TaskCompletionSource<Unit>();
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    "ConfirmationDialog_Title".Localize(),
                    "Account_Delete_ConfirmationMessage".Localize(),
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
