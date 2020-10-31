using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class IncomeCategory : Model.Models.IncomeCategory, IRealmModel<Persistence.Category>
    {
        private readonly IMergeOrm _mergeOrm;
        private readonly IRealmIncomeCategoryRepositoryInternal _repository;
        private readonly RealmObjectWrap<Persistence.Category> _realmObjectWrap;

        public IncomeCategory(
            // parameters
            Persistence.Category? realmObject,
            string name, 
            int monthOffset,
            
            // dependencies
            ICrudOrm<Persistence.Category> crudOrm,
            IMergeOrm mergeOrm,
            IRealmIncomeCategoryRepositoryInternal repository) : base(name, monthOffset)
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

        public Persistence.Category? RealmObject => _realmObjectWrap.RealmObject;

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

            var incomeCategoryRealmObject = ((IncomeCategory)category).RealmObject ?? throw new NullReferenceException("Shouldn't be null at that point");
            await _mergeOrm.MergeCategoryAsync(
                    RealmObject ?? throw new NullReferenceException("Shouldn't be null at that point"),
                    incomeCategoryRealmObject)
                .ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);
        }
    }
}
