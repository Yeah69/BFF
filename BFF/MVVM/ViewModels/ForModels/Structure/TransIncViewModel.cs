using System;
using System.Reactive.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITransIncViewModel : ITransIncBaseViewModel, IHaveCategoryViewModel
    {
        INewCategoryViewModel NewCategoryViewModel { get; }
    }

    /// <summary>
    /// Base class for ViewModels of Transaction and Income
    /// </summary>
    public abstract class TransIncViewModel : TransIncBaseViewModel, ITransIncViewModel
    {
        /// <summary>
        /// Each Transaction or Income can be budgeted to a category.
        /// </summary>
        public IReactiveProperty<ICategoryViewModel> Category { get; }

        /// <summary>
        /// The amount of money of the exchange of the Transaction or Income.
        /// </summary>
        public override IReactiveProperty<long> Sum { get; }
        public INewCategoryViewModel NewCategoryViewModel { get; }

        /// <summary>
        /// Initializes a TransIncViewModel.
        /// </summary>
        /// <param name="transInc">The associated Model of this ViewModel.</param>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="accountViewModelService">Service of accounts.</param>
        /// <param name="payeeViewModelService">Service of payees.</param>
        /// <param name="categoryViewModelService">Service of categories.</param>
        protected TransIncViewModel(
            ITransInc transInc,
            Func<IHaveCategoryViewModel, INewCategoryViewModel> newCategoryViewModelFactory,
            Func<IHavePayeeViewModel, INewPayeeViewModel> newPayeeViewModelFactory,
            IBffOrm orm, 
            AccountViewModelService accountViewModelService,
            PayeeViewModelService payeeViewModelService, 
            CategoryViewModelService categoryViewModelService)
            : base(orm, transInc, newPayeeViewModelFactory, accountViewModelService, payeeViewModelService)
        {
            Category = transInc.ToReactivePropertyAsSynchronized(
                ti => ti.Category, 
                categoryViewModelService.GetViewModel, 
                categoryViewModelService.GetModel)
                .AddTo(CompositeDisposable);

            Sum = transInc.ToReactivePropertyAsSynchronized(ti => ti.Sum).AddTo(CompositeDisposable);

            Sum.Skip(1)
                .Subscribe(sum => NotifyRelevantAccountsToRefreshBalance())
                .AddTo(CompositeDisposable);

            NewCategoryViewModel = newCategoryViewModelFactory(this);
        }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        public override bool ValidToInsert()
        {
            return Account != null && Payee != null && Category != null;
        }
    }
}
