using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class IncomeCategory : Model.Models.IncomeCategory, IRealmModel<ICategoryRealm>
    {
        private readonly IMergeOrm _mergeOrm;
        private readonly IRealmIncomeCategoryRepositoryInternal _repository;
        private readonly RealmObjectWrap<ICategoryRealm> _realmObjectWrap;

        public IncomeCategory(
            ICrudOrm<ICategoryRealm> crudOrm,
            IMergeOrm mergeOrm,
            IRealmIncomeCategoryRepositoryInternal repository,
            IRxSchedulerProvider rxSchedulerProvider,
            ICategoryRealm realmObject,
            string name, 
            int monthOffset) : base(rxSchedulerProvider, name, monthOffset)
        {
            _realmObjectWrap = new RealmObjectWrap<ICategoryRealm>(
                realmObject,
                realm =>
                {
                    var id = realm.All<Persistence.Category>().Count();
                    var ro = new Persistence.Category{ Id = id };
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            _mergeOrm = mergeOrm;
            _repository = repository;
            
            void UpdateRealmObject(ICategoryRealm ro)
            {
                ro.Parent = null;
                ro.IsIncomeRelevant = true;
                ro.MonthOffset = MonthOffset;
                ro.Name = Name;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public ICategoryRealm RealmObject => _realmObjectWrap.RealmObject;

        public override async Task InsertAsync()
        {
            await _realmObjectWrap.InsertAsync().ConfigureAwait(false);
            await _repository.AddAsync(this).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            await _realmObjectWrap.DeleteAsync().ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);
        }

        protected override Task UpdateAsync()
        {
            return _realmObjectWrap.UpdateAsync();
        }

        public override async Task MergeToAsync(IIncomeCategory category)
        {
            if (!(category is IncomeCategory)) throw new ArgumentException("Cannot merge if other part isn't from same backend", nameof(category));

            await _mergeOrm.MergeCategoryAsync(
                    RealmObject,
                    ((IncomeCategory)category).RealmObject)
                .ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);
        }
    }
}
