using System;
using BFF.Model.Models;
using BFF.Model.Repositories.ModelRepositories;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.MVVM.Services
{
    public interface IIncomeCategoryViewModelService : ICommonPropertyViewModelServiceBase<IIncomeCategory, IIncomeCategoryViewModel>
    {
    }

    public class IncomeCategoryViewModelService : CommonPropertyViewModelServiceBase<IIncomeCategory, IIncomeCategoryViewModel>, IIncomeCategoryViewModelService
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
