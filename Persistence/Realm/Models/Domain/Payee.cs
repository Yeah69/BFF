using System;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Payee : Model.Models.Payee, IRealmModel<Persistence.Payee>
    {
        private readonly IMergeOrm _mergeOrm;
        private readonly IRealmPayeeRepositoryInternal _repository;
        private readonly RealmObjectWrap<Persistence.Payee> _realmObjectWrap;

        public Payee(
            // parameters
            Persistence.Payee? realmObject,
            string name,
            
            // dependencies
            ICrudOrm<Persistence.Payee> crudOrm,
            IMergeOrm mergeOrm,
            IRealmPayeeRepositoryInternal repository) : base(name)
        {
            _realmObjectWrap = new RealmObjectWrap<Persistence.Payee>(
                realmObject,
                realm =>
                {
                    var ro = new Persistence.Payee();
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            
            _mergeOrm = mergeOrm;
            _repository = repository;
            
            void UpdateRealmObject(Persistence.Payee ro)
            {
                ro.Name = Name;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public Persistence.Payee? RealmObject => _realmObjectWrap.RealmObject;

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

        public override async Task MergeToAsync(IPayee payee)
        {
            if (!(payee is Payee)) throw new ArgumentException("Cannot merge if other part isn't from same backend", nameof(payee));

            await _mergeOrm.MergePayeeAsync(
                    RealmObject ?? throw new NullReferenceException("Shouldn't be null at that point"),
                    ((Payee)payee).RealmObject ?? throw new NullReferenceException("Shouldn't be null at that point"))
                .ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);
        }
    }
}
