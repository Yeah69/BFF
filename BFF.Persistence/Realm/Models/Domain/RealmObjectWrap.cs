using System;
using System.Threading.Tasks;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using MrMeeseeks.Extensions;
using Realms;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class RealmObjectWrap<T> 
        where T : class, IPersistenceModelRealm
    {
        private readonly Func<Realms.Realm, T> _createRealmObject;
        private readonly Action<T> _updateRealmObject;
        private readonly ICrudOrm<T> _crudOrm;

        public RealmObjectWrap(
            // parameters
            T? realmObject,
            Func<Realms.Realm, T> createRealmObject,
            Action<T> updateRealmObject, 
            
            // dependencies
            ICrudOrm<T> crudOrm)
        {
            RealmObject = realmObject;
            _createRealmObject = createRealmObject ?? throw new ArgumentNullException(nameof(createRealmObject));
            _updateRealmObject = updateRealmObject ?? throw new ArgumentNullException(nameof(updateRealmObject));
            _crudOrm = crudOrm ?? throw new ArgumentNullException(nameof(crudOrm));
        }
        
        public bool IsInserted => RealmObject != null;
        
        public T? RealmObject { get; private set; }
        
        public async Task InsertAsync()
        {
            var result = await _crudOrm.CreateAsync(CreateRealmObject).ConfigureAwait(false);
            if (result.Not())
                RealmObject = null;

            RealmObject CreateRealmObject(Realms.Realm realm)
            {
                RealmObject = _createRealmObject(realm);
                return RealmObject as RealmObject ?? throw new NullReferenceException("Shouldn't be null at that point");
            }
        }

        public Task UpdateAsync()
        {
            return RealmObject is null
                ? Task.CompletedTask 
                : _crudOrm.UpdateAsync(RealmObject, UpdateRealmObject);

            void UpdateRealmObject() => _updateRealmObject(RealmObject);
        }

        public async Task DeleteAsync()
        {
            await _crudOrm.DeleteAsync(RealmObject ?? throw new NullReferenceException("Shouldn't be null at that point")).ConfigureAwait(false);
            RealmObject = null;
        }
    }
}