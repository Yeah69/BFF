using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IParentTransIncViewModel : ITransIncBaseViewModel {
        
    }

    /// <summary>
    /// Base class for ViewModels of the Models ParentTransaction and ParentIncome
    /// </summary>
    public abstract class ParentTransIncViewModel : TransIncBaseViewModel, IParentTransIncViewModel
    {
        /// <summary>
        /// The concrete Parent class should provide a ViewModel of the given SubElement.
        /// </summary>
        /// <param name="subElement">The SubElement, which gets a ViewModel.</param>
        /// <returns>A new ViewModel for a SubElement.</returns>
        protected abstract ISubTransIncViewModel CreateNewSubViewModel(ISubTransInc subElement);

        /// <summary>
        /// Initializes a ParentTransIncViewModel.
        /// </summary>
        /// <param name="parentTransInc">The associated Model of this ViewModel.</param>
        /// <param name="orm">Used for the database accesses.</param>
        protected ParentTransIncViewModel(
            IParentTransInc parentTransInc,
            Func<IHavePayeeViewModel, INewPayeeViewModel> newPayeeViewModelFactory, 
            IBffOrm orm) 
            : base(
                  orm, 
                  parentTransInc, 
                  newPayeeViewModelFactory,
                  orm.CommonPropertyProvider.AccountViewModelService,
                  orm.CommonPropertyProvider.PayeeViewModelService)
        {
        }

        
    }
}
