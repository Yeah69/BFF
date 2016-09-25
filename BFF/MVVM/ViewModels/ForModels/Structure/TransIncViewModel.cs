using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITransIncViewModel : ITransIncBaseViewModel
    {
        /// <summary>
        /// Each Transaction or Income can be budgeted to a category.
        /// </summary>
        ICategoryViewModel Category { get; set; }
    }

    /// <summary>
    /// Base class for ViewModels of Transaction and Income
    /// </summary>
    public abstract class TransIncViewModel : TransIncBaseViewModel, ITransIncViewModel
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
        public override IAccount Account
        {
            get
            {
                return TransInc.AccountId == -1 ? null :
                  Orm?.CommonPropertyProvider?.GetAccount(TransInc.AccountId);
            }
            set
            {
                IAccount temp = Account;
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
        public override IPayee Payee
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
        public ICategoryViewModel Category
        {
            get { return TransInc.CategoryId == -1 ? null :
                    Orm?.CommonPropertyProvider.GetCategoryViewModel(TransInc.CategoryId); }
            set
            {
                if(value == null || value.Id == TransInc.CategoryId) return; //todo: make Category nullable?
                TransInc.CategoryId = value.Id;
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
        public override bool ValidToInsert()
        {
            return Account != null  && (Orm?.CommonPropertyProvider.Accounts.Contains(Account) ?? false) &&
                   Payee != null    &&  Orm .CommonPropertyProvider.Payees.Contains(Payee) &&
                   Category != null &&  Orm .CommonPropertyProvider.AllCategoryViewModels.Contains(Category);
        }

        #region Category Editing

        //todo: Delegate the Editing?

        private string _categoryText;

        /// <summary>
        /// User input of the to be searched or to be created Category.
        /// </summary>
        public string CategoryText
        {
            get { return _categoryText; }
            set
            {
                _categoryText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The ParentCategory to which the new Category should be added.
        /// </summary>
        public ICategoryViewModel AddingCategoryParent { get; set; }
        
        /// <summary>
        /// Creates a new Category.
        /// </summary>
        public ICommand AddCategoryCommand => new RelayCommand(obj =>
        {
            ICategory newCategory = new Category {Name = CategoryText.Trim(), ParentId = AddingCategoryParent?.Id };
            Orm?.CommonPropertyProvider?.Add(newCategory);
            OnPropertyChanged(nameof(AllCategories));
            Category = Orm?.CommonPropertyProvider?.GetCategoryViewModel(newCategory.Id);
        }, obj =>
        {
            return !string.IsNullOrWhiteSpace(CategoryText) && 
            (AddingCategoryParent == null && (Orm?.CommonPropertyProvider?.ParentCategoryViewModels.All(pcvm => pcvm.Name != CategoryText) ?? false) ||
            AddingCategoryParent != null && AddingCategoryParent.Categories.All(c => c.Name != CategoryText));
        });

        /// <summary>
        /// All currently available Categories.
        /// </summary>
        public IEnumerable<ICategoryViewModel> AllCategories => Orm?.CommonPropertyProvider?.AllCategoryViewModels;

        #endregion

        /// <summary>
        /// Uses the OR mapper to insert the model into the database. Inner function for the Insert method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void InsertToDb()
        {
            TransInc.Insert(Orm);
        }

        /// <summary>
        /// Uses the OR mapper to update the model in the database. Inner function for the Update method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void UpdateToDb()
        {
            TransInc.Update(Orm);
            Messenger.Default.Send(SummaryAccountMessage.Refresh);
            Messenger.Default.Send(AccountMessage.Refresh, Account);
        }

        /// <summary>
        /// Uses the OR mapper to delete the model from the database. Inner function for the Delete method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void DeleteFromDb()
        {
            TransInc.Delete(Orm);
        }
    }
}
