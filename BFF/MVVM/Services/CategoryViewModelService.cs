using System;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using MuVaViMo;

namespace BFF.MVVM.Services
{
    public interface ICategoryViewModelService : ICommonPropertyViewModelServiceBase<ICategory, ICategoryViewModel>
    {
    }

    public class CategoryViewModelService : CommonPropertyViewModelServiceBase<ICategory, ICategoryViewModel>, ICategoryViewModelService
    {
        public CategoryViewModelService(ICategoryRepository repository, Func<ICategory, ICategoryViewModel> factory) : base(repository, factory, true)
        {
            All = new TransformingObservableReadOnlyList<ICategory, ICategoryViewModel>(
                new WrappingObservableReadOnlyList<ICategory>(repository.All),
                AddToDictionaries);
        }
    }
}