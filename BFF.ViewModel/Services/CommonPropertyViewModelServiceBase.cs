using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.IoC;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MrMeeseeks.Extensions;
using MuVaViMo;

namespace BFF.ViewModel.Services
{
    public interface ICommonPropertyViewModelServiceBase<TDomain, TViewModel> : IConvertingViewModelServiceBase<TDomain, TViewModel>, IOncePerBackend
        where TDomain : class, IDataModel
        where TViewModel : class, IDataModelViewModel
    {

        IObservableReadOnlyList<TViewModel> All { get; }
        Task AllCollectionInitialized { get; }
    }

    public interface IConvertingViewModelServiceBase<TDomain, TViewModel>
        where TDomain : class, IDataModel
        where TViewModel : class, IDataModelViewModel
    {
        TViewModel GetViewModel(TDomain model);
        TDomain GetModel(TViewModel viewModel);
    }

    internal abstract class CommonPropertyViewModelServiceBase<TDomain, TViewModel> : ICommonPropertyViewModelServiceBase<TDomain, TViewModel>, IDisposable
        where TDomain : class, IDataModel 
        where TViewModel : class, IDataModelViewModel
    {
        private readonly Func<TDomain, TViewModel> _factory;

        protected readonly ConcurrentDictionary<TDomain, TViewModel> ModelToViewModel 
            = new ConcurrentDictionary<TDomain, TViewModel>();
        protected readonly ConcurrentDictionary<TViewModel, TDomain> ViewModelToModel 
            = new ConcurrentDictionary<TViewModel, TDomain>();

        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        public IObservableReadOnlyList<TViewModel> All { get; protected set; }
        public Task AllCollectionInitialized { get; protected set; }

        protected CommonPropertyViewModelServiceBase(
            IObservableRepositoryBase<TDomain> repository, 
            Func<TDomain, TViewModel> factory, bool deferAll = false)
        {
            _factory = factory;
            if(!deferAll)
            {
                All = new TransformingObservableReadOnlyList<TDomain, TViewModel>(
                    repository.All,
                    AddToDictionaries);
                AllCollectionInitialized = repository.AllAsync;
                All
                    .ObserveCollectionChanges()
                    .Where(e => e.EventArgs.Action == NotifyCollectionChangedAction.Reset)
                    .Subscribe(_ =>
                    {
                        ModelToViewModel.Clear();
                        ViewModelToModel.Clear();
                    });
            }

            Disposable.Create(() =>
            {
                ModelToViewModel.Clear();
                ViewModelToModel.Clear();
            }).AddForDisposalTo(CompositeDisposable);
        }

        protected TViewModel AddToDictionaries(TDomain model)
        {
            TViewModel viewModel = ModelToViewModel.GetOrAdd(model, _factory(model));
            ViewModelToModel.GetOrAdd(viewModel, model);
            return viewModel;
        }

        public virtual TViewModel GetViewModel(TDomain model)
        {
            if(model is null) return null;
            if(!ModelToViewModel.ContainsKey(model))
                AddToDictionaries(model);

            return ModelToViewModel[model];
        }

        public TDomain GetModel(TViewModel viewModel)
        {
            if(viewModel is null) return null;
            if(ViewModelToModel.ContainsKey(viewModel))
                return ViewModelToModel[viewModel];

            return null;
        }

        public void Dispose()
        {
            CompositeDisposable.Dispose();
        }
    }
}