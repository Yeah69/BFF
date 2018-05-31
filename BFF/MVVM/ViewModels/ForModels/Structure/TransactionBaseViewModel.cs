using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITransactionBaseViewModel : ITransBaseViewModel, IHavePayeeViewModel
    {
        IObservableReadOnlyList<IAccountViewModel> AllAccounts { get; }
        /// <summary>
        /// The assigned Account, where this Transaction is registered.
        /// </summary>
        IReactiveProperty<IAccountViewModel> Account { get; }
    }

    /// <summary>
    /// Base class for ViewModels of the models Transaction, Income, ParentTransaction and ParentIncome.
    /// </summary>
    public abstract class TransactionBaseViewModel : TransBaseViewModel, ITransactionBaseViewModel
    {
        private readonly IAccountViewModelService _accountViewModelService;
        private readonly ISummaryAccountViewModel _summaryAccountViewModel;

        public IObservableReadOnlyList<IAccountViewModel> AllAccounts => _accountViewModelService.All;

        /// <summary>
        /// The assigned Account, where this Transaction is registered.
        /// </summary>
        public IReactiveProperty<IAccountViewModel> Account { get; }

        public INewPayeeViewModel NewPayeeViewModel { get; }

        /// <summary>
        /// Someone or something, who got paid or paid the user by the Transaction.
        /// </summary>
        public virtual IReactiveProperty<IPayeeViewModel> Payee { get; }
        
        protected TransactionBaseViewModel(
            ITransactionBase transactionBase,
            INewPayeeViewModel newPayeeViewModel,
            INewFlagViewModel newFlagViewModel,
            IAccountViewModelService accountViewModelService,
            IPayeeViewModelService payeeViewModelService,
            ILastSetDate lastSetDate,
            IRxSchedulerProvider schedulerProvider,
            ISummaryAccountViewModel summaryAccountViewModel,
            IFlagViewModelService flagViewModelService,
            IAccountBaseViewModel owner) 
            : base(
                transactionBase, 
                newFlagViewModel, 
                lastSetDate, 
                schedulerProvider, 
                flagViewModelService, 
                owner)
        {
            _accountViewModelService = accountViewModelService;
            _summaryAccountViewModel = summaryAccountViewModel;

            void RefreshAnAccountViewModel(IAccountViewModel account)
            {
                account?.RefreshTits();
                account?.RefreshBalance();
            }

            Account = transactionBase
                .ToReactivePropertyAsSynchronized(
                    nameof(transactionBase.Account),
                    () => transactionBase.Account,
                    a => transactionBase.Account = a,
                    accountViewModelService.GetViewModel, 
                    accountViewModelService.GetModel,
                    schedulerProvider.UI,
                    ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            if (Account.Value is null && owner is IAccountViewModel specificAccount)
                Account.Value = specificAccount;

            transactionBase
                .ObservePropertyChanges(tb => tb.Account)
                .SkipLast(1)
                .Subscribe(a => RefreshAnAccountViewModel(accountViewModelService.GetViewModel(a)))
                .AddTo(CompositeDisposable);

            transactionBase
                .ObservePropertyChanges(tb => tb.Account)
                .Subscribe(a => RefreshAnAccountViewModel(accountViewModelService.GetViewModel(a)))
                .AddTo(CompositeDisposable);

            Payee = transactionBase
                .ToReactivePropertyAsSynchronized(
                    nameof(transactionBase.Payee),
                    () => transactionBase.Payee,
                    p => transactionBase.Payee = p,
                    payeeViewModelService.GetViewModel,
                    payeeViewModelService.GetModel,
                    schedulerProvider.UI,
                    ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            NewPayeeViewModel = newPayeeViewModel;
        }

        public override bool IsInsertable() => base.IsInsertable() && Account.Value.IsNotNull() && Payee.Value.IsNotNull();

        public override async Task DeleteAsync()
        {
            await base.DeleteAsync();
            NotifyRelevantAccountsToRefreshTits();
            NotifyRelevantAccountsToRefreshBalance();
        }

        protected override void NotifyRelevantAccountsToRefreshTits()
        {
            Account.Value?.RefreshTits();
            _summaryAccountViewModel.RefreshTits();
        }

        protected override void NotifyRelevantAccountsToRefreshBalance()
        {
            Account.Value?.RefreshBalance();
            _summaryAccountViewModel.RefreshBalance();
        }
    }
}
