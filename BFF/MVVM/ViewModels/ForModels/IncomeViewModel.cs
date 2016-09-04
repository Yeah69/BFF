using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    /// <summary>
    /// The ViewModel of the Model Income.
    /// </summary>
    class IncomeViewModel : TransIncViewModel
    {
        /// <summary>
        /// Initializes an IncomeViewModel.
        /// </summary>
        /// <param name="transInc">A Transaction Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public IncomeViewModel(IIncome transInc, IBffOrm orm) : base(transInc, orm) { }
    }
}