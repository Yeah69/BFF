using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class BudgetCategory : Model.Models.BudgetCategory
    {
        private readonly IBudgetOrm _budgetOrm;
        private readonly IRealmBudgetEntryRepository _budgetEntryRepository;

        public BudgetCategory(
            ICategory category,
            IBudgetOrm budgetOrm,
            IRealmBudgetEntryRepository budgetEntryRepository) : base(category)
        {
            _budgetOrm = budgetOrm;
            _budgetEntryRepository = budgetEntryRepository;
        }

        public override async Task<IEnumerable<IBudgetEntry>> GetBudgetEntriesFor(int year)
        {
            var realmCategory = (Category as Category)?.RealmObject;
            return await (await _budgetOrm.FindAsync(year, realmCategory).ConfigureAwait(false))
                .Select(t => 
                    _budgetEntryRepository.Convert(
                        t.Entry, 
                        realmCategory, 
                        t.Data.Month,
                        t.Data.Budget, 
                        t.Data.Outflow,
                        t.Data.Balance,
                        t.Data.AggregatedBudget,
                        t.Data.AggregatedOutflow,
                        t.Data.AggregatedBalance))
                .ToAwaitableEnumerable()
                .ConfigureAwait(false);
        }
    }
}