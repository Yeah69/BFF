using System;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
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
