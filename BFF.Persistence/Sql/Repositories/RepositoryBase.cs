using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Model.Models.Structure;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories
{
    internal interface IRepositoryBase<TDomain> : IWriteOnlyRepositoryBase<TDomain>, IDbTableRepository<TDomain> 
        where TDomain : class, IDataModel
    {
    }

    internal abstract class RepositoryBase<TDomain, TPersistence> : WriteOnlyRepositoryBase<TDomain>, IRepositoryBase<TDomain>
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