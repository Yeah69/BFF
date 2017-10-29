using System;
using System.Data.Common;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;
using Dapper.Contrib.Extensions;

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
        protected IProvideConnection ProvideConnection { get; }

        protected WriteOnlyRepositoryBase(IProvideConnection provideConnection)
        {
            ProvideConnection = provideConnection;
        }

        protected abstract Converter<TDomain, TPersistence> ConvertToPersistence { get; }

        public virtual void Add(TDomain dataModel, DbConnection connection = null)
        {
            if (dataModel.Id > 0) return;
            ConnectionHelper.ExecuteOnExistingOrNewConnection(
                c =>
                {
                    TPersistence persistenceModel = ConvertToPersistence(dataModel);
                    c.Insert(persistenceModel);
                    dataModel.Id = persistenceModel.Id;
                },
                ProvideConnection,
                connection);
        }

        public virtual void Update(TDomain dataModel, DbConnection connection = null)
        {
            if (dataModel.Id < 0) return;
            ConnectionHelper.ExecuteOnExistingOrNewConnection(
                c => c.Update(ConvertToPersistence(dataModel)),
                ProvideConnection,
                connection);
        }

        public virtual void Delete(TDomain dataModel, DbConnection connection = null)
        {
            if (dataModel.Id < 0) return;
            ConnectionHelper.ExecuteOnExistingOrNewConnection(
                c => c.Delete(ConvertToPersistence(dataModel)),
                ProvideConnection,
                connection);
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
