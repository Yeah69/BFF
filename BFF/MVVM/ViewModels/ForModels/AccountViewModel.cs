using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
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
        IReactiveProperty<DateTime> StartingDate { get; }

        ISumEditViewModel StartingBalanceEdit { get; }
    }

    public interface IImportCsvBankStatement
    {
        ReactiveCommand ImportCsvBankStatement { get; }
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
        private readonly IRxSchedulerProvider _schedulerProvider;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public sealed override IReactiveProperty<long> StartingBalance { get; }


        public IReactiveProperty<DateTime> StartingDate { get; }
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
            IPayeeViewModelService payeeService,
            Func<IPayee> payeeFactory,
            Func<ITransLikeViewModelPlaceholder> placeholderFactory,
            IMainBffDialogCoordinator mainBffDialogCoordinator,
            IRxSchedulerProvider schedulerProvider,
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
                schedulerProvider,
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
            _schedulerProvider = schedulerProvider;

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

            StartingDate = account
                .ToReactivePropertyAsSynchronized(a => a.StartingDate, ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            NewTransactionCommand.Subscribe(_ => NewTransList.Add(transactionViewModelFactory(this))).AddTo(CompositeDisposable);

            NewTransferCommand.Subscribe(_ => NewTransList.Add(transferViewModelFactory(this))).AddTo(CompositeDisposable);

            NewParentTransactionCommand.Subscribe(_ => NewTransList.Add(parentTransactionViewModelFactory(this))).AddTo(CompositeDisposable);

            ApplyCommand = NewTransList.ToReadOnlyReactivePropertyAsSynchronized(collection => collection.Count)
                .Select(count => count > 0)
                .ToReactiveCommand();

            ApplyCommand.Subscribe(async _ => await ApplyTits()).AddTo(CompositeDisposable);

            ImportCsvBankStatement.Subscribe(async _ => await childWindowManager.OpenImportCsvBankStatementDialogAsync(importCsvBankStatementFactory(
                async items =>
                {
                    foreach (var item in items)
                    {
                        var transactionViewModel = transactionViewModelFactory(this);

                        if (item.HasDate.Value)
                            transactionViewModel.Date.Value = item.Date.Value;
                        if (item.HasPayee.Value)
                        {
                            if(payeeService.All.Any(p => p.Name.Value == item.Payee.Value))
                                transactionViewModel.Payee.Value = payeeService.All.FirstOrDefault(p => p.Name.Value == item.Payee.Value);
                            else if(item.CreatePayeeIfNotExisting.Value)
                            {
                                IPayee newPayee = payeeFactory();
                                newPayee.Name = item.Payee.Value.Trim();
                                await newPayee.InsertAsync();
                                transactionViewModel.Payee.Value = payeeService.GetViewModel(newPayee);
                            }
                        }
                        if (item.HasMemo.Value)
                            transactionViewModel.Memo.Value = item.Memo.Value;
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

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        public override IDataVirtualizingCollection<ITransLikeViewModel> Tits => _tits ?? CreateDataVirtualizingCollection();

        /// <summary>
        /// Refreshes the TITs of this Account.
        /// </summary>
        public override void RefreshTits()
        {
            if(IsOpen.Value)
            {
                Task.Run(() => CreateDataVirtualizingCollection())
                    .ContinueWith(async t =>
                    {
                        var temp = _tits;
                        _tits = await t;
                        _schedulerProvider.UI.MinimalSchedule(() =>
                        {
                            OnPreVirtualizedRefresh();
                            OnPropertyChanged(nameof(Tits));
                            OnPostVirtualizedRefresh();
                            Task.Run(() => temp?.Dispose());
                        });
                        
                    });
            }
        }

        public override Task DeleteAsync()
        {
            TaskCompletionSource<Unit> source = new TaskCompletionSource<Unit>();
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    "ConfirmationDialog_Title".Localize(),
                    "Account_Delete_ConfirmationMessage".Localize(),
                    BffMessageDialogStyle.AffirmativeAndNegative)
                .ToObservable()
                .ObserveOn(_schedulerProvider.UI)
                .Subscribe(async r =>
                {
                    if (r == BffMessageDialogResult.Affirmative)
                    {
                        await base.DeleteAsync();
                        foreach (var accountViewModel in AllAccounts)
                        {
                            accountViewModel.RefreshTits();
                            accountViewModel.RefreshBalance();
                        }
                        _summaryAccountViewModel.RefreshStartingBalance();
                        _summaryAccountViewModel.RefreshBalance();
                        _summaryAccountViewModel.RefreshTits();
                    }
                    source.SetResult(Unit.Default);
                });
            return source.Task;
        }
        
        public sealed override ReactiveCommand NewTransactionCommand { get; } = new ReactiveCommand();
        
        public sealed override ReactiveCommand NewTransferCommand { get; } = new ReactiveCommand();
        
        public sealed override ReactiveCommand NewParentTransactionCommand { get; } = new ReactiveCommand();
        
        public sealed override ReactiveCommand ApplyCommand { get; }

        public override ReactiveCommand ImportCsvBankStatement { get; } = new ReactiveCommand();
    }
}
