using System.Collections.Generic;
using System.Linq;
using BFF.DB.Dapper;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels.Structure;
using MuVaViMo;

namespace BFF.MVVM.ViewModelRepositories
{
    public abstract class ViewModelServiceBase<TDomain, TViewModel> 
        where TDomain : class, IDataModel 
        where TViewModel : class, IDataModelViewModel
    {
        private readonly IDictionary<TDomain, TViewModel> _modelToViewModel 
            = new Dictionary<TDomain, TViewModel>();
        private readonly IDictionary<TViewModel, TDomain> _viewModelToModel 
            = new Dictionary<TViewModel, TDomain>();
        public IObservableReadOnlyList<TViewModel> All { get; protected set; }

        protected ViewModelServiceBase(IObservableRepositoryBase<TDomain> repository, bool deferAllIntialization = false)
        {
            if(!deferAllIntialization)
                All = new TransformingObservableReadOnlyList<TDomain ,TViewModel>(
                    new WrappingObservableReadOnlyList<TDomain>(repository.All),
                    AddToDictionaries);
        }

        protected abstract TViewModel Create(TDomain model);

        protected TViewModel AddToDictionaries(TDomain category)
        {
            TViewModel categoryViewModel;
            if(!_modelToViewModel.ContainsKey(category))
            {
                categoryViewModel = Create(category);
                _modelToViewModel.Add(category, categoryViewModel);
            }
            else
                categoryViewModel = _modelToViewModel[category];
            if(!_viewModelToModel.ContainsKey(categoryViewModel))
                _viewModelToModel.Add(categoryViewModel, category);
            return categoryViewModel;
        }

        public TViewModel GetViewModel(TDomain category)
        {
            if(category == null) return null;
            if(_modelToViewModel.ContainsKey(category))
                return _modelToViewModel[category];

            return null;
        }

        public TViewModel GetViewModel(long id)
        {
            if(id < 1) return null;
            TDomain category = _viewModelToModel.Values.SingleOrDefault(c => c.Id == id);
            if(category != null && _modelToViewModel.ContainsKey(category))
                return _modelToViewModel[category];

            return null;
        }

        public TDomain GetModel(TViewModel categoryViewModel)
        {
            if(categoryViewModel == null) return null;
            if(_viewModelToModel.ContainsKey(categoryViewModel))
                return _viewModelToModel[categoryViewModel];

            return null;
        }
    }
}