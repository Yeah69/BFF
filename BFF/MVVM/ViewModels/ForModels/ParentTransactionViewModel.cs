using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

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
        /// The amount of money of the exchange of the ParentTransaction or ParentIncome.
        /// A ParentElement's Sum is defined by the Sum of all Sum's of its SubElements.
        /// </summary>
        public override IReactiveProperty<long> Sum { get; }
        //{
        //    get { return SubElements.Sum(subElement => subElement.Sum); } //todo: Write an SQL query for that
        //    set => RefreshSum();
        //}

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
            Sum = new ReactiveProperty<long>(SubTransactions.Sum(stvw => stvw.Sum.Value))
                .AddTo(CompositeDisposable);

            SubTransactions.ObserveAddChanged().Concat(SubTransactions.ObserveRemoveChanged())
                .Subscribe(obj => Sum.Value = SubTransactions.Sum(stvw => stvw.Sum.Value))
                .AddTo(CompositeDisposable);
            SubTransactions.ObserveReplaceChanged()
                .Subscribe(obj => Sum.Value = SubTransactions.Sum(stvw => stvw.Sum.Value))
                .AddTo(CompositeDisposable);
            SubTransactions.ObserveResetChanged()
                .Subscribe(obj => Sum.Value = SubTransactions.Sum(stvw => stvw.Sum.Value))
                .AddTo(CompositeDisposable);
            SubTransactions.ObserveElementObservableProperty(stvw => stvw.Sum)
                .Subscribe(obj => Sum.Value = SubTransactions.Sum(stvw => stvw.Sum.Value))
                .AddTo(CompositeDisposable);
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
                Orm.CommonPropertyProvider.CategoryViewModelService);
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
