using System;
using BFF.Model.Models;
using BFF.Model.Repositories.ModelRepositories;
using BFF.ViewModel.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.ViewModel.Services
{
    public interface IFlagViewModelService : ICommonPropertyViewModelServiceBase<IFlag, IFlagViewModel>
    {
    }

    internal class FlagViewModelService : CommonPropertyViewModelServiceBase<IFlag, IFlagViewModel>, IFlagViewModelService
    {
        public FlagViewModelService(IFlagRepository repository, Func<IFlag, IFlagViewModel> factory) : base(repository, factory, true)
        {
            All = new TransformingObservableReadOnlyList<IFlag, IFlagViewModel>(
                new WrappingObservableReadOnlyList<IFlag>(repository.All),
                AddToDictionaries);
        }
    }
}
