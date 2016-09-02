using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    /// <summary>
    /// Base class for ViewModels of Transaction and Income
    /// </summary>
    public abstract class TransIncViewModel : TransIncBaseViewModel
    {
        /// <summary>
        /// Model of Transaction or Income. Mostly they both act almost the same. Differences are handled in their concrete classes.
        /// </summary>
        protected readonly ITransInc TransInc;

        #region Transaction/Income Properties
        
        /// <summary>
        /// The object's Id in the table of the database.
        /// </summary>
        public override long Id => TransInc.Id;

        /// <summary>
        /// The assigned Account, where this Transaction/Income is registered.
        /// </summary>
        public override Account Account
        {
            get
            {
                return TransInc.AccountId == -1 ? null :
                  Orm?.CommonPropertyProvider?.GetAccount(TransInc.AccountId);
            }
            set
            {
                Account temp = Account;
                TransInc.AccountId = value?.Id ?? -1;
                Update();
                if (temp != null) Messenger.Default.Send(AccountMessage.Refresh, temp);
                if (value != null) Messenger.Default.Send(AccountMessage.Refresh, value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// This timestamp marks the time point, when the TIT happened.
        /// </summary>
        public override DateTime Date
        {
            get { return TransInc.Date; }
            set
            {
                TransInc.Date = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Someone or something, who got paid or paid the user by the Transaction/Income.
        /// </summary>
        public override Payee Payee
        {
            get { return TransInc.PayeeId == -1 ? null : 
                    Orm?.CommonPropertyProvider?.GetPayee(TransInc.PayeeId); }
            set
            {
                TransInc.PayeeId = value?.Id ?? -1;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Each Transaction or Income can be budgeted to a category.
        /// </summary>
        public Category Category
        {
            get { return TransInc.CategoryId == -1 ? null :
                    Orm?.CommonPropertyProvider.GetCategory(TransInc.CategoryId); }
            set
            {
                TransInc.CategoryId = value?.Id ?? -1;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// A note, which a user can attach to each TIT as a reminder for himself.
        /// </summary>
        public override string Memo
        {
            get { return TransInc.Memo; }
            set
            {
                TransInc.Memo = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The amount of money of the exchangement of the Transaction or Income.
        /// </summary>
        public override long Sum
        {
            get { return TransInc.Sum; }
            set
            {
                TransInc.Sum = value;
                Update();
                Messenger.Default.Send(AccountMessage.RefreshBalance, Account);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Like the Memo the Cleared flag is an aid for the user.
        /// It can be used to mark TITs, which the user thinks is processed enough (True) or needs to be changed later (False).
        /// This maybe needed, if the user does not remember everything clearly and wants to finish the Tit later.
        /// </summary>
        public override bool Cleared
        {
            get { return TransInc.Cleared; }
            set
            {
                TransInc.Cleared = value;
                Update();
                OnPropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// Initializes a TransIncViewModel.
        /// </summary>
        /// <param name="transInc">The associated Model of this ViewModel.</param>
        /// <param name="orm">Used for the database accesses.</param>
        protected TransIncViewModel(ITransInc transInc, IBffOrm orm) : base(orm)
        {
            TransInc = transInc;
        }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        internal override bool ValidToInsert()
        {
            return Account != null  && (Orm?.CommonPropertyProvider.Accounts.Contains(Account) ?? false) &&
                   Payee != null    &&  Orm .CommonPropertyProvider.Payees.Contains(Payee) &&
                   Category != null &&  Orm .CommonPropertyProvider.Categories.Contains(Category);
        }

        #region Category Editing

        /// <summary>
        /// User input of the to be searched or to be created Category.
        /// </summary>
        public string CategoryText { get; set; }

        /// <summary>
        /// The ParentCategory to which the new Category should be added.
        /// </summary>
        public Category AddingCategoryParent { get; set; }
        
        /// <summary>
        /// Creates a new Category.
        /// </summary>
        public ICommand AddCategoryCommand => new RelayCommand(obj =>
        {
            Category newCategory = new Category {Name = CategoryText.Trim()};
            if(AddingCategoryParent != null)
            {
                newCategory.Parent = AddingCategoryParent;
                AddingCategoryParent.Categories.Add(newCategory);
            }
            Orm?.CommonPropertyProvider?.Add(newCategory);
            OnPropertyChanged(nameof(AllCategories));
            Category = newCategory;
        }, obj =>
        {
            string trimmedCategoryText = CategoryText?.Trim();
            return !string.IsNullOrEmpty(trimmedCategoryText) &&
                   (AddingCategoryParent == null &&
                    AllCategories?.Count(category => category.Parent == null && category.Name == trimmedCategoryText) ==
                    0 ||
                    AddingCategoryParent != null &&
                    AddingCategoryParent.Categories.Count(category => category.Name == trimmedCategoryText) == 0);
        });

        /// <summary>
        /// All currently available Categories.
        /// </summary>
        public ObservableCollection<Category> AllCategories => Orm?.CommonPropertyProvider?.Categories;

        #endregion

    }
}
