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

        public TViewModel GetViewModel(TDomain model)
        {
            if (model == null) return null;
            if (!_modelToViewModel.ContainsKey(model))
                _modelToViewModel[model] = Create(model);

            return _modelToViewModel[model];
        }
    }
}
