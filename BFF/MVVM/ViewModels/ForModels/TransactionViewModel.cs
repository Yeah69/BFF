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
        private readonly ITransaction _transaction;
        private readonly ICategoryBaseViewModelService _categoryViewModelService;
        private ICategoryBaseViewModel _category;

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
            IRxSchedulerProvider rxSchedulerProvider,
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
                rxSchedulerProvider, 
                summaryAccountViewModel,
                flagViewModelService,
                owner)
        {
            _transaction = transaction;
            _categoryViewModelService = categoryViewModelService;

            _category = _categoryViewModelService.GetViewModel(transaction.Category);
            transaction
                .ObservePropertyChanges(nameof(transaction.Category))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ =>
                {
                    _category = _categoryViewModelService.GetViewModel(transaction.Category);
                    OnPropertyChanged(nameof(Category));
                })
                .AddTo(CompositeDisposable);

            this
                .ObservePropertyChanges(nameof(Category))
                .Subscribe(_ => SumSign = Category is IncomeCategoryViewModel ? Sign.Plus : Sign.Minus)
                .AddTo(CompositeDisposable);

            Sum = transaction.ToReactivePropertyAsSynchronized(
                nameof(transaction.Sum),
                () => transaction.Sum,
                s => transaction.Sum = s,
                rxSchedulerProvider.UI,
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            transaction
                .ObservePropertyChanges(t => t.Sum)
                .Where(_ => transaction.Id != -1)
                .Subscribe(sum => NotifyRelevantAccountsToRefreshBalance())
                .AddTo(CompositeDisposable);

            SumEdit = createSumEdit(Sum);

            NewCategoryViewModel = newCategoryViewModel;
        }
        
        public ICategoryBaseViewModel Category
        {
            get => _category;
            set => _transaction.Category = _categoryViewModelService.GetModel(value);
        }

        public override IReactiveProperty<long> Sum { get; }

        public override ISumEditViewModel SumEdit { get; }

        public INewCategoryViewModel NewCategoryViewModel { get; }

        public override bool IsInsertable() => base.IsInsertable() && Category.IsNotNull();
    }
}