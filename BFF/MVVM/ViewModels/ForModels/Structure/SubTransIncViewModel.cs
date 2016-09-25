using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ISubTransIncViewModel : ITitLikeViewModel
    {
        /// <summary>
        /// Each SubTransaction or SubIncome can be budgeted to a category.
        /// </summary>
        ICategoryViewModel Category { get; set; }

        long ParentId { get; set; }
    }

    /// <summary>
    /// Base class for ViewModels of the Models SubTransaction and SubIncome
    /// </summary>
    public abstract class SubTransIncViewModel : TitLikeViewModel, ISubTransIncViewModel
    {
        /// <summary>
        /// Model of SubTransaction or SubIncome. Mostly they both act almost the same. Differences are handled in their concrete classes.
        /// </summary>
        protected readonly ISubTransInc SubTransInc;
        /// <summary>
        /// The ViewModel of the Parent Model of the SubTransInc.
        /// </summary>
        protected IParentTransIncViewModel Parent;

        #region SubTransaction/SubIncome Properties

        /// <summary>
        /// The object's Id in the table of the database.
        /// </summary>
        public override long Id => SubTransInc.Id;

        public long ParentId
        {
            get { return SubTransInc.ParentId; }
            set { SubTransInc.ParentId = value; }
        }

        /// <summary>
        /// Each SubTransaction or SubIncome can be budgeted to a category.
        /// </summary>
        public ICategoryViewModel Category
        {
            get
            {
                return SubTransInc.CategoryId == -1 ? null :
                  Orm?.CommonPropertyProvider.GetCategoryViewModel(SubTransInc.CategoryId);
            }
            set
            {
                if (value == null || value.Id == SubTransInc.CategoryId) return; //todo: make Category nullable?
                SubTransInc.CategoryId = value.Id;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// A note, which a user can attach to each TIT as a reminder for himself.
        /// </summary>
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

        /// <summary>
        /// The amount of money of the exchangement of the SubTransaction or SubIncome.
        /// </summary>
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

        #endregion

        /// <summary>
        /// Initializes a SubTransIncViewModel.
        /// </summary>
        /// <param name="subTransInc">The associated Model of this ViewModel.</param>
        /// <param name="parent">The ViewModel of the Parent Model of the SubTransInc.</param>
        /// <param name="orm">Used for the database accesses.</param>
        protected SubTransIncViewModel(ISubTransInc subTransInc, IParentTransIncViewModel parent, IBffOrm orm) : base(orm)
        {
            SubTransInc = subTransInc;
            Parent = parent;
            SubTransInc.ParentId = Parent.Id;
        }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        public override bool ValidToInsert()
        {
            return Category != null && (Orm?.CommonPropertyProvider.AllCategoryViewModels.Contains(Category) ?? false);
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
            ICategory newCategory = new Category { Name = CategoryText.Trim(), ParentId = AddingCategoryParent?.Id };
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
        /// Deletes the Model from the database and removes itself from its parent.
        /// </summary>
        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            Delete();
            Parent?.RemoveSubElement(this);
        });

        /// <summary>
        /// Uses the OR mapper to insert the model into the database. Inner function for the Insert method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void InsertToDb()
        {
            SubTransInc.Insert(Orm);
        }

        /// <summary>
        /// Uses the OR mapper to update the model in the database. Inner function for the Update method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void UpdateToDb()
        {
            SubTransInc.Update(Orm);
        }

        /// <summary>
        /// Uses the OR mapper to delete the model from the database. Inner function for the Delete method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void DeleteFromDb()
        {
            SubTransInc.Delete(Orm);
        }
    }
}
