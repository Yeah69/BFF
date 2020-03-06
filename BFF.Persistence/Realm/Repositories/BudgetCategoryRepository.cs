using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.Models.Domain;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Realm.Repositories
{
    internal class RealmBudgetCategoryRepository : IBudgetCategoryRepository
    {
        private readonly Lazy<IRealmBudgetEntryRepository> _budgetEntryRepository;
        private readonly Lazy<IBudgetOrm> _budgetOrm;

        public RealmBudgetCategoryRepository(
            Lazy<IRealmBudgetEntryRepository> budgetEntryRepository,
            Lazy<IBudgetOrm> budgetOrm)
        {
            _budgetEntryRepository = budgetEntryRepository;
            _budgetOrm = budgetOrm;
        }

        public void Dispose()
        {
        }

        public Task<IBudgetCategory> FindAsync(ICategory category)
        {
            return new Models.Domain.BudgetCategory(
                category,
                _budgetOrm.Value,
                _budgetEntryRepository.Value)
                .TaskFromResult<IBudgetCategory>();
        }
    }
}
