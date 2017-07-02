using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.ViewModelRepositories
{
    public class CategoryViewModelService : ViewModelServiceBase<Category, CategoryViewModel>
    {
        private readonly IBffOrm _orm;

        public CategoryViewModelService(CategoryRepository repository, IBffOrm orm) : base(repository)
        {
            _orm = orm;
        }

        protected override CategoryViewModel Create(Category model) => new CategoryViewModel(model, _orm, this);

    }
}