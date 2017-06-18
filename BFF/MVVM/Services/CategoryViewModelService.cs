using System.Collections.Generic;
using System.Linq;
using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.MVVM.ViewModelRepositories
{
    public class CategoryViewModelService
    {
        private readonly CategoryRepository _repository;
        private readonly IBffOrm _orm;

        private readonly IDictionary<ICategory, ICategoryViewModel> modelToViewModel 
            = new Dictionary<ICategory, ICategoryViewModel>();
        private readonly IDictionary<ICategoryViewModel, ICategory> viewModelToModel 
            = new Dictionary<ICategoryViewModel, ICategory>();
        public IObservableReadOnlyList<ICategoryViewModel> All { get; }

        public CategoryViewModelService(CategoryRepository repository, IBffOrm orm)
        {
            _repository = repository;
            _orm = orm;
            
            All = new TransformingObservableReadOnlyList<ICategory ,ICategoryViewModel>(
                new WrappingObservableReadOnlyList<Category>(_repository.All),
                AddToDictionaries);
            
        }

        private ICategoryViewModel AddToDictionaries(ICategory category)
        {
            ICategoryViewModel categoryViewModel;
            if(!modelToViewModel.ContainsKey(category))
            {
                categoryViewModel = new CategoryViewModel(category, _orm, this);
                modelToViewModel.Add(category, categoryViewModel);
            }
            else
                categoryViewModel = modelToViewModel[category];
            if(!viewModelToModel.ContainsKey(categoryViewModel))
                viewModelToModel.Add(categoryViewModel, category);
            return categoryViewModel;
        }

        public ICategoryViewModel GetViewModel(ICategory category)
        {
            if(category == null) return null;
            if(modelToViewModel.ContainsKey(category))
                return modelToViewModel[category];

            return null;
        }

        public ICategoryViewModel GetViewModel(long id)
        {
            if(id < 1) return null;
            ICategory category = viewModelToModel.Values.SingleOrDefault(c => c.Id == id);
            if(category != null && modelToViewModel.ContainsKey(category))
                return modelToViewModel[category];

            return null;
        }

        public ICategory GetModel(ICategoryViewModel categoryViewModel)
        {
            if(categoryViewModel == null) return null;
            if(viewModelToModel.ContainsKey(categoryViewModel))
                return viewModelToModel[categoryViewModel];

            return null;
        }

    }
}