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
        /// <param name="transInc">A Transaction Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="accountViewModelService">Service of accounts.</param>
        /// <param name="payeeViewModelService">Service of payees.</param>
        /// <param name="categoryViewModelService">Service of categories.</param>
        public TransactionViewModel(
            ITransaction transInc, 
            IBffOrm orm,
            AccountViewModelService accountViewModelService, 
            PayeeViewModelService payeeViewModelService, 
            CategoryViewModelService categoryViewModelService) 
            : base(
                  transInc, 
                  orm, 
                  accountViewModelService,
                  payeeViewModelService,
                  categoryViewModelService) { }
    }
}