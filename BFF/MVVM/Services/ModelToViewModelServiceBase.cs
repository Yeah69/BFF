using System.Collections.Generic;
using System.Linq;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.Services
{
    public abstract class ModelToViewModelServiceBase<TDomain, TViewModel> 
        where TDomain : class, IDataModel
        where TViewModel : class, IDataModelViewModel
    {
        private readonly IDictionary<TDomain, TViewModel> _modelToViewModel
            = new Dictionary<TDomain, TViewModel>();

        protected abstract TViewModel Create(TDomain model);

        protected TViewModel AddToDictionaries(TDomain model)
        {
            TViewModel viewModel;
            if (!_modelToViewModel.ContainsKey(model))
            {
                viewModel = Create(model);
                _modelToViewModel.Add(model, viewModel);
            }
            else
                viewModel = _modelToViewModel[model];
            return viewModel;
        }

        public TViewModel GetViewModel(TDomain model)
        {
            if (model == null) return null;
            if (_modelToViewModel.ContainsKey(model))
                return _modelToViewModel[model];

            return null;
        }

        public TViewModel GetViewModel(long id)
        {
            if (id < 1) return null;
            TDomain model = _modelToViewModel.Keys.SingleOrDefault(c => c.Id == id);
            if (model != null)
                return _modelToViewModel[model];

            return null;
        }
    }
}
