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
    }

    /// <summary>
    /// The ViewModel of the Model Transaction.
    /// </summary>
    public sealed class TransactionViewModel : TransactionBaseViewModel, ITransactionViewModel
    {
        public TransactionViewModel(
            ITransaction transaction,
            INewCategoryViewModel newCategoryViewModel,
            INewPayeeViewModel newPayeeViewModel,
            INewFlagViewModel newFlagViewModelFactory,
            IAccountViewModelService accountViewModelService,
            IPayeeViewModelService payeeViewModelService,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            ICategoryBaseViewModelService categoryViewModelService,
            ILastSetDate lastSetDate,
            IRxSchedulerProvider schedulerProvider,
            ISummaryAccountViewModel summaryAccountViewModel,
            IFlagViewModelService flagViewModelService,
            IAccountBaseViewModel owner)
            : base(
                transaction, 
                newPayeeViewModel,
                newFlagViewModelFactory,
                accountViewModelService,
                payeeViewModelService,
                lastSetDate, 
                schedulerProvider, 
                summaryAccountViewModel,
                flagViewModelService,
                owner)
        {
            Category = transaction.ToReactivePropertyAsSynchronized(
                    nameof(transaction.Category),
                    () => transaction.Category,
                    c => transaction.Category = c,
                    categoryViewModelService.GetViewModel,
                    categoryViewModelService.GetModel,
                    schedulerProvider.UI,
                    ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            Category
                .Subscribe(c => SumSign = c is IncomeCategoryViewModel ? Sign.Plus : Sign.Minus)
                .AddTo(CompositeDisposable);

            Sum = transaction.ToReactivePropertyAsSynchronized(
                nameof(transaction.Sum),
                () => transaction.Sum,
                s => transaction.Sum = s,
                schedulerProvider.UI,
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            transaction
                .ObservePropertyChanges(t => t.Sum)
                .Where(_ => transaction.Id != -1)
                .Subscribe(sum => NotifyRelevantAccountsToRefreshBalance())
                .AddTo(CompositeDisposable);

            SumEdit = createSumEdit(Sum);

            NewCategoryViewModel = newCategoryViewModel;
        }
        
        public IReactiveProperty<ICategoryBaseViewModel> Category { get; }
        
        public override IReactiveProperty<long> Sum { get; }

        public override ISumEditViewModel SumEdit { get; }

        public INewCategoryViewModel NewCategoryViewModel { get; }

        public override bool IsInsertable() => base.IsInsertable() && Category.Value.IsNotNull();
    }
}