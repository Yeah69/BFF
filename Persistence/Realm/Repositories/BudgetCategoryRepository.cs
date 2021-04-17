using System;
using System.Threading.Tasks;
using BFF.Model;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Realm.Repositories
{
    internal class RealmBudgetCategoryRepository : IBudgetCategoryRepository
    {
        private readonly Lazy<IRealmBudgetEntryRepository> _budgetEntryRepository;
        private readonly Lazy<IBudgetOrm> _budgetOrm;
        private readonly IObserveUpdateBudgetCategory _observeUpdateBudgetCategory;

        public RealmBudgetCategoryRepository(
            Lazy<IRealmBudgetEntryRepository> budgetEntryRepository,
            Lazy<IBudgetOrm> budgetOrm,
            IObserveUpdateBudgetCategory observeUpdateBudgetCategory)
        {
            _budgetEntryRepository = budgetEntryRepository;
            _budgetOrm = budgetOrm;
            _observeUpdateBudgetCategory = observeUpdateBudgetCategory;
        }

        public void Dispose()
        {
        }

        public Task<IBudgetCategory> FindAsync(ICategory category)
        {
            return new Models.Domain.BudgetCategory(
                category,
                _budgetOrm.Value,
                _budgetEntryRepository.Value,
                _observeUpdateBudgetCategory)
                .TaskFromResult<IBudgetCategory>();
        }
    }
}
