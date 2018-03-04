using System;
using System.Reactive.Linq;
using BFF.Helper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface ITransactionViewModel : ITransactionBaseViewModel, IHaveCategoryViewModel
    {
        INewCategoryViewModel NewCategoryViewModel { get; }
    }

    /// <summary>
    /// The ViewModel of the Model Transaction.
    /// </summary>
    public sealed class TransactionViewModel : TransactionBaseViewModel, ITransactionViewModel
    {
        /// <summary>
        /// Initializes a TransactionViewModel.
        /// </summary>
        /// <param name="transaction">A Transaction Model.</param>
        /// <param name="newPayeeViewModelFactory">Creates a payee factory.</param>
        /// <param name="accountViewModelService">Service of accounts.</param>
        /// <param name="payeeViewModelService">Service of payees.</param>
        /// <param name="createSumEdit">Creates a sum editing viewmodel.</param>
        /// <param name="categoryViewModelService">Service of categories.</param>
        /// <param name="flagViewModelService">Fetches flags.</param>
        /// <param name="newCategoryViewModelFactory">Creates a category factory.</param>
        public TransactionViewModel(
            ITransaction transaction,
            Func<IHaveCategoryViewModel, INewCategoryViewModel> newCategoryViewModelFactory,
            Func<IHavePayeeViewModel, INewPayeeViewModel> newPayeeViewModelFactory,
            Func<IHaveFlagViewModel, INewFlagViewModel> newFlagViewModelFactory,
            IAccountViewModelService accountViewModelService,
            IPayeeViewModelService payeeViewModelService,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            ICategoryBaseViewModelService categoryViewModelService,
            IFlagViewModelService flagViewModelService)
            : base(transaction, newPayeeViewModelFactory, newFlagViewModelFactory, accountViewModelService, payeeViewModelService, flagViewModelService)
        {
            Category = transaction.ToReactivePropertyAsSynchronized(
                    ti => ti.Category,
                    categoryViewModelService.GetViewModel,
                    categoryViewModelService.GetModel,
                    ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            Category
                .Subscribe(c => SumSign = c is IncomeCategoryViewModel ? Sign.Plus : Sign.Minus)
                .AddTo(CompositeDisposable);

            Sum = transaction.ToReactivePropertyAsSynchronized(ti => ti.Sum, ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            Sum.Where(_ => transaction.Id != -1)
                .Subscribe(sum => NotifyRelevantAccountsToRefreshBalance())
                .AddTo(CompositeDisposable);

            SumEdit = createSumEdit(Sum);

            NewCategoryViewModel = newCategoryViewModelFactory(this);
        }

        /// <summary>
        /// Each Transaction can be budgeted to a category.
        /// </summary>
        public IReactiveProperty<ICategoryBaseViewModel> Category { get; }

        /// <summary>
        /// The amount of money of the exchange of the Transaction.
        /// </summary>
        public override IReactiveProperty<long> Sum { get; }

        public override ISumEditViewModel SumEdit { get; }

        public INewCategoryViewModel NewCategoryViewModel { get; }
    }
}