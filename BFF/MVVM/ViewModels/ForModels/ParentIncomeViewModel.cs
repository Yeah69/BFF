using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

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
        /// The amount of money of the exchange of the ParentTransaction or ParentIncome.
        /// A ParentElement's Sum is defined by the Sum of all Sum's of its SubElements.
        /// </summary>
        public override IReactiveProperty<long> Sum { get; }

        /// <summary>
        /// Initializes a ParentIncomeViewModel.
        /// </summary>
        /// <param name="parentTransInc">A ParentIncome Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public ParentIncomeViewModel(
            IParentIncome parentTransInc,
            IBffOrm orm,
            SubIncomeViewModelService subIncomeViewModelService) : base(parentTransInc, orm)
        {
            _parentIncome = parentTransInc;

            SubIncomes =
                _parentIncome.SubIncomes.ToReadOnlyReactiveCollection(subIncomeViewModelService
                    .GetViewModel);
            Sum = new ReactiveProperty<long>(SubIncomes.Sum(sivw => sivw.Sum.Value))
                .AddTo(CompositeDisposable);

            SubIncomes.ObserveAddChanged().Concat(SubIncomes.ObserveRemoveChanged())
                .Subscribe(obj => Sum.Value = SubIncomes.Sum(sivw => sivw.Sum.Value))//todo: Write an SQL query for that
                .AddTo(CompositeDisposable);
            SubIncomes.ObserveReplaceChanged()
                .Subscribe(obj => Sum.Value = SubIncomes.Sum(sivw => sivw.Sum.Value))
                .AddTo(CompositeDisposable);
            SubIncomes.ObserveResetChanged()
                .Subscribe(obj => Sum.Value = SubIncomes.Sum(sivw => sivw.Sum.Value))
                .AddTo(CompositeDisposable);
            SubIncomes.ObserveElementObservableProperty(sivw => sivw.Sum)
                .Subscribe(obj => Sum.Value = SubIncomes.Sum(sivw => sivw.Sum.Value))
                .AddTo(CompositeDisposable);
        }

        #region Overrides of ParentTransIncViewModel<SubIncome>

        /// <summary>
        /// Provides a ViewModel of the given SubElement.
        /// </summary>
        /// <param name="subElement">The SubElement, which gets a ViewModel.</param>
        /// <returns>A new ViewModel for a SubElement.</returns>
        protected override ISubTransIncViewModel CreateNewSubViewModel(ISubTransInc subElement)
        {
            return Orm.SubIncomeViewModelService.GetViewModel(subElement as ISubIncome);
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
