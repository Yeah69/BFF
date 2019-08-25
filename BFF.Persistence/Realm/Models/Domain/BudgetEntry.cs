using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class BudgetEntry : Model.Models.BudgetEntry, IRealmModel<IBudgetEntryRealm>
    {
        private readonly ICrudOrm<IBudgetEntryRealm> _crudOrm;
        private readonly IRealmTransRepository _transRepository;
        private bool _isInserted;

        public BudgetEntry(
            ICrudOrm<IBudgetEntryRealm> crudOrm,
            IRealmTransRepository transRepository,
            IRxSchedulerProvider rxSchedulerProvider,
            IBudgetEntryRealm realmObject,
            bool isInserted,
            DateTime month,
            ICategory category, 
            long budget, 
            long outflow, 
            long balance) : base(rxSchedulerProvider, month, category, budget, outflow, balance)
        {
            _crudOrm = crudOrm;
            _transRepository = transRepository;
            RealmObject = realmObject;
            _isInserted = isInserted;
        }

        public IBudgetEntryRealm RealmObject { get; }

        public override bool IsInserted => _isInserted;

        public override async Task InsertAsync()
        {
            _isInserted = await _crudOrm.CreateAsync(RealmObject).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            await _crudOrm.DeleteAsync(RealmObject).ConfigureAwait(false);
            _isInserted = false;
        }

        protected override async Task UpdateAsync()
        {
            await _crudOrm.UpdateAsync(RealmObject).ConfigureAwait(false);
        }

        public override Task<IEnumerable<ITransBase>> GetAssociatedTransAsync()
        {
            return _transRepository.GetFromMonthAndCategoryAsync(Month, Category);
        }

        public override Task<IEnumerable<ITransBase>> GetAssociatedTransIncludingSubCategoriesAsync()
        {
            return _transRepository.GetFromMonthAndCategoriesAsync(Month, Category.IterateTreeBreadthFirst(c => c.Categories));
        }
    }
}
