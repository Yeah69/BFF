using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.WPFStuff;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native.Structure
{
    /// <summary>
    /// Base of all Tit-classes except Transfer (TIT := Transaction Income Transfer)
    /// </summary>
    public abstract class TitNoTransfer : TitBase
    {
        private Category _category;
        private Payee _payee;
        private Account _account;

        /// <summary>
        /// The Account to which this belongs
        /// </summary>
        [Write(false)]
        public Account Account
        {
            get { return _account; }
            set
            {
                if(_account == value) return;
                Account temp = _account;
                _account = value;
                if(Id != -1) Update();
                Messenger.Default.Send(AccountMessage.Refresh, temp);
                Messenger.Default.Send(AccountMessage.Refresh, _account);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of Account
        /// </summary>
        public long AccountId
        {
            get { return Account?.Id ?? -1; }
            set { _account = Database?.CommonPropertyProvider.GetAccount(value); }
        }

        /// <summary>
        /// To whom was payeed or who payeed
        /// </summary>
        [Write(false)]
        public Payee Payee
        {
            get { return _payee; }
            set
            {
                if(_payee == value) return;
                _payee = value;
                if(Id != -1) Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of Payee
        /// </summary>
        public long PayeeId
        {
            get { return Payee?.Id ?? -1; }
            set { Payee = Database?.GetPayee(value); }
        }

        /// <summary>
        /// Categorizes this
        /// </summary>
        [Write(false)]
        public Category Category
        {
            get { return _category; }
            set
            {
                if(_category == value) return;
                _category = value;
                if(Id != -1) Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of Category
        /// </summary>
        public long? CategoryId
        {
            get { return Category?.Id; }
            set { Category = Database?.GetCategory(value ?? -1L); }
        }

        public override long Sum
        {
            get { return base.Sum; }
            set
            {
                base.Sum = value;
                Messenger.Default.Send(AccountMessage.RefreshBalance, _account);
            }
        }

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payeed or who payeed</param>
        /// <param name="category">Categorizes this</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The amount of money, which was payeed or recieved</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        protected TitNoTransfer(DateTime date, Account account = null, Payee payee = null,
            Category category = null, string memo = null, long sum = 0L, bool? cleared = null)
            : base(date, memo: memo, sum: sum, cleared: cleared)
        {
            ConstrDbLock = true;
            
            _account = account ?? _account;
            _payee = payee ?? _payee;
            _category = category ?? _category;

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
        protected TitNoTransfer(long id, long accountId, DateTime date, long payeeId, long categoryId, string memo,
            long sum, bool cleared)
            : base(date, id, memo, sum, cleared)
        {
            ConstrDbLock = true;
            
            AccountId = accountId;
            PayeeId = payeeId;
            CategoryId = categoryId;

            ConstrDbLock = false;
        }

        [Write(false)]
        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            Delete();
            Messenger.Default.Send(AllAccountMessage.Refresh);
            Messenger.Default.Send(AccountMessage.Refresh, Account);
        });

        public override bool ValidToInsert()
        {
            return Account != null && (Database?.CommonPropertyProvider.Accounts.Contains(Account) ?? false) && 
                Payee != null && (Database?.AllPayees.Contains(Payee) ?? false) && 
                Category != null && (Database?.AllCategories.Contains(Category) ?? false);
        }

        #region EditingPayeeAndCategory

        [Write(false)]
        public string PayeeText { get; set; }

        [Write(false)]
        public ICommand AddPayeeCommand => new RelayCommand(obj =>
        {
            Payee newPayee = new Payee {Name = PayeeText.Trim()};
            Database?.Insert(newPayee);
            OnPropertyChanged();
            Payee = newPayee;
        }, obj =>
        {
            string trimmedPayeeText = PayeeText?.Trim();
            return !string.IsNullOrEmpty(trimmedPayeeText) && Database?.AllPayees?.Count(payee => payee.Name == trimmedPayeeText) == 0;
        });

        [Write(false)]
        public ObservableCollection<Payee> AllPayees => Database?.AllPayees;

        [Write(false)]
        public string CategoryText { get; set; }

        [Write(false)]
        public Category AddingCategoryParent { get; set; }

        [Write(false)]
        public ICommand AddCategoryCommand => new RelayCommand(obj =>
        {
            Category newCategory = new Category {Name = CategoryText.Trim()};
            if (AddingCategoryParent != null)
            {
                newCategory.Parent = AddingCategoryParent;
                AddingCategoryParent.Categories.Add(newCategory);
            }
            Database?.Insert(newCategory);
            OnPropertyChanged(nameof(AllCategories));
            Category = newCategory;
        }, obj =>
        {
            string trimmedCategoryText = CategoryText?.Trim();
            return !string.IsNullOrEmpty(trimmedCategoryText) && 
                (AddingCategoryParent == null && Database?.AllCategories?.Count(category => category.Parent == null && category.Name == trimmedCategoryText) == 0 ||
                AddingCategoryParent != null && AddingCategoryParent.Categories.Count(category => category.Name == trimmedCategoryText) == 0);
        });

        [Write(false)]
        public ObservableCollection<Category> AllCategories => Database?.AllCategories;
        
        #endregion EditingPayeeAndCategory
    }
}
