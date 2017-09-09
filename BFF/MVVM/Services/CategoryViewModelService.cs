using System.Reactive.Disposables;
using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.Services
{
    public class CategoryViewModelService : CommonPropertyViewModelServiceBase<ICategory, ICategoryViewModel>
    {
        private readonly ICategoryRepository _repository;
        private readonly IBffOrm _orm;

        public CategoryViewModelService(ICategoryRepository repository, IBffOrm orm) : base(repository)
        {
            _repository = repository;
            _orm = orm;

            new CategoryViewModel.CategoryViewModelInitializer(this).Initialize(All);
        }

        protected override ICategoryViewModel Create(ICategory model) 
            => new CategoryViewModel(model, _orm, this);
        public override ICategoryViewModel GetNewNonInsertedViewModel() 
            => new CategoryViewModel(_repository.Create(), _orm, this);
    }
}