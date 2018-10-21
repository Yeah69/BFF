using System;
using BFF.Model.Models;
using BFF.Model.Repositories.ModelRepositories;
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
