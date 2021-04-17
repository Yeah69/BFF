using System.Collections.Generic;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.ViewModel.ViewModels.ForModels.Structure;

namespace BFF.ViewModel.ViewModels.ForModels
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
            IRxSchedulerProvider rxSchedulerProvider) : base(category, rxSchedulerProvider)
        {
        }
    }
}