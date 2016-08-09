using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.Helper.Import;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels;
using Dapper.Contrib.Extensions;

namespace BFF.MVVM.Models.Native
{
    /// <summary>
    /// A Transaction, which is split into several SubTransactions
    /// </summary>
    public class ParentTransaction : Transaction, IParentTransInc<SubTransaction>
    {
        private ObservableCollection<SubTransaction> _subElements;

        /// <summary>
        /// SubElements if this is a Parent
        /// </summary>
        [Write(false)]
        public ObservableCollection<SubTransaction> SubElements
        {
            get
            {
                if (_subElements == null)
                {
                    _subElements = new ObservableCollection<SubTransaction>(Database?.GetSubTransInc<SubTransaction>(Id) ?? new List<SubTransaction>());
                    foreach (SubTransaction subTransaction in _subElements)
                        subTransaction.Parent = this;
                }
                return _subElements;
            }
            set { }
        }

        public override long Sum
        {
            get { return SubElements.Sum(subElement => subElement.Sum); } //todo: Write an SQL query for that
            set
            {
                OnPropertyChanged();
                Messenger.Default.Send(AllAccountMessage.Refresh);
                Messenger.Default.Send(AccountMessage.Refresh, Account);
            }
        }

        [Write(false)]
        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            foreach(SubTransaction subTransaction in SubElements)
                subTransaction.Delete();
            SubElements.Clear();
            Delete();
        });

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payeed or who payeed</param>
        /// <param name="category">Categorizes this</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public ParentTransaction(DateTime date, Account account = null, Payee payee = null,
            Category category = null, string memo = null, bool? cleared = null)
            : base(date, account, payee, category, memo, 0L, cleared)
        {
            ConstrDbLock = true;

            ConstrDbLock = false;
        }

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="accountId">Id of Account</param>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="payeeId">Id of Payee</param>
        /// <param name="categoryId">Id of Category</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The amount of money, which was payeed or recieved</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public ParentTransaction(long id, long accountId, DateTime date, long payeeId, long categoryId, string memo,
            long sum, bool cleared)
            : base(id, accountId, date, payeeId, categoryId, memo, sum, cleared)
        {
            ConstrDbLock = true;

            ConstrDbLock = false;
        }

        /// <summary>
        /// Creates a Transaction-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        public static explicit operator ParentTransaction(Conversion.YNAB.Transaction ynabTransaction)
        {
            ParentTransaction ret = new ParentTransaction(ynabTransaction.Date)
            {
                Account = Account.GetOrCreate(ynabTransaction.Account),
                Payee = Payee.GetOrCreate(YnabCsvImport.PayeePartsRegex.Match(ynabTransaction.Payee).Groups["payeeStr"].Value),
                Category = null,
                Memo = YnabCsvImport.MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value,
                Sum = 0L,
                Cleared = ynabTransaction.Cleared
            };
            return ret;
        }

        #region SubElementStuff

        public override bool ValidToInsert()
        {
            return Account != null && (Database?.CommonPropertyProvider.Accounts.Contains(Account) ?? false) &&
                Payee != null && (Database?.AllPayees.Contains(Payee) ?? false) && NewSubElements.All(subElement => subElement.ValidToInsert());
        }

        private readonly ObservableCollection<SubTransaction> _newSubElements = new ObservableCollection<SubTransaction>();
        private ICommand _openParentTitView;

        /// <summary>
        /// New SubElements, which have to be edited before addition 
        /// </summary>
        [Write(false)]
        public ObservableCollection<SubTransaction> NewSubElements
        {
            get
            {
                return _newSubElements;
            }
            set { }
        }

        public ICommand OpenParentTitView
        {
            get { return new RelayCommand(param => 
            Messenger.Default.Send(new ParentTitViewModel(this, "Yeah69", param as Account))); }
        }

        [Write(false)]
        public ICommand NewSubElementCommand => new RelayCommand(obj => _newSubElements.Add(new SubTransaction(this)));

        [Write(false)]
        public ICommand ApplyCommand => new RelayCommand(obj =>
        {
            foreach (SubTransaction subTransaction in _newSubElements)
            {
                if(Id > 0L)
                    subTransaction.Insert();
                _subElements.Add(subTransaction);
            }
            _newSubElements.Clear();
            OnPropertyChanged(nameof(Sum));
            Messenger.Default.Send(AllAccountMessage.Refresh);
            Messenger.Default.Send(AccountMessage.Refresh, Account);
        });

        public void RemoveSubElement(SubTitBase toRemove)
        {
            if (SubElements.Contains(toRemove))
                SubElements.Remove(toRemove as SubTransaction);
        }

        #endregion
    }
}
