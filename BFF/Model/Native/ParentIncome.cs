using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    /// <summary>
    /// An Income, which is split into several SubIncomes
    /// </summary>
    public class ParentIncome : Income, IParentTitNoTransfer<SubIncome>
    {
        private ObservableCollection<SubIncome> _subElements;

        /// <summary>
        /// SubElements if this is a Parent
        /// </summary>
        [Write(false)]
        public ObservableCollection<SubIncome> SubElements
        {
            get
            {
                if (_subElements == null)
                {
                    _subElements = new ObservableCollection<SubIncome>(Database?.GetSubTransInc<SubIncome>(Id));
                    foreach (SubIncome subIncome in _subElements)
                        subIncome.Parent = this;
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
                Account?.RefreshBalance();
            }
        }

        [Write(false)]
        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            foreach (SubIncome subIncome in SubElements)
                subIncome.Delete();
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
        public ParentIncome(DateTime date, Account account = null, Payee payee = null,
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
        /// <param name="payeeId">Id of Payee</param>
        /// <param name="categoryId">Id of Category</param>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The amount of money, which was payeed or recieved</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public ParentIncome(long id, long accountId, DateTime date, long payeeId, long categoryId, string memo,
            long sum, bool cleared)
            : base(id, accountId, date, payeeId, categoryId, memo, sum, cleared)
        {
            ConstrDbLock = true;

            ConstrDbLock = false;
        }

        #region SubElementStuff

        public override bool ValidToInsert()
        {
            return Account != null && (Database?.AllAccounts.Contains(Account) ?? false) &&
                Payee != null && (Database?.AllPayees.Contains(Payee) ?? false) && NewSubElements.All(subElement => subElement.ValidToInsert());
        }

        private readonly ObservableCollection<SubIncome> _newSubElements = new ObservableCollection<SubIncome>();

        /// <summary>
        /// New SubElements, which have to be edited before addition 
        /// </summary>
        [Write(false)]
        public ObservableCollection<SubIncome> NewSubElements
        {
            get
            {
                return _newSubElements;
            }
            set { }
        }

        [Write(false)]
        public ICommand NewSubElementCommand => new RelayCommand(obj => _newSubElements.Add(new SubIncome(this)));

        [Write(false)]
        public ICommand ApplyCommand => new RelayCommand(obj =>
        {
            foreach (SubIncome subIncome in _newSubElements)
            {
                if (Id > 0L)
                    subIncome.Insert();
                _subElements.Add(subIncome);
            }
            _newSubElements.Clear();
            OnPropertyChanged(nameof(Sum));
            Account?.RefreshBalance();
        });

        public void RemoveSubElement(SubTitBase toRemove)
        {
            if (SubElements.Contains(toRemove))
                SubElements.Remove(toRemove as SubIncome);
        }

        #endregion
    }
}
