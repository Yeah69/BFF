using System;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface ITransactionViewModel : ITransIncViewModel {}

    /// <summary>
    /// The ViewModel of the Model Transaction.
    /// </summary>
    public class TransactionViewModel : TransIncViewModel, ITransactionViewModel
    {
        /// <summary>
        /// Initializes a TransactionViewModel.
        /// </summary>
        /// <param name="parentTransInc">A Transaction Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="accountViewModelService">Service of accounts.</param>
        /// <param name="payeeViewModelService">Service of payees.</param>
        /// <param name="categoryViewModelService">Service of categories.</param>
        public TransactionViewModel(
            ITransaction parentTransInc,
            Func<IHaveCategoryViewModel, INewCategoryViewModel> newCategoryViewModelFactory,
            IBffOrm orm,
            AccountViewModelService accountViewModelService, 
            PayeeViewModelService payeeViewModelService, 
            CategoryViewModelService categoryViewModelService) 
            : base(
                  parentTransInc, 
                  newCategoryViewModelFactory,
                  orm, 
                  accountViewModelService,
                  payeeViewModelService,
                  categoryViewModelService) { }
    }
}