using System;
using System.Threading.Tasks;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using JetBrains.Annotations;
using MrMeeseeks.Extensions;
using Realms;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class RealmObjectWrap<T> 
        where T : class, IPersistenceModelRealm
    {
        [NotNull] private readonly Func<Realms.Realm, T> _createRealmObject;
        [NotNull] private readonly Action<T> _updateRealmObject;
        private readonly ICrudOrm<T> _crudOrm;

        public RealmObjectWrap(
            // parameter
            [CanBeNull] T realmObject,
            [NotNull] Func<Realms.Realm, T> createRealmObject,
            [NotNull] Action<T> updateRealmObject, 
            
            // dependencies
            [NotNull] ICrudOrm<T> crudOrm)
        {
            RealmObject = realmObject;
            _createRealmObject = createRealmObject ?? throw new ArgumentNullException(nameof(createRealmObject));
            _updateRealmObject = updateRealmObject ?? throw new ArgumentNullException(nameof(updateRealmObject));
            _crudOrm = crudOrm ?? throw  new ArgumentNullException(nameof(crudOrm));
        }
        
        public bool IsInserted => RealmObject != null;
        
        [CanBeNull]
        public T RealmObject { get; private set; }
        
        public async Task InsertAsync()
        {
            var result = await _crudOrm.CreateAsync(CreateRealmObject).ConfigureAwait(false);
            if (result.Not())
                RealmObject = null;

            RealmObject CreateRealmObject(Realms.Realm realm)
            {
                RealmObject = _createRealmObject(realm);
                return RealmObject as RealmObject;
            }
        }

        public async Task UpdateAsync()
        {
            if (IsInserted.Not()) return;

            await _crudOrm.UpdateAsync(RealmObject, UpdateRealmObject).ConfigureAwait(false);
            
            void UpdateRealmObject() => _updateRealmObject(RealmObject);
        }

        public async Task DeleteAsync()
        {
            await _crudOrm.DeleteAsync(RealmObject).ConfigureAwait(false);
            RealmObject = null;
        }
    }
}