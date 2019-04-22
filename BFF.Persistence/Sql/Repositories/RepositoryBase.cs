using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Model.Models.Structure;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories
{
    internal interface ISqliteRepositoryBase<TDomain> : ISqliteWriteOnlyRepositoryBase<TDomain>, ISqliteDbTableRepository<TDomain> 
        where TDomain : class, IDataModel
    {
    }

    internal abstract class SqliteRepositoryBase<TDomain, TPersistence> : SqliteWriteOnlyRepositoryBase<TDomain>, ISqliteRepositoryBase<TDomain>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelSql
    {
        private readonly ICrudOrm<TPersistence> _crudOrm;

        protected SqliteRepositoryBase(ICrudOrm<TPersistence> crudOrm)
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