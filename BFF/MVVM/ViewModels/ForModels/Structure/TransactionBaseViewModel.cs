using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Core;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using MuVaViMo;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITransactionBaseViewModel : ITransBaseViewModel, IHavePayeeViewModel
    {
        IObservableReadOnlyList<IAccountViewModel> AllAccounts { get; }
        /// <summary>
        /// The assigned Account, where this Transaction is registered.
        /// </summary>
        IAccountViewModel Account { get; set; }
    }

    /// <summary>
    /// Base class for ViewModels of the models Transaction, Income, ParentTransaction and ParentIncome.
    /// </summary>
    public abstract class TransactionBaseViewModel : TransBaseViewModel, ITransactionBaseViewModel
    {
        private readonly ITransactionBase _transactionBase;
        private readonly IAccountViewModelService _accountViewModelService;
        private readonly IPayeeViewModelService _payeeViewModelService;
        private readonly ISummaryAccountViewModel _summaryAccountViewModel;
        private IAccountViewModel _account;
        private IPayeeViewModel _payee;

        public IObservableReadOnlyList<IAccountViewModel> AllAccounts => _accountViewModelService.All;

        /// <summary>
        /// The assigned Account, where this Transaction is registered.
        /// </summary>
        public IAccountViewModel Account
        {
            get => _account;
            set => _transactionBase.Account = _accountViewModelService.GetModel(value);
        }

        public INewPayeeViewModel NewPayeeViewModel { get; }

        /// <summary>
        /// Someone or something, who got paid or paid the user by the Transaction.
        /// </summary>
        public IPayeeViewModel Payee
        {
            get => _payee;
            set => _transactionBase.Payee = _payeeViewModelService.GetModel(value);
        }

        protected TransactionBaseViewModel(
            ITransactionBase transactionBase,
            INewPayeeViewModel newPayeeViewModel,
            INewFlagViewModel newFlagViewModel,
            IAccountViewModelService accountViewModelService,
            IPayeeViewModelService payeeViewModelService,
            ILastSetDate lastSetDate,
            IRxSchedulerProvider rxSchedulerProvider,
            ISummaryAccountViewModel summaryAccountViewModel,
            IFlagViewModelService flagViewModelService,
            IAccountBaseViewModel owner) 
            : base(
                transactionBase, 
                newFlagViewModel, 
                lastSetDate, 
                rxSchedulerProvider, 
                flagViewModelService, 
                owner)
        {
            _transactionBase = transactionBase;
            _accountViewModelService = accountViewModelService;
            _payeeViewModelService = payeeViewModelService;
            _summaryAccountViewModel = summaryAccountViewModel;

            void RefreshAnAccountViewModel(IAccountViewModel account)
            {
                account?.RefreshTransCollection();
                account?.RefreshBalance();
            }

            _account = _accountViewModelService.GetViewModel(transactionBase.Account);
            transactionBase
                .ObservePropertyChanges(nameof(transactionBase.Account))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ =>
                {
                    _account = _accountViewModelService.GetViewModel(transactionBase.Account);
                    OnPropertyChanged(nameof(Account));
                    if (_account != null)
                    {
                        ClearErrors(nameof(Account));
                        OnErrorChanged(nameof(Account));
                    }
                })
                .AddTo(CompositeDisposable);

            if (Account is null && owner is IAccountViewModel specificAccount)
                Account = specificAccount;

            transactionBase
                .ObservePropertyChanges(nameof(transactionBase.Account))
                .SkipLast(1)
                .Subscribe(a => RefreshAnAccountViewModel(accountViewModelService.GetViewModel(transactionBase.Account)))
                .AddTo(CompositeDisposable);

            transactionBase
                .ObservePropertyChanges(nameof(transactionBase.Account))
                .Subscribe(a => RefreshAnAccountViewModel(accountViewModelService.GetViewModel(transactionBase.Account)))
                .AddTo(CompositeDisposable);

            _payee = _payeeViewModelService.GetViewModel(transactionBase.Payee);
            transactionBase
                .ObservePropertyChanges(nameof(transactionBase.Payee))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ =>
                {
                    _payee = _payeeViewModelService.GetViewModel(transactionBase.Payee);
                    OnPropertyChanged(nameof(Payee));
                    if (_payee != null)
                    {
                        ClearErrors(nameof(Payee));
                        OnErrorChanged(nameof(Payee));
                    }
                })
                .AddTo(CompositeDisposable);

            NewPayeeViewModel = newPayeeViewModel;
        }

        public override bool IsInsertable() => base.IsInsertable() && Account.IsNotNull() && Payee.IsNotNull();

        public override async Task DeleteAsync()
        {
            await base.DeleteAsync();
            NotifyRelevantAccountsToRefreshTrans();
            NotifyRelevantAccountsToRefreshBalance();
        }

        public override void NotifyErrorsIfAny()
        {
            if (Account is null)
            {
                SetErrors("ErrorMessageEmptyAccount".Localize().ToEnumerable(), nameof(Account));
                OnErrorChanged(nameof(Account));
            }

            if (!(Payee is null)) return;

            SetErrors("ErrorMessageEmptyPayee".Localize().ToEnumerable(), nameof(Payee));
            OnErrorChanged(nameof(Payee));
        }

        protected override void NotifyRelevantAccountsToRefreshTrans()
        {
            Account?.RefreshTransCollection();
            _summaryAccountViewModel.RefreshTransCollection();
        }

        protected override void NotifyRelevantAccountsToRefreshBalance()
        {
            Account?.RefreshBalance();
            _summaryAccountViewModel.RefreshBalance();
        }
    }
}
