using System;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    public class SubIncomeViewModelService : ModelToViewModelServiceBase<ISubIncome, ISubIncomeViewModel>
    {
        private readonly Func<ISubIncome, ISubIncomeViewModel> _subIncomeViewModelFactory;

        public SubIncomeViewModelService(
            Func<ISubIncome, ISubIncomeViewModel> subIncomeViewModelFactory)
        {
            _subIncomeViewModelFactory = subIncomeViewModelFactory;
        }

        protected override ISubIncomeViewModel Create(ISubIncome model)
        {
            return _subIncomeViewModelFactory(model);
        }
    }
}
