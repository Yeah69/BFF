using System;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.MVVM.Services
{
    public interface ICategoryBaseViewModelService : IConvertingViewModelServiceBase<ICategoryBase, ICategoryBaseViewModel>
    {
        IObservableReadOnlyList<ICategoryBaseViewModel> All { get; }
    }

    public class CategoryBaseViewModelService : ICategoryBaseViewModelService
    {
        private readonly ICategoryViewModelService _categoryViewModelService;
        private readonly IIncomeCategoryViewModelService _incomeCategoryViewModelService;

        public IObservableReadOnlyList<ICategoryBaseViewModel> All { get; }

        public CategoryBaseViewModelService(ICategoryViewModelService categoryViewModelService, IIncomeCategoryViewModelService incomeCategoryViewModelService)
        {
            _categoryViewModelService = categoryViewModelService;
            _incomeCategoryViewModelService = incomeCategoryViewModelService;

            All = new ConcatenatingObservableReadOnlyList<ICategoryBaseViewModel>(categoryViewModelService.All, incomeCategoryViewModelService.All);
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
