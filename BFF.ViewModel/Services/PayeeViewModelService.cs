using System;
using BFF.Model.Models;
using BFF.Model.Repositories.ModelRepositories;
using BFF.ViewModel.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.ViewModel.Services
{
    public interface IPayeeViewModelService : ICommonPropertyViewModelServiceBase<IPayee, IPayeeViewModel>
    {
    }

    internal class PayeeViewModelService : CommonPropertyViewModelServiceBase<IPayee, IPayeeViewModel>, IPayeeViewModelService
    {

        public PayeeViewModelService(IPayeeRepository repository, Func<IPayee, IPayeeViewModel> factory) : base(repository, factory, true)
        {
            All = new TransformingObservableReadOnlyList<IPayee, IPayeeViewModel>(
                new WrappingObservableReadOnlyList<IPayee>(repository.All),
                AddToDictionaries);
        }
    }
}
