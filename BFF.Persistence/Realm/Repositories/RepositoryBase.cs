using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Models.Persistence;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Realm.Repositories
{
    internal interface IRealmRepositoryBase<TDomain, TPersistence> : IRealmWriteOnlyRepositoryBase<TDomain>, IRealmRepository<TDomain, TPersistence>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelRealm
    {
    }

    internal abstract class RealmRepositoryBase<TDomain, TPersistence> : RealmWriteOnlyRepositoryBase<TDomain>, IRealmRepositoryBase<TDomain, TPersistence>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelRealm
    {
        private readonly ICrudOrm<TPersistence> _crudOrm;

        protected RealmRepositoryBase(ICrudOrm<TPersistence> crudOrm)
        {
            _crudOrm = crudOrm;
        }

        protected abstract Task<TDomain> ConvertToDomainAsync(TPersistence persistenceModel);

        protected virtual Task<IEnumerable<TPersistence>> FindAllInnerAsync() => _crudOrm.ReadAllAsync();

        protected virtual async Task<IEnumerable<TDomain>> FindAllAsync()
        {
            return await
                (await FindAllInnerAsync().ConfigureAwait(false))
                .Select(async p => await ConvertToDomainAsync(p).ConfigureAwait(false))
                .ToAwaitableEnumerable()
                .ConfigureAwait(false);
        }

        public virtual Task<TDomain> FindAsync(TPersistence realmObject) => ConvertToDomainAsync(realmObject);
    }
}