using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.WPFStuff;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native.Structure
{
    /// <summary>
    /// Base of all Tit-classes except Transfer
    /// </summary>
    public abstract class TitNoTransfer : TitBase
    {
        private long? _sum;
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
                _account = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of Account
        /// </summary>
        public long AccountId
        {
            get { return Account?.Id ?? -1; }
            set { Account = Database?.GetAccount(value); }
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
                _payee = value;
                Update();
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
                _category = value;
                Update();
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

        /// <summary>
        /// The amount of money, which was payeed or recieved
        /// </summary>
        public long? Sum
        {
            get
            {
                return _sum ?? SubElements.Sum(subTransaction => subTransaction.Sum);
            }
            set
            {
                if (Type == "SingleTrans")
                {
                    _sum = value;
                    Update();
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// SubElements if this is a Parent
        /// </summary>
        [Write(false)]
        public abstract IEnumerable<SubTitBase> SubElements { get; }

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payeed or who payeed</param>
        /// <param name="category">Categorizes this</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        protected TitNoTransfer( Account account = null, Payee payee = null,
            Category category = null, string memo = null, bool? cleared = null)
            : base(memo, cleared)
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
        /// <param name="payeeId">Id of Payee</param>
        /// <param name="categoryId">Id of Category</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The amount of money, which was payeed or recieved</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        protected TitNoTransfer(long id, long accountId, long payeeId, long categoryId, string memo,
            long? sum, bool cleared)
            : base(memo, cleared)
        {
            ConstrDbLock = true;

            Id = id;
            AccountId = accountId;
            PayeeId = payeeId;
            CategoryId = categoryId;
            _sum = sum;

            ConstrDbLock = false;
        }

        #region EditingPayeeAndCategory

        [Write(false)]
        public string PayeeText { get; set; }

        [Write(false)]
        public ICommand AddPayeeCommand => new RelayCommand(obj =>
        {
            Database?.Insert(new Payee {Name = PayeeText.Trim()});
        }, obj =>
        {
            string trimmedPayeeText = PayeeText.Trim();
            return trimmedPayeeText != "" && Database?.AllPayees?.Count(payee => payee.Name == trimmedPayeeText) == 0;
        });

        [Write(false)]
        public string CategoryText { get; set; }

        [Write(false)]
        public Category AddingCategoryParent { get; set; }

        [Write(false)]
        public ICommand AddCategoryCommand => new RelayCommand(obj =>
        {
            Category newCategory = new Category {Name = CategoryText.Trim()};
            Database?.Insert(newCategory);
            if (AddingCategoryParent != null)
            {
                newCategory.Parent = AddingCategoryParent;
                AddingCategoryParent.Categories.Add(newCategory);
            }
            OnPropertyChanged(nameof(AllCategories));
        }, obj =>
        {
            string trimmedCategoryText = CategoryText.Trim();
            return trimmedCategoryText != "" && 
                (AddingCategoryParent == null && Database?.AllCategories?.Count(category => category.Parent == null && category.Name == trimmedCategoryText) == 0 ||
                AddingCategoryParent != null && AddingCategoryParent.Categories.Count(category => category.Name == trimmedCategoryText) == 0);
        });

        [Write(false)]
        public ObservableCollection<Category> AllCategories => Database?.AllCategories;
        
        #endregion EditingPayeeAndCategory
    }
}
