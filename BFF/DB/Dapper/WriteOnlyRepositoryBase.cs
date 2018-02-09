using System;
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

        public virtual void Add(TDomain dataModel)
        {
            if (dataModel.Id > 0) return;
            var persistenceModel = ConvertToPersistence(dataModel);
            _crudOrm.Create(persistenceModel);
            dataModel.Id = persistenceModel.Id;
        }

        public virtual void Update(TDomain dataModel)
        {
            if (dataModel.Id < 0) return;
            _crudOrm.Update(ConvertToPersistence(dataModel));
        }

        public virtual void Delete(TDomain dataModel)
        {
            if (dataModel.Id < 0) return;
            _crudOrm.Delete(ConvertToPersistence(dataModel));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
