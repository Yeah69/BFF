using System;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.ViewModel.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.ViewModel.Services
{
    public interface IAccountViewModelService : ICommonPropertyViewModelServiceBase<IAccount, IAccountViewModel>
    {
    }

    internal class AccountViewModelService : CommonPropertyViewModelServiceBase<IAccount, IAccountViewModel>, IAccountViewModelService
    {
        public AccountViewModelService(
            IAccountRepository repository,
            Func<IAccount, IAccountViewModel> factory) : base(repository, factory, true)
        {
            All = new TransformingObservableReadOnlyList<IAccount, IAccountViewModel>(
                repository.All,
                AddToDictionaries);
            AllCollectionInitialized = repository.AllAsync;
        }
    }
}