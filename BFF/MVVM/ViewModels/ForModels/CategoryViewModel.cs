using System;
using System.Collections.Generic;
using System.Linq;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
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

    public interface ICategoryViewModelInitializer
    {
        void Initialize(IEnumerable<ICategoryViewModel> categoryViewModels);
        void Initialize(ICategoryViewModel categoryViewModel);
    }

    public class CategoryViewModel : CategoryBaseViewModel, ICategoryViewModel
    {
        private readonly ICategory _category;

        public class CategoryViewModelInitializer : ICategoryViewModelInitializer
        {
            private readonly Lazy<ICategoryViewModelService> _service;

            public CategoryViewModelInitializer(Lazy<ICategoryViewModelService> service)
            {
                _service = service;
            }

            public void Initialize(IEnumerable<ICategoryViewModel> categoryViewModels)
            {
                foreach (var categoryViewModel in categoryViewModels)
                {
                    Initialize(categoryViewModel);
                }
            }

            public void Initialize(ICategoryViewModel categoryViewModel)
            {
                if (categoryViewModel is CategoryViewModel viewModel)
                {
                    viewModel.Categories =
                        viewModel._category.Categories.ToReadOnlyReactiveCollection(_service.Value.GetViewModel).AddTo(viewModel.CompositeDisposable);
                    viewModel.Parent = viewModel._category.ToReactivePropertyAsSynchronized(
                        c => c.Parent,
                        _service.Value.GetViewModel,
                        _service.Value.GetModel,
                        ReactivePropertyMode.DistinctUntilChanged).AddTo(viewModel.CompositeDisposable);
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
        public IReactiveProperty<ICategoryViewModel> Parent { get; private set; }

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

        public override string GetIndent()
        {
            return $"{Parent.Value?.GetIndent()}. ";
        }

        public CategoryViewModel(ICategory category) : base(category)
        {
            _category = category;
        }
    }
}