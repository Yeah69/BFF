﻿using System;
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
    public class TransactionViewModel : TransactionBaseViewModel, ITransactionViewModel
    {
        /// <summary>
        /// Initializes a TransactionViewModel.
        /// </summary>
        /// <param name="transaction">A Transaction Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="accountViewModelService">Service of accounts.</param>
        /// <param name="payeeViewModelService">Service of payees.</param>
        /// <param name="categoryViewModelService">Service of categories.</param>
        public TransactionViewModel(
            ITransaction transaction,
            Func<IHaveCategoryViewModel, INewCategoryViewModel> newCategoryViewModelFactory,
            Func<IHavePayeeViewModel, INewPayeeViewModel> newPayeeViewModelFactory,
            IAccountViewModelService accountViewModelService,
            IPayeeViewModelService payeeViewModelService,
            ICategoryBaseViewModelService categoryViewModelService,
            IFlagViewModelService flagViewModelService)
            : base(transaction, newPayeeViewModelFactory, accountViewModelService, payeeViewModelService, flagViewModelService)
        {
            Category = transaction.ToReactivePropertyAsSynchronized(
                    ti => ti.Category,
                    categoryViewModelService.GetViewModel,
                    categoryViewModelService.GetModel,
                    ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            Sum = transaction.ToReactivePropertyAsSynchronized(ti => ti.Sum, ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            Sum.Subscribe(sum => NotifyRelevantAccountsToRefreshBalance())
                .AddTo(CompositeDisposable);

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

        public INewCategoryViewModel NewCategoryViewModel { get; }
    }
}