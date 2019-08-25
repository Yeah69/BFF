using System;
using System.Collections.Specialized;
using System.Reactive.Linq;
using BFF.Core.Extensions;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.ViewModel.ViewModels.ForModels;
using MrMeeseeks.Extensions;
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
                repository.All,
                AddToDictionaries);
            AllCollectionInitialized = repository.AllAsync.ContinueWith(t => {});
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
                .AddForDisposalTo(CompositeDisposable);
        }
    }
}