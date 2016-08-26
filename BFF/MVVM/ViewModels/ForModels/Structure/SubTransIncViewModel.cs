using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    /// <summary>
    /// Base class for ViewModels of the Models SubTransaction and SubIncome
    /// </summary>
    /// <typeparam name="T">Type of the SubElement. Can be a SubTransaction or a SubIncome.</typeparam>
    public abstract class SubTransIncViewModel<T> : TitLikeViewModel where T : ISubTransInc
    {
        /// <summary>
        /// Model of SubTransaction or SubIncome. Mostly they both act almost the same. Differences are handled in their concrete classes.
        /// </summary>
        protected readonly ISubTransInc SubTransInc;
        /// <summary>
        /// The ViewModel of the Parent Model of the SubTransInc.
        /// </summary>
        protected ParentTransIncViewModel<T> Parent;

        #region SubTransaction/SubIncome Properties

        /// <summary>
        /// The object's Id in the table of the database.
        /// </summary>
        public override long Id => SubTransInc.Id;

        /// <summary>
        /// Each SubTransaction or SubIncome can be budgeted to a category.
        /// </summary>
        public Category Category
        {
            get
            {
                return SubTransInc.CategoryId == -1 ? null : 
                    Orm?.CommonPropertyProvider.GetCategory(SubTransInc.CategoryId);
            }
            set
            {
                SubTransInc.CategoryId = value?.Id ?? -1;
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
        protected SubTransIncViewModel(ISubTransInc subTransInc, ParentTransIncViewModel<T> parent, IBffOrm orm) : base(orm)
        {
            SubTransInc = subTransInc;
            Parent = parent;
        }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        internal override bool ValidToInsert()
        {
            return Category != null && (Orm?.CommonPropertyProvider.Categories.Contains(Category) ?? false) && SubTransInc.ParentId != -1;
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

        /// <summary>
        /// Deletes the Model from the database and removes itself from its parent.
        /// </summary>
        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            Delete();
            Parent?.RemoveSubElement(this);
        });
    }
}
