using System;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    public interface IParentIncomeViewModelService : IModelToViewModelServiceBase<IParentIncome, IParentIncomeViewModel>
    {
    }

    public class ParentIncomeViewModelService : ModelToViewModelServiceBase<IParentIncome, IParentIncomeViewModel>, IParentIncomeViewModelService
    {
        private readonly Func<IParentIncome, IParentIncomeViewModel> _parentIncomeViewModelFactory;

        public ParentIncomeViewModelService(Func<IParentIncome, IParentIncomeViewModel> parentIncomeViewModelFactory)
        {
            _parentIncomeViewModelFactory = parentIncomeViewModelFactory;
        }

        protected override IParentIncomeViewModel Create(IParentIncome model)
        {
            return _parentIncomeViewModelFactory(model);
        }
    }
}
