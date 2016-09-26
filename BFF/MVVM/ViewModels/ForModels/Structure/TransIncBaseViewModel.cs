using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITransIncBaseViewModel : ITitBaseViewModel
    {
        /// <summary>
        /// The assigned Account, where this Transaction/Income is registered.
        /// </summary>
        IAccount Account { get; set; }

        /// <summary>
        /// Someone or something, who got paid or paid the user by the Transaction/Income.
        /// </summary>
        IPayee Payee { get; set; }
    }

    /// <summary>
    /// Base class for ViewModels of the models Transaction, Income, ParentTransaction and ParentIncome.
    /// </summary>
    public abstract class TransIncBaseViewModel : TitBaseViewModel, ITransIncBaseViewModel
    {
        /// <summary>
        /// The assigned Account, where this Transaction/Income is registered.
        /// </summary>
        public abstract IAccount Account { get; set; } //todo: change to IAccountViewModel

        /// <summary>
        /// Someone or something, who got paid or paid the user by the Transaction/Income.
        /// </summary>
        public abstract IPayee Payee { get; set; }

        /// <summary>
        /// Initializes a TransIncBaseViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        protected TransIncBaseViewModel(IBffOrm orm) : base(orm) { }

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
            IPayee newPayee = new Payee {Name = PayeeText.Trim()};
            Orm?.CommonPropertyProvider?.Add(newPayee);
            OnPropertyChanged(nameof(AllPayees));
            Payee = newPayee;
        }, obj =>
        {
            string trimmedPayeeText = PayeeText?.Trim();
            return !string.IsNullOrEmpty(trimmedPayeeText) &&
                   AllPayees.Count(payee => payee.Name == trimmedPayeeText) == 0;
        });

        /// <summary>
        /// All currently available Payees.
        /// </summary>
        public ObservableCollection<IPayee> AllPayees => Orm?.CommonPropertyProvider.Payees;

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
