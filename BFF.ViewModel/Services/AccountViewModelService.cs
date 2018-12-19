using System;
using BFF.Model.Models;
using BFF.Model.Repositories.ModelRepositories;
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
                new WrappingObservableReadOnlyList<IAccount>(repository.All),
                AddToDictionaries);
        }
    }
}