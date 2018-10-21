using System;
using BFF.Model.Models;
using BFF.Model.Repositories.ModelRepositories;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.MVVM.Services
{
    public interface IAccountViewModelService : ICommonPropertyViewModelServiceBase<IAccount, IAccountViewModel>
    {
    }

    public class AccountViewModelService : CommonPropertyViewModelServiceBase<IAccount, IAccountViewModel>, IAccountViewModelService
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