using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    abstract class SubTransIncViewModel<T> : TitLikeViewModel where T : ISubTransInc
    {
        protected readonly ISubTransInc SubTransInc;
        protected ParentTransIncViewModel<T> Parent; 

        #region SubTransaction/SubIncome Properties

        public override long Id => SubTransInc.Id;

        public Category Category
        {
            get
            {
                return SubTransInc.CategoryId == -1 ? null : 
                    Orm?.GetCategory(SubTransInc.CategoryId);
            }
            set
            {
                SubTransInc.CategoryId = value?.Id ?? -1;
                Update();
                OnPropertyChanged();
            }
        }
        public override string Memo
        {
            get { return SubTransInc.Memo; }
            set
            {
                SubTransInc.Memo = value;
                Update();
                OnPropertyChanged();
            }
        }
        public override long Sum
        {
            get { return SubTransInc.Sum; }
            set
            {
                SubTransInc.Sum = value;
                Update();
                Parent.RefreshSum();
                OnPropertyChanged();
            }
        }

        internal override bool ValidToInsert()
        {
            return Category != null && (Orm?.AllCategories.Contains(Category) ?? false) && SubTransInc.ParentId != -1;
        }

        #endregion
        protected SubTransIncViewModel(ISubTransInc subTransInc, ParentTransIncViewModel<T> parent, IBffOrm orm) : base(orm)
        {
            SubTransInc = subTransInc;
            Parent = parent;
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

        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            Delete();
            Parent?.RemoveSubElement(this);
        });
    }
}
