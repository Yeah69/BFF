using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IParentTransactionViewModel : IParentTransIncViewModel
    {
        ReadOnlyReactiveCollection<ISubTransactionViewModel> SubTransactions { get; }
    }

    /// <summary>
    /// The ViewModel of the Model ParentTransaction.
    /// </summary>
    public class ParentTransactionViewModel : ParentTransIncViewModel, IParentTransactionViewModel
    {
        private readonly IParentTransaction _parentTransaction;

        /// <summary>
        /// Initializes a ParentTransactionViewModel.
        /// </summary>
        /// <param name="parentTransaction">A ParentTransaction Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public ParentTransactionViewModel(
            IParentTransaction parentTransaction,
            IBffOrm orm,
            SubTransactionViewModelService subTransactionViewModelService) : base(parentTransaction, orm)
        {
            _parentTransaction = parentTransaction;

            SubTransactions =
                _parentTransaction.SubTransactions.ToReadOnlyReactiveCollection(subTransactionViewModelService
                    .GetViewModel);
        }

        #region Overrides of ParentTransIncViewModel<SubTransaction>

        /// <summary>
        /// Provides a ViewModel of the given SubElement.
        /// </summary>
        /// <param name="subElement">The SubElement, which gets a ViewModel.</param>
        /// <returns>A new ViewModel for a SubElement.</returns>
        protected override ISubTransIncViewModel CreateNewSubViewModel(ISubTransInc subElement)
        {
            return new SubTransactionViewModel(
                subElement as ISubTransaction,
                Orm, 
                Orm.CommonPropertyProvider.CategoryViewModelService, 
                Orm.ParentTransactionViewModelService);
        }

        /// <summary>
        /// Provides a new SubElement.
        /// </summary>
        /// <returns>A new SubElement.</returns>
        public override ISubTransInc CreateNewSubElement()
        {
            return Orm.BffRepository.SubIncomeRepository.Create();
        }

        protected override IEnumerable<ISubTransInc> GetSubTransInc()
        {
            return Orm?.GetSubTransInc<SubTransaction>(_parentTransaction.Id);
        }

        #endregion

        public ReadOnlyReactiveCollection<ISubTransactionViewModel> SubTransactions { get; }
    }
}
