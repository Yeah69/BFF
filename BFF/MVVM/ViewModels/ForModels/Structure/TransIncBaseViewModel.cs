using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITransIncBaseViewModel : ITitBaseViewModel, IHavePayeeViewModel
    {
        /// <summary>
        /// The assigned Account, where this Transaction/Income is registered.
        /// </summary>
        IReactiveProperty<IAccountViewModel> Account { get; }
        INewPayeeViewModel NewPayeeViewModel { get; }
    }

    /// <summary>
    /// Base class for ViewModels of the models Transaction, Income, ParentTransaction and ParentIncome.
    /// </summary>
    public abstract class TransIncBaseViewModel : TitBaseViewModel, ITransIncBaseViewModel
    {
        private readonly PayeeViewModelService _payeeViewModelService;

        public IObservableReadOnlyList<IAccountViewModel> AllAccounts => CommonPropertyProvider.AllAccountViewModels;

        /// <summary>
        /// The assigned Account, where this Transaction/Income is registered.
        /// </summary>
        public virtual IReactiveProperty<IAccountViewModel> Account { get; }

        public INewPayeeViewModel NewPayeeViewModel { get; }

        /// <summary>
        /// Someone or something, who got paid or paid the user by the Transaction/Income.
        /// </summary>
        public virtual IReactiveProperty<IPayeeViewModel> Payee { get; }

        /// <summary>
        /// Initializes a TransIncBaseViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="parentTransIncBase">The model.</param>
        /// <param name="accountViewModelService">Service of accounts.</param>
        /// <param name="payeeViewModelService">Service of payees.</param>
        protected TransIncBaseViewModel(
            IBffOrm orm, 
            ITransIncBase parentTransIncBase,
            Func<IHavePayeeViewModel, INewPayeeViewModel> newPayeeViewModelFactory,
            AccountViewModelService accountViewModelService,
            PayeeViewModelService payeeViewModelService) : base(orm, parentTransIncBase)
        {
            void RefreshAnAccountViewModel(IAccountViewModel account)
            {
                account?.RefreshTits();
                account?.RefreshBalance();
            }

            _payeeViewModelService = payeeViewModelService;

            Account = parentTransIncBase.ToReactivePropertyAsSynchronized(
                tib => tib.Account,
                accountViewModelService.GetViewModel, 
                accountViewModelService.GetModel);

            Account
                .SkipLast(1)
                .Subscribe(RefreshAnAccountViewModel)
                .AddTo(CompositeDisposable);

            Account
                .Skip(1)
                .Subscribe(RefreshAnAccountViewModel)
                .AddTo(CompositeDisposable);

            Payee = parentTransIncBase.ToReactivePropertyAsSynchronized(
                tib => tib.Payee,
                payeeViewModelService.GetViewModel,
                payeeViewModelService.GetModel);

            NewPayeeViewModel = newPayeeViewModelFactory(this);
        }

        protected override void InitializeDeleteCommand()
        {
            DeleteCommand.Subscribe(_ =>
            {
                Delete();
                NotifyRelevantAccountsToRefreshTits();
                NotifyRelevantAccountsToRefreshBalance();
            });
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
