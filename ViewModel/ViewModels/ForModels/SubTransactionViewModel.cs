using System;
using System.Reactive.Linq;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.ResXToViewModelGenerator;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels.ForModels
{
    public interface ISubTransactionViewModel : ITransLikeViewModel, IHaveCategoryViewModel
    {
    }

    internal sealed class SubTransactionViewModel : TransLikeViewModel, ISubTransactionViewModel
    {
        private readonly ISubTransaction _subTransaction;
        private readonly ICategoryBaseViewModelService _categoryViewModelService;
        private readonly ICurrentTextsViewModel _currentTextsViewModel;
        private ICategoryBaseViewModel? _category;

        public SubTransactionViewModel(
            ISubTransaction subTransaction,
            INewCategoryViewModel newCategoryViewModel,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            IRxSchedulerProvider rxSchedulerProvider,
            ICategoryBaseViewModelService categoryViewModelService,
            ICurrentTextsViewModel currentTextsViewModel,
            IAccountBaseViewModel? owner)
            : base(subTransaction, rxSchedulerProvider, owner)
        {
            _subTransaction = subTransaction;
            _categoryViewModelService = categoryViewModelService;
            _currentTextsViewModel = currentTextsViewModel;

            _category = _categoryViewModelService.GetViewModel(subTransaction.Category);
            subTransaction
                .ObservePropertyChanged(nameof(subTransaction.Category))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ =>
                {
                    _category = _categoryViewModelService.GetViewModel(subTransaction.Category);
                    OnPropertyChanged(nameof(Category));
                    if (_category is not null)
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
        public ICategoryBaseViewModel? Category
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

            SetErrors(_currentTextsViewModel.CurrentTexts.ErrorMessageEmptyCategory
                .ToEnumerable(), nameof(Category));
            OnErrorChanged(nameof(Category));
        }
    }
}