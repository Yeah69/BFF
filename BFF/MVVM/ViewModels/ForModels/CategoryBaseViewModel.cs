using System.Collections.Generic;
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

        /// <summary>
        /// Representing string
        /// </summary>
        /// <returns>Name with preceding dots (foreach Ancestor one)</returns>
        public override string ToString() => Name.Value;

        public abstract string GetIndent();

        protected CategoryBaseViewModel(ICategoryBase category) : base(category)
        {
        }
    }
}