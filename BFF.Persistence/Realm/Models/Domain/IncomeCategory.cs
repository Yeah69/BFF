using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class IncomeCategory : Model.Models.IncomeCategory, IRealmModel<Persistence.Category>
    {
        private readonly IUpdateBudgetCache _updateBudgetCache;
        private readonly IMergeOrm _mergeOrm;
        private readonly IRealmIncomeCategoryRepositoryInternal _repository;
        private readonly RealmObjectWrap<Persistence.Category> _realmObjectWrap;

        public IncomeCategory(
            ICrudOrm<Persistence.Category> crudOrm,
            IUpdateBudgetCache updateBudgetCache,
            IMergeOrm mergeOrm,
            IRealmIncomeCategoryRepositoryInternal repository,
            IRxSchedulerProvider rxSchedulerProvider,
            Persistence.Category realmObject,
            string name, 
            int monthOffset) : base(rxSchedulerProvider, name, monthOffset)
        {
            _realmObjectWrap = new RealmObjectWrap<Persistence.Category>(
                realmObject,
                realm =>
                {
                    var dbSetting = realm.All<Persistence.DbSetting>().First();
                    var id = dbSetting.NextCategoryId++;
                    realm.Add(dbSetting, true);
                    var ro = new Persistence.Category{ Id = id };
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            _updateBudgetCache = updateBudgetCache;
            _mergeOrm = mergeOrm;
            _repository = repository;
            
            void UpdateRealmObject(Persistence.Category ro)
            {
                ro.Parent = null;
                ro.IsIncomeRelevant = true;
                ro.IncomeMonthOffset = MonthOffset;
                ro.Name = Name;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public Persistence.Category RealmObject => _realmObjectWrap.RealmObject;

        public override async Task InsertAsync()
        {
            await _realmObjectWrap.InsertAsync().ConfigureAwait(false);
            await _repository.AddAsync(this).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            await _updateBudgetCache
                .OnCategoryDeletion(_realmObjectWrap.RealmObject)
                .ConfigureAwait(false);
            await _realmObjectWrap.DeleteAsync().ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);

        }

        protected override async Task UpdateAsync()
        {
            var beforeIncomeMonthOffset = _realmObjectWrap.RealmObject?.IncomeMonthOffset;
            var afterIncomeMonthOffset = MonthOffset;
            await _realmObjectWrap.UpdateAsync().ConfigureAwait(false);
            if (beforeIncomeMonthOffset != null)
            {
                await _updateBudgetCache
                    .OnIncomeCategoryChange(
                        _realmObjectWrap.RealmObject, 
                        beforeIncomeMonthOffset.Value, 
                        afterIncomeMonthOffset)
                    .ConfigureAwait(false);
            }
        }

        public override async Task MergeToAsync(IIncomeCategory category)
        {
            if (!(category is IncomeCategory)) throw new ArgumentException("Cannot merge if other part isn't from same backend", nameof(category));

            await _updateBudgetCache
                .OnCategoryDeletion(_realmObjectWrap.RealmObject)
                .ConfigureAwait(false);
            var incomeCategoryRealmObject = ((IncomeCategory)category).RealmObject;
            await _mergeOrm.MergeCategoryAsync(
                    RealmObject,
                    incomeCategoryRealmObject)
                .ConfigureAwait(false);
            await _updateBudgetCache
                .OnCategoryMergeForTarget(incomeCategoryRealmObject)
                .ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);
        }
    }
}
