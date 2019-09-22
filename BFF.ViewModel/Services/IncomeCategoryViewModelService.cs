using System;
using BFF.Core.Helper;
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
        public IncomeCategoryViewModelService(
            IIncomeCategoryRepository repository, 
            Func<IIncomeCategory, IIncomeCategoryViewModel> factory,
            IRxSchedulerProvider rxSchedulerProvider) 
            : base(repository, factory, true)
        {
            All = new TransformingObservableReadOnlyList<IIncomeCategory, IIncomeCategoryViewModel>(
                repository.All,
                AddToDictionaries,
                rxSchedulerProvider.UI);
            AllCollectionInitialized = repository.AllAsync;
        }
    }
}
