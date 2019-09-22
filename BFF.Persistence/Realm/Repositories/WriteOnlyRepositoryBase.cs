using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.Model.Models.Structure;

namespace BFF.Persistence.Realm.Repositories
{
    internal interface IRealmWriteOnlyRepositoryBase<TDomain>
        : IRealmWriteOnlyRepository<TDomain>, IDisposable
        where TDomain : class, IDataModel
    {

    }

    internal abstract class RealmWriteOnlyRepositoryBase<TDomain>
        : IRealmWriteOnlyRepositoryBase<TDomain>
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
