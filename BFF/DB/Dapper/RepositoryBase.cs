using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native.Structure;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.DB.Dapper
{
    public interface IRepositoryBase<TDomain> : IWriteOnlyRepositoryBase<TDomain>, IDbTableRepository<TDomain> where TDomain : class, IDataModel
    {
    }

    public abstract class RepositoryBase<TDomain, TPersistence> : WriteOnlyRepositoryBase<TDomain, TPersistence>, IRepositoryBase<TDomain>
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