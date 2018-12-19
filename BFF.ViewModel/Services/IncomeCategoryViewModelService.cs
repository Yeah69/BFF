using System;
using BFF.Model.Models;
using BFF.Model.Repositories.ModelRepositories;
using BFF.ViewModel.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.ViewModel.Services
{
    public interface IIncomeCategoryViewModelService : ICommonPropertyViewModelServiceBase<IIncomeCategory, IIncomeCategoryViewModel>
    {
    }

    internal class IncomeCategoryViewModelService : CommonPropertyViewModelServiceBase<IIncomeCategory, IIncomeCategoryViewModel>, IIncomeCategoryViewModelService
    {
        public IncomeCategoryViewModelService(IIncomeCategoryRepository repository, Func<IIncomeCategory, IIncomeCategoryViewModel> factory) 
            : base(repository, factory, true)
        {
            All = new TransformingObservableReadOnlyList<IIncomeCategory, IIncomeCategoryViewModel>(
                new WrappingObservableReadOnlyList<IIncomeCategory>(repository.All),
                AddToDictionaries);
        }
    }
}
