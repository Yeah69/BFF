using System;
using System.Collections.Concurrent;
using BFF.Model.Models.Structure;
using BFF.ViewModel.ViewModels.ForModels.Structure;

namespace BFF.ViewModel.Services
{
    public interface IModelToViewModelServiceBase<in TDomain, out TViewModel>
        where TDomain : class, IDataModel
        where TViewModel : class, IDataModelViewModel
    {
        TViewModel? GetViewModel(TDomain? model);
    }

    internal abstract class ModelToViewModelServiceBase<TDomain, TViewModel> : IModelToViewModelServiceBase<TDomain, TViewModel>
        where TDomain : class, IDataModel
        where TViewModel : class, IDataModelViewModel
    {
        /// <summary>
        /// Before removing the Lazy abstraction read https://andrewlock.net/making-getoradd-on-concurrentdictionary-thread-safe-using-lazy/
        /// In cases were two ViewModels are generated, the other wouldn't be garbage collected because it would subscribe to the given model.
        /// The Lazy objects prevent from generating one unnecessary ViewModel. Hence, eliminates this problem.
        /// </summary>
        private readonly ConcurrentDictionary<TDomain, Lazy<TViewModel>> _modelToViewModel
            = new();

        protected abstract TViewModel Create(TDomain model);

        public TViewModel? GetViewModel(TDomain? model)
        {
            if (model is null) return null;
            
            return _modelToViewModel.GetOrAdd(model, new Lazy<TViewModel>(() => Create(model))).Value;
        }
    }
}
