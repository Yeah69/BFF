using System;
using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    public interface ICategoryViewModelService : ICommonPropertyViewModelServiceBase<ICategory, ICategoryViewModel>
    {
    }

    public class CategoryViewModelService : CommonPropertyViewModelServiceBase<ICategory, ICategoryViewModel>, ICategoryViewModelService
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
    public interface IIncomeCategoryViewModelService : ICommonPropertyViewModelServiceBase<IIncomeCategory, IIncomeCategoryViewModel>
    {
    }

    public class IncomeCategoryViewModelService : CommonPropertyViewModelServiceBase<IIncomeCategory, IIncomeCategoryViewModel>, IIncomeCategoryViewModelService
    {
        private readonly IIncomeCategoryRepository _repository;
        private readonly IBffOrm _orm;

        public IncomeCategoryViewModelService(IIncomeCategoryRepository repository, IBffOrm orm) : base(repository)
        {
            _repository = repository;
            _orm = orm;
        }

        protected override IIncomeCategoryViewModel Create(IIncomeCategory model)
            => new IncomeCategoryViewModel(model, _orm);
        public override IIncomeCategoryViewModel GetNewNonInsertedViewModel()
            => new IncomeCategoryViewModel(_repository.Create(), _orm);
    }
    
    public interface ICategoryBaseViewModelService : IConvertingViewModelServiceBase<ICategoryBase, ICategoryBaseViewModel>
    {
    }

    public class CategoryBaseViewModelService : ICategoryBaseViewModelService
    {
        private readonly ICategoryViewModelService _categoryViewModelService;
        private readonly IIncomeCategoryViewModelService _incomeCategoryViewModelService;

        public CategoryBaseViewModelService(ICategoryViewModelService categoryViewModelService, IIncomeCategoryViewModelService incomeCategoryViewModelService)
        {
            _categoryViewModelService = categoryViewModelService;
            _incomeCategoryViewModelService = incomeCategoryViewModelService;
        }

        public ICategoryBase GetModel(ICategoryBaseViewModel viewModel)
        {
            switch (viewModel)
            {
                case ICategoryViewModel categoryViewModel:
                    return _categoryViewModelService.GetModel(categoryViewModel);
                case IIncomeCategoryViewModel incomeCategoryViewModel:
                    return _incomeCategoryViewModelService.GetModel(incomeCategoryViewModel);
                case null:
                    return null;
                default:
                    throw new ArgumentException("Such a category type is not considered yet!"); // TODO Localize!
            }
        }

        public ICategoryBaseViewModel GetViewModel(ICategoryBase model)
        {
            switch (model)
            {
                case ICategory category:
                    return _categoryViewModelService.GetViewModel(category);
                case IIncomeCategory incomeCategory:
                    return _incomeCategoryViewModelService.GetViewModel(incomeCategory);
                case null:
                    return null;
                default:
                    throw new ArgumentException("Such a category type is not considered yet!"); // TODO Localize!
            }
        }
    }
}