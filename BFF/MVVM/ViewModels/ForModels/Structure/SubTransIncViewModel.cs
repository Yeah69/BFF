﻿using System.Collections.Generic;
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
        private readonly ISubTransInc _subTransInc;
        /// <summary>
        /// The ViewModel of the Parent Model of the SubTransInc.
        /// </summary>
        private readonly IParentTransIncViewModel _parent;

        #region SubTransaction/SubIncome Properties

        public long ParentId
        {
            get => _subTransInc.ParentId;
            set => _subTransInc.ParentId = value;
        }

        /// <summary>
        /// Each SubTransaction or SubIncome can be budgeted to a category.
        /// </summary>
        public ICategoryViewModel Category
        {
            get => _subTransInc.CategoryId == -1 ? null :
                       CommonPropertyProvider.CategoryViewModelService.GetViewModel(_subTransInc.CategoryId);
            set
            {
                if (value == null || value.Id == _subTransInc.CategoryId) return; //todo: make Category nullable?
                _subTransInc.CategoryId = value.Id;
                OnUpdate();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The amount of money of the exchangement of the SubTransaction or SubIncome.
        /// </summary>
        public override long Sum
        {
            get => _subTransInc.Sum;
            set
            {
                if(_subTransInc.Sum == value) return;
                _subTransInc.Sum = value;
                OnUpdate();
                _parent.RefreshSum();
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
        protected SubTransIncViewModel(ISubTransInc subTransInc, IParentTransIncViewModel parent, IBffOrm orm) : base(orm, subTransInc)
        {
            _subTransInc = subTransInc;
            _parent = parent;
            _subTransInc.ParentId = _parent.Id;
        }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        public override bool ValidToInsert()
        {
            return Category != null;
        }

        #region Category Editing

        //todo: Delegate the Editing?

        private string _categoryText;

        /// <summary>
        /// User input of the to be searched or to be created Category.
        /// </summary>
        public string CategoryText
        {
            get => _categoryText;
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
            ICategory newCategory = Orm.BffRepository.CategoryRepository.Create();
            newCategory.Name = CategoryText.Trim();
            newCategory.Parent = CommonPropertyProvider.CategoryViewModelService.GetModel(AddingCategoryParent);
            newCategory.Insert();
            OnPropertyChanged(nameof(AllCategories));
            Category = CommonPropertyProvider?.CategoryViewModelService.GetViewModel(newCategory);
        }, obj =>
        {
            return !string.IsNullOrWhiteSpace(CategoryText) &&
            (AddingCategoryParent == null && (CommonPropertyProvider?.ParentCategoryViewModels.All(pcvm => pcvm.Name.Value != CategoryText) ?? false) ||
            AddingCategoryParent != null && AddingCategoryParent.Categories.All(c => c.Name.Value != CategoryText));
        });

        /// <summary>
        /// All currently available Categories.
        /// </summary>
        public IEnumerable<ICategoryViewModel> AllCategories => CommonPropertyProvider?.AllCategoryViewModels;

        #endregion

        /// <summary>
        /// Deletes the Model from the database and removes itself from its parent.
        /// </summary>
        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            Delete();
            _parent?.RemoveSubElement(this);
        });
    }
}
