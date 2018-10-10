using System;
using System.Reactive.Linq;
using BFF.Core;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface ISubTransactionViewModel : ITransLikeViewModel, IHaveCategoryViewModel
    {
    }
    
    public sealed class SubTransactionViewModel : TransLikeViewModel, ISubTransactionViewModel
    {
        private readonly ISubTransaction _subTransaction;
        private readonly ICategoryBaseViewModelService _categoryViewModelService;
        private ICategoryBaseViewModel _category;

        public SubTransactionViewModel(
            ISubTransaction subTransaction,
            INewCategoryViewModel newCategoryViewModel,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            IRxSchedulerProvider rxSchedulerProvider,
            ICategoryBaseViewModelService categoryViewModelService,
            IAccountBaseViewModel owner)
            : base(subTransaction, rxSchedulerProvider, owner)
        {
            _subTransaction = subTransaction;
            _categoryViewModelService = categoryViewModelService;

            _category = _categoryViewModelService.GetViewModel(subTransaction.Category);
            subTransaction
                .ObservePropertyChanges(nameof(subTransaction.Category))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ =>
                {
                    _category = _categoryViewModelService.GetViewModel(subTransaction.Category);
                    OnPropertyChanged(nameof(Category));
                    if (_category != null)
                    {
                        ClearErrors(nameof(Category));
                        OnErrorChanged(nameof(Category));
                    }
                })
                .AddTo(CompositeDisposable);

            Sum = subTransaction.ToReactivePropertyAsSynchronized(sti => sti.Sum, ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            SumEdit = createSumEdit(Sum);

            NewCategoryViewModel = newCategoryViewModel;
        }

        public INewCategoryViewModel NewCategoryViewModel { get; }

        /// <summary>
        /// Each SubTransaction can be budgeted to a category.
        /// </summary>
        public ICategoryBaseViewModel Category
        {
            get => _category;
            set => _subTransaction.Category = _categoryViewModelService.GetModel(value);
        }

        /// <summary>
        /// The amount of money of the exchange of the SubTransaction.
        /// </summary>
        public override IReactiveProperty<long> Sum { get; }

        public override ISumEditViewModel SumEdit { get; }

        public override bool IsInsertable() => base.IsInsertable() && Category.IsNotNull();

        public override void NotifyErrorsIfAny()
        {
            if (!(Category is null)) return;

            SetErrors("ErrorMessageEmptyCategory".Localize().ToEnumerable(), nameof(Category));
            OnErrorChanged(nameof(Category));
        }
    }
}