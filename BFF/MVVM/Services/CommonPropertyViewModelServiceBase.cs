﻿using System;
using System.Collections.Generic;
using System.Linq;
using BFF.DB;
using BFF.DB.Dapper;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels.Structure;
using MuVaViMo;

namespace BFF.MVVM.Services
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

    public abstract class CommonPropertyViewModelServiceBase<TDomain, TViewModel> : ICommonPropertyViewModelServiceBase<TDomain, TViewModel>
        where TDomain : class, IDataModel 
        where TViewModel : class, IDataModelViewModel
    {
        private readonly Func<TDomain, TViewModel> _factory;

        private readonly IDictionary<TDomain, TViewModel> _modelToViewModel 
            = new Dictionary<TDomain, TViewModel>();
        private readonly IDictionary<TViewModel, TDomain> _viewModelToModel 
            = new Dictionary<TViewModel, TDomain>();

        public IObservableReadOnlyList<TViewModel> All { get; protected set; }

        protected CommonPropertyViewModelServiceBase(
            IObservableRepositoryBase<TDomain> repository, 
            Func<TDomain, TViewModel> factory, bool deferAll = false)
        {
            _factory = factory;
            if(!deferAll)
                All = new TransformingObservableReadOnlyList<TDomain, TViewModel>(
                    new WrappingObservableReadOnlyList<TDomain>(repository.All),
                    AddToDictionaries);
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
            if(_modelToViewModel.ContainsKey(model))
                return _modelToViewModel[model];

            return null;
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
    }
}