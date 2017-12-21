using System;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    internal interface IIncomeViewModel : ITransIncViewModel {}

    /// <summary>
    /// The ViewModel of the Model Income.
    /// </summary>
    public class IncomeViewModel : TransIncViewModel, IIncomeViewModel
    {
        /// <summary>
        /// Initializes an IncomeViewModel.
        /// </summary>
        /// <param name="parentTransInc">A Transaction Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="accountViewModelService">Service of accounts.</param>
        /// <param name="payeeViewModelService">Service of payees.</param>
        /// <param name="categoryViewModelService">Service of categories.</param>
        public IncomeViewModel(
            IIncome parentTransInc,
            Func<IHaveCategoryViewModel, INewCategoryViewModel> newCategoryViewModelFactory,
            Func<IHavePayeeViewModel, INewPayeeViewModel> newPayeeViewModelFactory,
            IBffOrm orm,
            IAccountViewModelService accountViewModelService,
            IPayeeViewModelService payeeViewModelService,
            ICategoryBaseViewModelService categoryViewModelService)
            : base(
                  parentTransInc,
                  newCategoryViewModelFactory,
                  newPayeeViewModelFactory,
                  orm,
                  accountViewModelService,
                  payeeViewModelService,
                  categoryViewModelService) { }
    }
}