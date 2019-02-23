using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Model.Models.Structure;
using BFF.Persistence.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories
{
    internal interface IRepositoryBase<TDomain, TPersistence> : IWriteOnlyRepositoryBase<TDomain>, IDbTableRepository<TDomain, TPersistence> 
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModel
    {
    }

    internal abstract class RepositoryBase<TDomain, TPersistence> : WriteOnlyRepositoryBase<TDomain, TPersistence>, IRepositoryBase<TDomain, TPersistence>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelSql
    {
        private readonly ICrudOrm<TPersistence> _crudOrm;

        protected RepositoryBase(ICrudOrm<TPersistence> crudOrm)
        {
            _crudOrm = crudOrm;
        }

        public virtual async Task<TDomain> FindAsync(long id)
        {
            return await ConvertToDomainAsync(await _crudOrm.ReadAsync(id).ConfigureAwait(false)).ConfigureAwait(false);
        }

        protected abstract Task<TDomain> ConvertToDomainAsync(TPersistence persistenceModel);

        protected virtual Task<IEnumerable<TPersistence>> FindAllInnerAsync() => _crudOrm.ReadAllAsync();

        public virtual async Task<IEnumerable<TDomain>> FindAllAsync()
        {
            return await 
                (await FindAllInnerAsync().ConfigureAwait(false))
                .Select(async p => await ConvertToDomainAsync(p).ConfigureAwait(false))
                .ToAwaitableEnumerable()
                .ConfigureAwait(false);
        }
    }
}