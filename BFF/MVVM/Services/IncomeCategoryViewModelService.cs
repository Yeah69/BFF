using System;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
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
