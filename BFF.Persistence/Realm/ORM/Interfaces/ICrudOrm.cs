using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Models.Persistence;
using Realms;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    internal interface ICrudOrm<T> : IOncePerBackend
        where T : class, IPersistenceModelRealm
    {
        Task<bool> CreateAsync(Func<Realms.Realm, RealmObject> createNewRealmObject);

        Task<IEnumerable<T>> ReadAllAsync();

        Task UpdateAsync(T model, Action updateRealmObject);

        Task DeleteAsync(T model);
    }
}