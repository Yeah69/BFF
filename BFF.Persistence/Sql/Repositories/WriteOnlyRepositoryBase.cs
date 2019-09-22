using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.Model.Models.Structure;

namespace BFF.Persistence.Sql.Repositories
{
    internal interface ISqliteWriteOnlyRepositoryBase<TDomain> 
        : ISqliteWriteOnlyRepository<TDomain>, IDisposable
        where TDomain : class, IDataModel
    {

    }

    internal abstract class SqliteWriteOnlyRepositoryBase<TDomain>
        : ISqliteWriteOnlyRepositoryBase<TDomain>
        where TDomain : class, IDataModel
    {

        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        public virtual Task<bool> AddAsync(TDomain dataModel)
        {
            return Task.FromResult(true);
        }

        public void Dispose()
        {
            CompositeDisposable.Dispose();
        }
    }
}
