using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IParentIncomeViewModel : IParentTransIncViewModel
    {
        ReadOnlyReactiveCollection<ISubIncomeViewModel> SubIncomes { get; }
    }

    /// <summary>
    /// The ViewModel of the Model ParentIncome.
    /// </summary>
    public class ParentIncomeViewModel : ParentTransIncViewModel, IParentIncomeViewModel
    {
        private readonly IParentIncome _parentIncome;

        /// <summary>
        /// Initializes a ParentIncomeViewModel.
        /// </summary>
        /// <param name="transInc">A ParentIncome Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public ParentIncomeViewModel(
            IParentIncome transInc,
            IBffOrm orm,
            SubIncomeViewModelService subIncomeViewModelService) : base(transInc, orm)
        {
            _parentIncome = transInc;

            SubIncomes =
                _parentIncome.SubIncomes.ToReadOnlyReactiveCollection(subIncomeViewModelService
                    .GetViewModel);
        }

        #region Overrides of ParentTransIncViewModel<SubIncome>

        /// <summary>
        /// Provides a ViewModel of the given SubElement.
        /// </summary>
        /// <param name="subElement">The SubElement, which gets a ViewModel.</param>
        /// <returns>A new ViewModel for a SubElement.</returns>
        protected override ISubTransIncViewModel CreateNewSubViewModel(ISubTransInc subElement)
        {
            return new SubIncomeViewModel(
                subElement as ISubIncome,
                Orm,
                Orm.CommonPropertyProvider.CategoryViewModelService,
                Orm.ParentIncomeViewModelService);
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
            return Orm?.GetSubTransInc<SubIncome>(_parentIncome.Id);
        }

        #endregion

        public ReadOnlyReactiveCollection<ISubIncomeViewModel> SubIncomes { get; }
    }
}
