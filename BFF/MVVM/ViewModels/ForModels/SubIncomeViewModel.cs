using System;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface ISubIncomeViewModel : ISubTransIncViewModel {}

    /// <summary>
    /// The ViewModel of the Model SubIncome.
    /// </summary>
    public class SubIncomeViewModel : SubTransIncViewModel, ISubIncomeViewModel
    {
        /// <summary>
        /// Initializes a SubIncomeViewModel.
        /// </summary>
        /// <param name="subIncome">A SubIncome Model.</param>
        /// <param name="parent">The ViewModel of the Model's ParentTransaction.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public SubIncomeViewModel(
            ISubIncome subIncome,
            Func<IHaveCategoryViewModel, INewCategoryViewModel> newCategoryViewModelFactory,
            IBffOrm orm,
            ICategoryViewModelService categoryViewModelService) : base(subIncome, newCategoryViewModelFactory, orm, categoryViewModelService) {}
    }
}