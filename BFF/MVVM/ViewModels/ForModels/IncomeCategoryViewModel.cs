using System.Collections.Generic;
using BFF.Helper;
using BFF.MVVM.Models.Native;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IIncomeCategoryViewModel : ICategoryBaseViewModel
    {
        IReactiveProperty<int> MonthOffset { get; }
    }

    public class IncomeCategoryViewModel : CategoryBaseViewModel, IIncomeCategoryViewModel
    {
        public override string FullName => Name.Value;

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
            IRxSchedulerProvider schedulerProvider) : base(category, schedulerProvider)
        {
            MonthOffset = category
                .ToReactivePropertyAsSynchronized(c => c.MonthOffset, ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);
        }

        public IReactiveProperty<int> MonthOffset { get; }
    }
}