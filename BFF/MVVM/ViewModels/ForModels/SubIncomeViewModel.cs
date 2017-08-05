using BFF.DB;
using BFF.Helper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;

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
            IBffOrm orm,
            CategoryViewModelService categoryViewModelService,
            ParentIncomeViewModelService parentIncomeViewModelService) :
        base(subIncome, orm, categoryViewModelService)
        {
            Parent =
                subIncome.ToReadOnlyReactivePropertyAsSynchronized(
                    st => st.Parent,
                    pt => parentIncomeViewModelService.GetViewModel(pt as IParentIncome));
        }

        public override IReadOnlyReactiveProperty<IParentTransIncViewModel> Parent { get; }
    }
}