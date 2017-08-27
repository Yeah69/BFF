using System;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    public class ParentIncomeViewModelService : ModelToViewModelServiceBase<IParentIncome, IParentIncomeViewModel>
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
