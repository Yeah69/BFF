using System.Linq;
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
    public interface ITransIncBaseViewModel : ITitBaseViewModel
    {
        /// <summary>
        /// The assigned Account, where this Transaction/Income is registered.
        /// </summary>
        IReactiveProperty<IAccountViewModel> Account { get; }

        /// <summary>
        /// Someone or something, who got paid or paid the user by the Transaction/Income.
        /// </summary>
        IReactiveProperty<IPayeeViewModel> Payee { get; }
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

        /// <summary>
        /// Someone or something, who got paid or paid the user by the Transaction/Income.
        /// </summary>
        public virtual IReactiveProperty<IPayeeViewModel> Payee { get; }

        /// <summary>
        /// Initializes a TransIncBaseViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="transIncBase">The model.</param>
        /// <param name="accountViewModelService">Service of accounts.</param>
        /// <param name="payeeViewModelService">Service of payees.</param>
        protected TransIncBaseViewModel(
            IBffOrm orm, 
            ITransIncBase transIncBase, 
            AccountViewModelService accountViewModelService,
            PayeeViewModelService payeeViewModelService) : base(orm, transIncBase)
        {
            _payeeViewModelService = payeeViewModelService;

            Account = transIncBase.ToReactivePropertyAsSynchronized(
                tib => tib.Account,
                accountViewModelService.GetViewModel, 
                accountViewModelService.GetModel);

            Payee = transIncBase.ToReactivePropertyAsSynchronized(
                tib => tib.Payee,
                payeeViewModelService.GetViewModel,
                payeeViewModelService.GetModel);
        }

        #region Payee Editing

        /// <summary>
        /// User input of the to be searched or to be created Payee.
        /// </summary>
        public string PayeeText { get; set; }

        /// <summary>
        /// Creates a new Payee.
        /// </summary>
        public ICommand AddPayeeCommand => new RelayCommand(obj =>
        {
            IPayee newPayee = Orm.BffRepository.PayeeRepository.Create();
            newPayee.Name = PayeeText.Trim();
            newPayee.Insert();
            OnPropertyChanged(nameof(AllPayees));
            Payee.Value = _payeeViewModelService.GetViewModel(newPayee);
        }, obj =>
        {
            string trimmedPayeeText = PayeeText?.Trim();
            return !string.IsNullOrEmpty(trimmedPayeeText) &&
                   AllPayees.Count(payee => payee.Name.Value == trimmedPayeeText) == 0;
        });

        /// <summary>
        /// All currently available Payees.
        /// </summary>
        public IObservableReadOnlyList<IPayeeViewModel> AllPayees => CommonPropertyProvider?.AllPayeeViewModels;

        #endregion

        /// <summary>
        /// Deletes the object from the database and refreshes the account, which it belonged to, and the summary account.
        /// </summary>
        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            Delete();
            Messenger.Default.Send(SummaryAccountMessage.Refresh);
            Messenger.Default.Send(AccountMessage.Refresh, Account);
        });
    }
}
