using System;
using BFF.Model.Models;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    public interface IBudgetEntryViewModelService : IModelToViewModelServiceBase<IBudgetEntry, IBudgetEntryViewModel>
    {
    }

    public class BudgetEntryViewModelService : ModelToViewModelServiceBase<IBudgetEntry, IBudgetEntryViewModel>, IBudgetEntryViewModelService
    {
        private readonly Func<IBudgetEntry, IBudgetEntryViewModel> _budgetEntryViewModelFactory;

        public BudgetEntryViewModelService(Func<IBudgetEntry, IBudgetEntryViewModel> budgetEntryViewModelFactory)
        {
            _budgetEntryViewModelFactory = budgetEntryViewModelFactory;
        }

        protected override IBudgetEntryViewModel Create(IBudgetEntry model)
        {
            return _budgetEntryViewModelFactory(model);
        }
    }
}
