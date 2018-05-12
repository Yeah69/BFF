using System.Collections.Generic;
using BFF.Helper;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface ICategoryBaseViewModel : ICommonPropertyViewModel
    {
        string FullName { get; }
        IEnumerable<ICategoryBaseViewModel> FullChainOfCategories { get; }
        int Depth { get; }
        string GetIndent();
    }

    public abstract class CategoryBaseViewModel : CommonPropertyViewModel, ICategoryBaseViewModel
    {
        public abstract string FullName { get; }

        public abstract IEnumerable<ICategoryBaseViewModel> FullChainOfCategories { get; }

        public abstract int Depth { get; }

        public abstract string GetIndent();

        protected CategoryBaseViewModel(
            ICategoryBase category,
            IRxSchedulerProvider schedulerProvider) : base(category, schedulerProvider)
        {
        }
    }
}