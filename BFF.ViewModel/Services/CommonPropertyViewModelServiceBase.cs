using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

        protected readonly IDictionary<TDomain, TViewModel> ModelToViewModel 
            = new Dictionary<TDomain, TViewModel>();
        protected readonly IDictionary<TViewModel, TDomain> ViewModelToModel 
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
                        ModelToViewModel.Clear();
                        ViewModelToModel.Clear();
                    });
            }

            Disposable.Create(() =>
            {
                ModelToViewModel.Clear();
                ViewModelToModel.Clear();
            }).AddTo(CompositeDisposable);
        }

        protected TViewModel AddToDictionaries(TDomain model)
        {
            TViewModel viewModel;
            if(!ModelToViewModel.ContainsKey(model))
            {
                viewModel = _factory(model);
                ModelToViewModel.Add(model, viewModel);
            }
            else
                viewModel = ModelToViewModel[model];
            if(!ViewModelToModel.ContainsKey(viewModel))
                ViewModelToModel.Add(viewModel, model);
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