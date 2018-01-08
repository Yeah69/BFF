using System;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.MVVM.Services
{
    public interface IPayeeViewModelService : ICommonPropertyViewModelServiceBase<IPayee, IPayeeViewModel>
    {
    }

    public class PayeeViewModelService : CommonPropertyViewModelServiceBase<IPayee, IPayeeViewModel>, IPayeeViewModelService
    {

        public PayeeViewModelService(IPayeeRepository repository, Func<IPayee, IPayeeViewModel> factory) : base(repository, factory, true)
        {
            All = new TransformingObservableReadOnlyList<IPayee, IPayeeViewModel>(
                new WrappingObservableReadOnlyList<IPayee>(repository.All),
                AddToDictionaries);
        }
    }
}
