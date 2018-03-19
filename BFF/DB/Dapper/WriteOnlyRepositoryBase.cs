using System;
using System.Threading.Tasks;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB.Dapper
{
    public interface IWriteOnlyRepositoryBase<in T> 
        : IWriteOnlyRepository<T>, IDisposable
        where T : class, IDataModel
    {

    }

    public abstract class WriteOnlyRepositoryBase<TDomain, TPersistence>
        : IWriteOnlyRepositoryBase<TDomain>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModel
    {
        private readonly ICrudOrm _crudOrm;
        protected IProvideConnection ProvideConnection { get; }

        protected WriteOnlyRepositoryBase(IProvideConnection provideConnection, ICrudOrm crudOrm)
        {
            _crudOrm = crudOrm;
            ProvideConnection = provideConnection;
        }

        protected abstract Converter<TDomain, TPersistence> ConvertToPersistence { get; }

        public virtual async Task Add(TDomain dataModel)
        {
            if (dataModel.Id > 0) return;
            var persistenceModel = ConvertToPersistence(dataModel);
            await _crudOrm.CreateAsync(persistenceModel).ConfigureAwait(false);
            dataModel.Id = persistenceModel.Id;
        }

        public virtual async Task Update(TDomain dataModel)
        {
            if (dataModel.Id < 0) return;
            await _crudOrm.UpdateAsync(ConvertToPersistence(dataModel)).ConfigureAwait(false);
        }

        public virtual async Task Delete(TDomain dataModel)
        {
            if (dataModel.Id < 0) return;
            await _crudOrm.DeleteAsync(ConvertToPersistence(dataModel)).ConfigureAwait(false);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
