using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IIncomeCategoryViewModel : ICategoryBaseViewModel
    {
        int MonthOffset { get; set; }
    }

    public class IncomeCategoryViewModel : CategoryBaseViewModel, IIncomeCategoryViewModel
    {
        private readonly IIncomeCategory _category;
        public override string FullName => Name;

        public override IEnumerable<ICategoryBaseViewModel> FullChainOfCategories
        {
            get
            {
                yield return this;
            }
        }

        public override int Depth { get; } = 0;

        public override string GetIndent() => "";

        public IncomeCategoryViewModel(
            IIncomeCategory category,
            IRxSchedulerProvider rxSchedulerProvider) : base(category, rxSchedulerProvider)
        {
            _category = category;

            category
                .ObservePropertyChanges(nameof(category.MonthOffset))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(MonthOffset)))
                .AddTo(CompositeDisposable);
        }

        public int MonthOffset { get => _category.MonthOffset; set => _category.MonthOffset = value; }
    }
}