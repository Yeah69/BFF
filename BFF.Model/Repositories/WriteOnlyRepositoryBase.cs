using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.Model.Models.Structure;
using BFF.Persistence.Models.Sql;

namespace BFF.Model.Repositories
{
    internal interface IWriteOnlyRepositoryBase<TPersistence> 
        : IWriteOnlyRepository<TPersistence>, IDisposable
        where TPersistence : class, IDataModel
    {

    }

    internal abstract class WriteOnlyRepositoryBase<TDomain, TPersistence>
        : IWriteOnlyRepositoryBase<TDomain>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelSql
    {

        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        public virtual async Task<bool> AddAsync(TDomain dataModel)
        {
            var result = await ((dataModel as IDataModelInternal<TPersistence>).BackingPersistenceModel?.InsertAsync()).ConfigureAwait(false);
            return result;
        }

        public virtual async Task UpdateAsync(TDomain dataModel)
        {
            await ((dataModel as IDataModelInternal<TPersistence>).BackingPersistenceModel?.UpdateAsync()).ConfigureAwait(false);
        }

        public virtual async Task DeleteAsync(TDomain dataModel)
        {
            await ((dataModel as IDataModelInternal<TPersistence>).BackingPersistenceModel?.DeleteAsync()).ConfigureAwait(false);
        }

        public void Dispose()
        {
            CompositeDisposable.Dispose();
        }
    }
}
