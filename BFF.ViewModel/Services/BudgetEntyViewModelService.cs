using System;
using BFF.Model.Models;
using BFF.ViewModel.ViewModels.ForModels;

namespace BFF.ViewModel.Services
{
    public interface IBudgetEntryViewModelService : IModelToViewModelServiceBase<IBudgetEntry, IBudgetEntryViewModel>
    {
    }

    internal class BudgetEntryViewModelService : ModelToViewModelServiceBase<IBudgetEntry, IBudgetEntryViewModel>, IBudgetEntryViewModelService
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
