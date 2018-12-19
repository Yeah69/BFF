using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BFF.Core.Extensions;
using BFF.Core.IoC;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MuVaViMo;

namespace BFF.ViewModel.Services
{
    public interface ICommonPropertyViewModelServiceBase<TDomain, TViewModel> : IConvertingViewModelServiceBase<TDomain, TViewModel>, IOncePerBackend
        where TDomain : class, IDataModel
        where TViewModel : class, IDataModelViewModel
    {

        IObservableReadOnlyList<TViewModel> All { get; }
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

        protected readonly IDictionary<TDomain, TViewModel> _modelToViewModel 
            = new Dictionary<TDomain, TViewModel>();
        protected readonly IDictionary<TViewModel, TDomain> _viewModelToModel 
            = new Dictionary<TViewModel, TDomain>();

        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        public IObservableReadOnlyList<TViewModel> All { get; protected set; }

        protected CommonPropertyViewModelServiceBase(
            IObservableRepositoryBase<TDomain> repository, 
            Func<TDomain, TViewModel> factory, bool deferAll = false)
        {
            _factory = factory;
            if(!deferAll)
            {
                All = new TransformingObservableReadOnlyList<TDomain, TViewModel>(
                    new WrappingObservableReadOnlyList<TDomain>(repository.All),
                    AddToDictionaries);
                All
                    .ObserveCollectionChanges()
                    .Where(e => e.EventArgs.Action == NotifyCollectionChangedAction.Reset)
                    .Subscribe(_ =>
                    {
                        _modelToViewModel.Clear();
                        _viewModelToModel.Clear();
                    });
            }

            Disposable.Create(() =>
            {
                _modelToViewModel.Clear();
                _viewModelToModel.Clear();
            }).AddTo(CompositeDisposable);
        }

        protected TViewModel AddToDictionaries(TDomain model)
        {
            TViewModel viewModel;
            if(!_modelToViewModel.ContainsKey(model))
            {
                viewModel = _factory(model);
                _modelToViewModel.Add(model, viewModel);
            }
            else
                viewModel = _modelToViewModel[model];
            if(!_viewModelToModel.ContainsKey(viewModel))
                _viewModelToModel.Add(viewModel, model);
            return viewModel;
        }

        public virtual TViewModel GetViewModel(TDomain model)
        {
            if(model is null) return null;
            if(!_modelToViewModel.ContainsKey(model))
                AddToDictionaries(model);

            return _modelToViewModel[model];
        }

        public TViewModel GetViewModel(long id)
        {
            if(id < 1) return null;
            TDomain model = _viewModelToModel.Values.SingleOrDefault(c => c.Id == id);

            return GetViewModel(model);
        }

        public TDomain GetModel(TViewModel viewModel)
        {
            if(viewModel is null) return null;
            if(_viewModelToModel.ContainsKey(viewModel))
                return _viewModelToModel[viewModel];

            return null;
        }

        public void Dispose()
        {
            CompositeDisposable.Dispose();
        }
    }
}