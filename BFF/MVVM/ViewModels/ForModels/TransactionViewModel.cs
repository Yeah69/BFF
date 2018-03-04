using System;
using System.Reactive.Linq;
using BFF.Helper;
using BFF.Helper.Extensions;
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
        
        public IReactiveProperty<ICategoryBaseViewModel> Category { get; }
        
        public override IReactiveProperty<long> Sum { get; }

        public override ISumEditViewModel SumEdit { get; }

        public INewCategoryViewModel NewCategoryViewModel { get; }

        public override bool IsInsertable() => base.IsInsertable() && Category.Value.IsNotNull();
    }
}