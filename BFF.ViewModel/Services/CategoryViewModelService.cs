using System;
using System.Collections.Specialized;
using System.Reactive.Linq;
using BFF.Core.Extensions;
using BFF.Model.Models;
using BFF.Model.Repositories.ModelRepositories;
using BFF.ViewModel.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.ViewModel.Services
{
    public interface ICategoryViewModelService : ICommonPropertyViewModelServiceBase<ICategory, ICategoryViewModel>
    {
    }

    internal class CategoryViewModelService : CommonPropertyViewModelServiceBase<ICategory, ICategoryViewModel>, ICategoryViewModelService
    {
        public CategoryViewModelService(
            ICategoryRepository repository,
            ICategoryViewModelInitializer categoryViewModelInitializer,
            Func<ICategory, ICategoryViewModel> factory) : base(repository, factory, true)
        {
            All = new TransformingObservableReadOnlyList<ICategory, ICategoryViewModel>(
                new WrappingObservableReadOnlyList<ICategory>(repository.All),
                AddToDictionaries);
            All
                .ObserveCollectionChanges()
                .Where(e => e.EventArgs.Action == NotifyCollectionChangedAction.Reset)
                .Subscribe(_ =>
                {
                    ModelToViewModel.Clear();
                    ViewModelToModel.Clear();
                });

            repository
                .ObserveResetAll
                .Subscribe(cs => categoryViewModelInitializer.Initialize(All))
                .AddTo(CompositeDisposable);
        }
    }
}