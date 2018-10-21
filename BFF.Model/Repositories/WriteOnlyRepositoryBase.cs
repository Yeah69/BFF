using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.Core.Persistence;
using BFF.Model.Models.Structure;
using BFF.Persistence.Models;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.Model.Repositories
{
    public interface IWriteOnlyRepositoryBase<in T> 
        : IWriteOnlyRepository<T>, IDisposable
        where T : class, IDataModel
    {

    }

    internal abstract class WriteOnlyRepositoryBase<TDomain, TPersistence>
        : IWriteOnlyRepositoryBase<TDomain>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelDto
    {
        private readonly ICrudOrm _crudOrm;
        protected IProvideConnection ProvideConnection { get; }

        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        protected WriteOnlyRepositoryBase(IProvideConnection provideConnection, ICrudOrm crudOrm)
        {
            _crudOrm = crudOrm;
            ProvideConnection = provideConnection;
        }

        protected abstract Converter<TDomain, TPersistence> ConvertToPersistence { get; }

        public virtual async Task AddAsync(TDomain dataModel)
        {
            if (dataModel.Id > 0) return;
            var persistenceModel = ConvertToPersistence(dataModel);
            await _crudOrm.CreateAsync(persistenceModel).ConfigureAwait(false);
            dataModel.Id = persistenceModel.Id;
        }

        public virtual async Task UpdateAsync(TDomain dataModel)
        {
            if (dataModel.Id < 0) return;
            await _crudOrm.UpdateAsync(ConvertToPersistence(dataModel)).ConfigureAwait(false);
        }

        public virtual async Task DeleteAsync(TDomain dataModel)
        {
            if (dataModel.Id < 0) return;
            await _crudOrm.DeleteAsync(ConvertToPersistence(dataModel)).ConfigureAwait(false);
        }

        public void Dispose()
        {
            CompositeDisposable.Dispose();
        }
    }
}
