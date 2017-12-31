using System.Collections.Generic;
using System.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface ICategoryBaseViewModel : ICommonPropertyViewModel
    {
        string FullName { get; }
        IEnumerable<ICategoryBaseViewModel> FullChainOfCategories { get; }
        int Depth { get; }
        string GetIndent();
    }
    public interface IIncomeCategoryViewModel : ICategoryBaseViewModel
    {
    }

    public interface ICategoryViewModel : ICategoryBaseViewModel
    {
        /// <summary>
        /// The Child-Categories
        /// </summary>
        ReadOnlyReactiveCollection<ICategoryViewModel> Categories { get; }

        /// <summary>
        /// The Parent
        /// </summary>
        IReactiveProperty<ICategoryViewModel> Parent { get; }
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

        protected CategoryBaseViewModel(ICategoryBase category, IBffOrm orm) : base(orm, category)
        {
        }
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

        public IncomeCategoryViewModel(IIncomeCategory category, IBffOrm orm) : base(category, orm)
        {
        }

        #region Overrides of DataModelViewModel

        public override bool ValidToInsert()
        {
            return !string.IsNullOrWhiteSpace(Name.Value);
        }

        #endregion
    }

    public class CategoryViewModel : CategoryBaseViewModel, ICategoryViewModel
    {
        private readonly ICategory _category;

        public class CategoryViewModelInitializer
        {
            private readonly ICategoryViewModelService _service;

            public CategoryViewModelInitializer(ICategoryViewModelService service)
            {
                _service = service;
            }

            public void Initialize(IEnumerable<ICategoryViewModel> categoryViewModels)
            {
                foreach (var categoryViewModel in categoryViewModels.OfType<CategoryViewModel>())
                {
                    categoryViewModel.Categories =
                        categoryViewModel._category.Categories.ToReadOnlyReactiveCollection(_service.GetViewModel);
                }
            }
        }

        /// <summary>
        /// The Child-Categories
        /// </summary>
        public ReadOnlyReactiveCollection<ICategoryViewModel> Categories { get; private set; }

        /// <summary>
        /// The Parent
        /// </summary>
        public IReactiveProperty<ICategoryViewModel> Parent { get; }

        public override string FullName => $"{(Parent.Value != null ? $"{Parent.Value.FullName}." : "")}{Name.Value}";

        public override IEnumerable<ICategoryBaseViewModel> FullChainOfCategories
        {
            get
            {
                ICategoryViewModel current = this;
                IList<ICategoryBaseViewModel> temp = new List<ICategoryBaseViewModel> { current };
                while (current.Parent.Value != null)
                {
                    current = current.Parent.Value;
                    temp.Add(current);
                }
                return temp.Reverse();
            }
        }

        public override int Depth => Parent.Value?.Depth + 1 ?? 0;

        /// <summary>
        /// Representing string
        /// </summary>
        /// <returns>Name with preceding dots (foreach Ancestor one)</returns>
        public override string ToString()
        {
            return Name.Value;
        }

        public override string GetIndent()
        {
            return $"{Parent.Value?.GetIndent()}. ";
        }

        public CategoryViewModel(ICategory category, IBffOrm orm, ICategoryViewModelService service) : base(category, orm)
        {
            _category = category;
            Parent = category.ToReactivePropertyAsSynchronized(
                c => c.Parent,
                service.GetViewModel,
                service.GetModel, 
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);
        }

        #region Overrides of DataModelViewModel

        public override bool ValidToInsert()
        {
            return !string.IsNullOrWhiteSpace(Name.Value);
        }

        #endregion
    }
}