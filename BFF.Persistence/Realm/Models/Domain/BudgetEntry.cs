using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class BudgetEntry : Model.Models.BudgetEntry, IRealmModel<Persistence.BudgetEntry>
    {
        private readonly IUpdateBudgetCache _updateBudgetCache;
        private readonly IRealmTransRepository _transRepository;
        private readonly RealmObjectWrap<Persistence.BudgetEntry> _realmObjectWrap;

        public BudgetEntry(
            ICrudOrm<Persistence.BudgetEntry> crudOrm,
            IUpdateBudgetCache updateBudgetCache,
            IRealmTransRepository transRepository,
            IRxSchedulerProvider rxSchedulerProvider,
            Persistence.BudgetEntry realmObject,
            DateTime month,
            ICategory category, 
            long budget, 
            long outflow, 
            long balance) : base(rxSchedulerProvider, month, category, budget, outflow, balance)
        {
            _realmObjectWrap = new RealmObjectWrap<Persistence.BudgetEntry>(
                realmObject,
                realm =>
                {
                    var dbSetting = realm.All<Persistence.DbSetting>().First();
                    var id = dbSetting.NextBudgetEntryId++;
                    realm.Add(dbSetting, true);
                    var ro = new Persistence.BudgetEntry{ Id = id };
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            _updateBudgetCache = updateBudgetCache;
            _transRepository = transRepository;
            
            void UpdateRealmObject(Persistence.BudgetEntry ro)
            {
                ro.Category = 
                    Category is null 
                        ? null 
                        : (Category as Category)?.RealmObject 
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Month = new DateTimeOffset(Month, TimeSpan.Zero);
                ro.Budget = Budget;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public Persistence.BudgetEntry RealmObject => _realmObjectWrap.RealmObject;

        public override async Task InsertAsync()
        {
            await _realmObjectWrap.InsertAsync().ConfigureAwait(false);
            if (_realmObjectWrap.RealmObject != null)
                await _updateBudgetCache.OnBudgetEntryChange(
                    _realmObjectWrap.RealmObject.Category,
                    _realmObjectWrap.RealmObject.Month)
                    .ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            var temp = _realmObjectWrap.RealmObject;
            await _realmObjectWrap.DeleteAsync().ConfigureAwait(false);
            if (temp != null)
                await _updateBudgetCache.OnBudgetEntryChange(
                        temp.Category,
                        temp.Month)
                    .ConfigureAwait(false);

        }

        protected override async Task UpdateAsync()
        {
            var tempBudget = _realmObjectWrap.RealmObject?.Budget;
            await _realmObjectWrap.UpdateAsync().ConfigureAwait(false);
            if (tempBudget != Budget && _realmObjectWrap.RealmObject != null)
                await _updateBudgetCache.OnBudgetEntryChange(
                        _realmObjectWrap.RealmObject.Category,
                        _realmObjectWrap.RealmObject.Month)
                    .ConfigureAwait(false);
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
