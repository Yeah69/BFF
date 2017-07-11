using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    public class CategoryViewModelService : ViewModelServiceBase<Category, CategoryViewModel>
    {
        private readonly CategoryRepository _repository;
        private readonly IBffOrm _orm;

        public CategoryViewModelService(CategoryRepository repository, IBffOrm orm) : base(repository)
        {
            _repository = repository;
            _orm = orm;
        }

        protected override CategoryViewModel Create(Category model) 
            => new CategoryViewModel(model, _orm, this);
        public override CategoryViewModel GetNewNonInsertedViewModel() 
            => new CategoryViewModel(_repository.Create(), _orm, this);
    }
}