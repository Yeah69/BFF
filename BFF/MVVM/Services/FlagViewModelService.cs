using System;
using BFF.Model.Models;
using BFF.Model.Repositories.ModelRepositories;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.MVVM.Services
{
    public interface IFlagViewModelService : ICommonPropertyViewModelServiceBase<IFlag, IFlagViewModel>
    {
    }

    public class FlagViewModelService : CommonPropertyViewModelServiceBase<IFlag, IFlagViewModel>, IFlagViewModelService
    {
        public FlagViewModelService(IFlagRepository repository, Func<IFlag, IFlagViewModel> factory) : base(repository, factory, true)
        {
            All = new TransformingObservableReadOnlyList<IFlag, IFlagViewModel>(
                new WrappingObservableReadOnlyList<IFlag>(repository.All),
                AddToDictionaries);
        }
    }
}
