using System;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    public interface ISubIncomeViewModelService : IModelToViewModelServiceBase<ISubIncome, ISubIncomeViewModel>
    {
        ISubIncomeViewModel Create(IParentIncome parent);
    }

    public class SubIncomeViewModelService : ModelToViewModelServiceBase<ISubIncome, ISubIncomeViewModel>, ISubIncomeViewModelService
    {
        private readonly Func<ISubIncome, ISubIncomeViewModel> _subIncomeViewModelFactory;
        private readonly Func<ISubIncome> _subIncomeFactory;

        public SubIncomeViewModelService(
            Func<ISubIncome, ISubIncomeViewModel> subIncomeViewModelFactory,
        Func<ISubIncome> subIncomeFactory)
        {
            _subIncomeViewModelFactory = subIncomeViewModelFactory;
            _subIncomeFactory = subIncomeFactory;
        }

        protected override ISubIncomeViewModel Create(ISubIncome model)
        {
            return _subIncomeViewModelFactory(model);
        }

        public ISubIncomeViewModel Create(IParentIncome parent)
        {
            var subTransaction = _subIncomeFactory();
            subTransaction.Parent = parent;

            return _subIncomeViewModelFactory(subTransaction);
        }
    }
}
