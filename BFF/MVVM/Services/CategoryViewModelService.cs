using System;
using System.Collections.Specialized;
using System.Reactive.Linq;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.MVVM.Services
{
    public interface ICategoryViewModelService : ICommonPropertyViewModelServiceBase<ICategory, ICategoryViewModel>
    {
    }

    public class CategoryViewModelService : CommonPropertyViewModelServiceBase<ICategory, ICategoryViewModel>, ICategoryViewModelService
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
                .ObservePropertyChanges()
                .Where(e => e.EventArgs.Action == NotifyCollectionChangedAction.Reset)
                .Subscribe(_ =>
                {
                    _modelToViewModel.Clear();
                    _viewModelToModel.Clear();
                });

            repository
                .ObserveResetAll
                .Subscribe(cs => categoryViewModelInitializer.Initialize(All))
                .AddTo(CompositeDisposable);
        }
    }
}