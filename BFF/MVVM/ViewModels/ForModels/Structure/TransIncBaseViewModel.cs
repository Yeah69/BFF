using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using MuVaViMo;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITransIncBaseViewModel : ITitBaseViewModel
    {
        /// <summary>
        /// The assigned Account, where this Transaction/Income is registered.
        /// </summary>
        IAccountViewModel Account { get; set; }

        /// <summary>
        /// Someone or something, who got paid or paid the user by the Transaction/Income.
        /// </summary>
        IPayeeViewModel Payee { get; set; }
    }

    /// <summary>
    /// Base class for ViewModels of the models Transaction, Income, ParentTransaction and ParentIncome.
    /// </summary>
    public abstract class TransIncBaseViewModel : TitBaseViewModel, ITransIncBaseViewModel
    {
        private readonly ITransIncBase _transIncBase;

        /// <summary>
        /// The assigned Account, where this Transaction/Income is registered.
        /// </summary>
        public virtual IAccountViewModel Account
        {
            get => _transIncBase.AccountId == -1
                ? null
                : CommonPropertyProvider?.GetAccountViewModel(_transIncBase.AccountId);
            set
            {
                if (value == null || value.Id == _transIncBase.AccountId) return;
                IAccountViewModel temp = Account;
                _transIncBase.AccountId = value.Id;
                OnUpdate();
                if (temp != null) Messenger.Default.Send(AccountMessage.Refresh, temp);
                Messenger.Default.Send(AccountMessage.Refresh, value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Someone or something, who got paid or paid the user by the Transaction/Income.
        /// </summary>
        public virtual IPayeeViewModel Payee
        {
            get => _transIncBase.PayeeId == -1
                ? null
                : CommonPropertyProvider?.GetPayeeViewModel(_transIncBase.PayeeId);
            set
            {
                if (value == null || value.Id == _transIncBase.PayeeId) return;
                _transIncBase.PayeeId = value.Id;
                OnUpdate();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a TransIncBaseViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="transIncBase">The model.</param>
        protected TransIncBaseViewModel(IBffOrm orm, ITransIncBase transIncBase) : base(orm, transIncBase)
        {
            _transIncBase = transIncBase;
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
            Payee newPayee = Orm.BffRepository.PayeeRepository.Create();
            newPayee.Name = PayeeText.Trim();
            newPayee.Insert();
            OnPropertyChanged(nameof(AllPayees));
            Payee = CommonPropertyProvider?.GetPayeeViewModel(newPayee.Id);
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
