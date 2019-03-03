using System;
using BFF.Model.Models;
using BFF.Model.Repositories;
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
                repository.All,
                AddToDictionaries);
            AllCollectionInitialized = repository.AllAsync;
        }
    }
}
