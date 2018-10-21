using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Persistence;
using BFF.Model.Models.Structure;
using BFF.Persistence.Models;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.Model.Repositories
{
    public interface IRepositoryBase<TDomain> : IWriteOnlyRepositoryBase<TDomain>, IDbTableRepository<TDomain> where TDomain : class, IDataModel
    {
    }

    internal abstract class RepositoryBase<TDomain, TPersistence> : WriteOnlyRepositoryBase<TDomain, TPersistence>, IRepositoryBase<TDomain>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelDto
    {
        private readonly ICrudOrm _crudOrm;

        protected RepositoryBase(IProvideConnection provideConnection, ICrudOrm crudOrm) : base(provideConnection, crudOrm)
        {
            _crudOrm = crudOrm;
        }

        public virtual async Task<TDomain> FindAsync(long id)
        {
            return await ConvertToDomainAsync(await _crudOrm.ReadAsync<TPersistence>(id).ConfigureAwait(false)).ConfigureAwait(false);
        }

        protected abstract Task<TDomain> ConvertToDomainAsync(TPersistence persistenceModel);

        protected virtual Task<IEnumerable<TPersistence>> FindAllInnerAsync() => _crudOrm.ReadAllAsync<TPersistence>();

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