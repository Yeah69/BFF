using System;
using System.Reactive.Linq;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels.ForModels
{
    public interface ITransactionViewModel : ITransactionBaseViewModel, IHaveCategoryViewModel
    {
        IRxRelayCommand NonInsertedConvertToParentTransaction { get; }
        IRxRelayCommand NonInsertedConvertToTransfer { get; }
        IRxRelayCommand InsertedConvertToParentTransaction { get; }
    }

    /// <summary>
    /// The ViewModel of the Model Transaction.
    /// </summary>
    public sealed class TransactionViewModel : TransactionBaseViewModel, ITransactionViewModel
    {
        private readonly ITransaction _transaction;
        private readonly ILocalizer _localizer;
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
            ILocalizer localizer,
            ICategoryBaseViewModelService categoryViewModelService,
            ITransTransformingManager transTransformingManager,
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
                localizer,
                rxSchedulerProvider, 
                summaryAccountViewModel,
                flagViewModelService,
                owner)
        {
            _transaction = transaction;
            _localizer = localizer;
            _categoryViewModelService = categoryViewModelService;

            _category = _categoryViewModelService.GetViewModel(transaction.Category);
            transaction
                .ObservePropertyChanged(nameof(transaction.Category))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ =>
                {
                    _category = _categoryViewModelService.GetViewModel(transaction.Category);
                    OnPropertyChanged(nameof(Category));
                    if (_category != null)
                    {
                        ClearErrors(nameof(Category));
                        OnErrorChanged(nameof(Category));
                    }
                })
                .AddTo(CompositeDisposable);

            this
                .ObservePropertyChanged(nameof(Category))
                .Subscribe(_ => SumSign = Category is IncomeCategoryViewModel ? Sign.Plus : Sign.Minus)
                .AddTo(CompositeDisposable);

            Sum = transaction.ToReactivePropertyAsSynchronized(
                nameof(transaction.Sum),
                () => transaction.Sum,
                s => transaction.Sum = s,
                rxSchedulerProvider.UI,
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            transaction
                .ObservePropertyChanged(nameof(transaction.Sum))
                .Where(_ => transaction.IsInserted)
                .Subscribe(sum => NotifyRelevantAccountsToRefreshBalance())
                .AddTo(CompositeDisposable);

            SumEdit = createSumEdit(Sum);

            NewCategoryViewModel = newCategoryViewModel;

            NonInsertedConvertToParentTransaction = new RxRelayCommand(
                    () => Owner.ReplaceNewTrans(
                        this,
                        transTransformingManager.NotInsertedToParentTransactionViewModel(this)))
                .AddTo(CompositeDisposable);

            NonInsertedConvertToTransfer = new RxRelayCommand(
                    () => Owner.ReplaceNewTrans(
                        this,
                        transTransformingManager.NotInsertedToTransferViewModel(this)))
                .AddTo(CompositeDisposable);

            InsertedConvertToParentTransaction = new AsyncRxRelayCommand(
                    async () =>
                    {
                        var parentTransactionViewModel = transTransformingManager.InsertedToParentTransactionViewModel(this);
                        await transaction.DeleteAsync();
                        await parentTransactionViewModel.InsertAsync();
                        NotifyRelevantAccountsToRefreshTrans();
                    })
                .AddTo(CompositeDisposable);
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
        public IRxRelayCommand NonInsertedConvertToParentTransaction { get; }
        public IRxRelayCommand NonInsertedConvertToTransfer { get; }
        public IRxRelayCommand InsertedConvertToParentTransaction { get; }

        public override void NotifyErrorsIfAny()
        {
            base.NotifyErrorsIfAny();

            if (!(Category is null)) return;

            SetErrors(_localizer.Localize("ErrorMessageEmptyCategory").ToEnumerable(), nameof(Category));
            OnErrorChanged(nameof(Category));
        }
    }
}