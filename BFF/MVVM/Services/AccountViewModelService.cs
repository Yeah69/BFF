using System;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
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