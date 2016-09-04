using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    /// <summary>
    /// The ViewModel of the Model Transaction.
    /// </summary>
    public class TransactionViewModel : TransIncViewModel
    {
        /// <summary>
        /// Initializes a TransactionViewModel.
        /// </summary>
        /// <param name="transInc">A Transaction Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public TransactionViewModel(ITransaction transInc, IBffOrm orm) : base(transInc, orm) { }
    }
}