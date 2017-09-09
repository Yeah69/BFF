using System.Collections.Generic;
using System.Linq;
using BFF.DB.Dapper;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels.Structure;
using MuVaViMo;

namespace BFF.MVVM.Services
{
    public abstract class CommonPropertyViewModelServiceBase<TDomain, TViewModel> 
        where TDomain : class, IDataModel 
        where TViewModel : class, IDataModelViewModel
    {
        private readonly IDictionary<TDomain, TViewModel> _modelToViewModel 
            = new Dictionary<TDomain, TViewModel>();
        private readonly IDictionary<TViewModel, TDomain> _viewModelToModel 
            = new Dictionary<TViewModel, TDomain>();
        public IObservableReadOnlyList<TViewModel> All { get; protected set; }

        protected CommonPropertyViewModelServiceBase(IObservableRepositoryBase<TDomain> repository, bool deferAllInitialization = false)
        {
            if(!deferAllInitialization)
                All = new TransformingObservableReadOnlyList<TDomain ,TViewModel>(
                    new WrappingObservableReadOnlyList<TDomain>(repository.All),
                    AddToDictionaries);
        }

        protected abstract TViewModel Create(TDomain model);

        protected TViewModel AddToDictionaries(TDomain model)
        {
            TViewModel categoryViewModel;
            if(!_modelToViewModel.ContainsKey(model))
            {
                categoryViewModel = Create(model);
                _modelToViewModel.Add(model, categoryViewModel);
            }
            else
                categoryViewModel = _modelToViewModel[model];
            if(!_viewModelToModel.ContainsKey(categoryViewModel))
                _viewModelToModel.Add(categoryViewModel, model);
            return categoryViewModel;
        }

        public virtual TViewModel GetViewModel(TDomain model)
        {
            if(model == null) return null;
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
            if(viewModel == null) return null;
            if(_viewModelToModel.ContainsKey(viewModel))
                return _viewModelToModel[viewModel];

            return null;
        }

        public abstract TViewModel GetNewNonInsertedViewModel();
    }
}