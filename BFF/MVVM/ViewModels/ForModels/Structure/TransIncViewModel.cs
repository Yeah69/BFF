using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public abstract class TransIncViewModel : TransIncBaseViewModel
    {
        protected readonly ITransInc TransInc;

        #region Transaction/Income Properties

        public override long Id => TransInc.Id;

        public override Account Account
        {
            get { return TransInc.Account; }
            set
            {
                TransInc.Account = value;
                Update();
                OnPropertyChanged();
            }
        }
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
        public override Payee Payee
        {
            get { return TransInc.Payee; }
            set
            {
                TransInc.Payee = value;
                Update();
                OnPropertyChanged();
            }
        }
        public Category Category
        {
            get { return TransInc.Category; }
            set
            {
                TransInc.Category = value;
                Update();
                OnPropertyChanged();
            }
        }
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
        public override long Sum
        {
            get { return TransInc.Sum; }
            set
            {
                TransInc.Sum = value;
                Update();
                OnPropertyChanged();
            }
        }

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

        protected TransIncViewModel(ITransInc transInc, IBffOrm orm) : base(orm)
        {
            TransInc = transInc;
        }

        internal override bool ValidToInsert()
        {
            return Account != null  && (Orm?.CommonPropertyProvider.Accounts.Contains(Account) ?? false) &&
                   Payee != null    &&  Orm .CommonPropertyProvider.Payees.Contains(Payee) &&
                   Category != null &&  Orm .AllCategories.Contains(Category);
        }
        
        public string CategoryText { get; set; }
        
        public Category AddingCategoryParent { get; set; }
        
        public ICommand AddCategoryCommand => new RelayCommand(obj =>
        {
            Category newCategory = new Category { Name = CategoryText.Trim() };
            if (AddingCategoryParent != null)
            {
                newCategory.Parent = AddingCategoryParent;
                AddingCategoryParent.Categories.Add(newCategory);
            }
            Orm?.Insert(newCategory);
            OnPropertyChanged(nameof(AllCategories));
            Category = newCategory;
        }, obj =>
        {
            string trimmedCategoryText = CategoryText?.Trim();
            return !string.IsNullOrEmpty(trimmedCategoryText) &&
                (AddingCategoryParent == null && AllCategories?.Count(category => category.Parent == null && category.Name == trimmedCategoryText) == 0 ||
                AddingCategoryParent != null && AddingCategoryParent.Categories.Count(category => category.Name == trimmedCategoryText) == 0);
        });
        
        public ObservableCollection<Category> AllCategories => Orm?.AllCategories;
    }
}
