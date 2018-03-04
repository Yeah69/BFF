using System;
using System.Reactive.Linq;
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
        INewPayeeViewModel NewPayeeViewModel { get; }
    }

    /// <summary>
    /// Base class for ViewModels of the models Transaction, Income, ParentTransaction and ParentIncome.
    /// </summary>
    public abstract class TransactionBaseViewModel : TransBaseViewModel, ITransactionBaseViewModel
    {
        private readonly IAccountViewModelService _accountViewModelService;
         
        public IObservableReadOnlyList<IAccountViewModel> AllAccounts => _accountViewModelService.All;

        /// <summary>
        /// The assigned Account, where this Transaction is registered.
        /// </summary>
        public virtual IReactiveProperty<IAccountViewModel> Account { get; }

        public INewPayeeViewModel NewPayeeViewModel { get; }

        /// <summary>
        /// Someone or something, who got paid or paid the user by the Transaction.
        /// </summary>
        public virtual IReactiveProperty<IPayeeViewModel> Payee { get; }

        /// <summary>
        /// Initializes a TransIncBaseViewModel.
        /// </summary>
        /// <param name="parentTransactionBase">The model.</param>
        /// <param name="newPayeeViewModelFactory">Creates a new payee.</param>
        /// <param name="accountViewModelService">Service of accounts.</param>
        /// <param name="createSumEdit">Creates a sum editing viewmodel.</param>
        /// <param name="payeeViewModelService">Service of payees.</param>
        /// <param name="flagViewModelService">Fetches flags.</param>
        protected TransactionBaseViewModel(
            ITransactionBase parentTransactionBase,
            Func<IHavePayeeViewModel, INewPayeeViewModel> newPayeeViewModelFactory,
            Func<IHaveFlagViewModel, INewFlagViewModel> newFlagViewModelFactory,
            IAccountViewModelService accountViewModelService,
            IPayeeViewModelService payeeViewModelService,
            IFlagViewModelService flagViewModelService) 
            : base(parentTransactionBase, newFlagViewModelFactory, flagViewModelService)
        {
            _accountViewModelService = accountViewModelService;

            void RefreshAnAccountViewModel(IAccountViewModel account)
            {
                account?.RefreshTits();
                account?.RefreshBalance();
            }

            Account = parentTransactionBase.ToReactivePropertyAsSynchronized(
                tib => tib.Account,
                accountViewModelService.GetViewModel, 
                accountViewModelService.GetModel, 
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            Account
                .SkipLast(1)
                .Subscribe(RefreshAnAccountViewModel)
                .AddTo(CompositeDisposable);

            Account
                .Subscribe(RefreshAnAccountViewModel)
                .AddTo(CompositeDisposable);

            Payee = parentTransactionBase.ToReactivePropertyAsSynchronized(
                tib => tib.Payee,
                payeeViewModelService.GetViewModel,
                payeeViewModelService.GetModel, 
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            NewPayeeViewModel = newPayeeViewModelFactory(this);
        }

        public override void Delete()
        {
            base.Delete();
            NotifyRelevantAccountsToRefreshTits();
            NotifyRelevantAccountsToRefreshBalance();
        }

        protected override void NotifyRelevantAccountsToRefreshTits()
        {
            Account.Value?.RefreshTits();
            Messenger.Default.Send(SummaryAccountMessage.RefreshTits);
        }

        protected override void NotifyRelevantAccountsToRefreshBalance()
        {
            Account.Value?.RefreshBalance();
            Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
        }
    }
}
