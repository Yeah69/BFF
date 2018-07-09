using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using BFF.Helper;
using BFF.Helper.Extensions;
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
        ICategoryViewModel Parent { get; }

        IRxRelayCommand<ICategoryViewModel> MergeTo { get; }
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
            private readonly IRxSchedulerProvider _rxSchedulerProvider;

            public CategoryViewModelInitializer(
                Lazy<ICategoryViewModelService> service,
                IRxSchedulerProvider rxSchedulerProvider)
            {
                _service = service;
                _rxSchedulerProvider = rxSchedulerProvider;
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

                    viewModel.Parent = _service.Value.GetViewModel(viewModel._category.Parent);
                    viewModel._category
                        .ObservePropertyChanges(nameof(viewModel._category.Parent))
                        .ObserveOn(_rxSchedulerProvider.UI)
                        .Subscribe(_ =>
                        {
                            viewModel.Parent = _service.Value.GetViewModel(viewModel._category.Parent);
                            viewModel.OnPropertyChanged(nameof(viewModel.Parent));
                        })
                        .AddTo(viewModel.CompositeDisposable);
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
        public ICategoryViewModel Parent { get; private set; }

        public IRxRelayCommand<ICategoryViewModel> MergeTo { get; }

        public override string FullName => $"{(Parent != null ? $"{Parent.FullName}." : "")}{Name}";

        public override IEnumerable<ICategoryBaseViewModel> FullChainOfCategories
        {
            get
            {
                ICategoryViewModel current = this;
                IList<ICategoryBaseViewModel> temp = new List<ICategoryBaseViewModel> { current };
                while (current.Parent != null)
                {
                    current = current.Parent;
                    temp.Add(current);
                }
                return temp.Reverse();
            }
        }

        public override int Depth => Parent?.Depth + 1 ?? 0;

        public override string GetIndent()
        {
            return $"{Parent?.GetIndent()}. ";
        }

        public CategoryViewModel(
            ICategory category,
            IRxSchedulerProvider rxSchedulerProvider) : base(category, rxSchedulerProvider)
        {
            _category = category;

            MergeTo = new RxRelayCommand<ICategoryViewModel>(cvm =>
            {
                if (cvm is CategoryViewModel categoryViewModel)
                {
                    category.MergeTo(categoryViewModel._category);
                }
            });
        }
    }
}