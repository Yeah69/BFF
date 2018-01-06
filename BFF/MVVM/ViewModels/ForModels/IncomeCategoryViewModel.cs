using System.Collections.Generic;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IIncomeCategoryViewModel : ICategoryBaseViewModel
    {
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

        public IncomeCategoryViewModel(IIncomeCategory category) : base(category)
        {
        }
    }
}