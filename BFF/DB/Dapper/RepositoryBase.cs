using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Transactions;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.DB.Dapper
{
    public abstract class CreateTableBase : ICreateTable
    {
        private readonly IProvideConnection _provideConnection;
        
        protected CreateTableBase(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public virtual void CreateTable(DbConnection connection = null)
        {
            ConnectionHelper.ExecuteOnExistingOrNewConnection(
                c => c.Execute(CreateTableStatement), 
                _provideConnection,
                connection);
        }
        
        protected abstract string CreateTableStatement { get; }
    }

    public interface IRepositoryBase<TDomain> : IDbTableRepository<TDomain> where TDomain : class, IDataModel
    {
    }

    public abstract class RepositoryBase<TDomain, TPersistence> : IRepositoryBase<TDomain>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModel
    {
        protected IProvideConnection ProvideConnection { get; }

        protected RepositoryBase(IProvideConnection provideConnection)
        {
            ProvideConnection = provideConnection;
        }

        public abstract TDomain Create();

        public virtual void Add(TDomain dataModel, DbConnection connection = null)
        {
            if(dataModel.Id > 0) return;
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
            if(dataModel.Id < 0) return;
            ConnectionHelper.ExecuteOnExistingOrNewConnection(
                c => c.Update(ConvertToPersistence(dataModel)), 
                ProvideConnection,
                connection);
        }

        public virtual void Delete(TDomain dataModel, DbConnection connection = null)
        {
            if(dataModel.Id < 0) return;
            ConnectionHelper.ExecuteOnExistingOrNewConnection(
                c => c.Delete(ConvertToPersistence(dataModel)), 
                ProvideConnection,
                connection);
        }

        public virtual TDomain Find(long id, DbConnection connection = null)
        {
            if(connection != null) return ConvertToDomain((connection.Get<TPersistence>(id), connection));
            
            TDomain ret;
            using(TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
            using(DbConnection newConnection = ProvideConnection.Connection)
            {
                newConnection.Open();
                var result = newConnection.Get<TPersistence>(id);
                ret = ConvertToDomain((result, newConnection));
                transactionScope.Complete();
            }
            return ret;
        }

        protected abstract Converter<TDomain, TPersistence> ConvertToPersistence { get; }
        protected abstract Converter<(TPersistence, DbConnection), TDomain> ConvertToDomain { get; }
        public virtual IEnumerable<TDomain> FindAll(DbConnection connection = null)
        {
            IEnumerable<TDomain> Inner(DbConnection conn)
            {
                return conn.GetAll<TPersistence>().Select(p => ConvertToDomain((p, conn)));
            }

            if(connection != null) return Inner(connection);
            
            IEnumerable<TDomain> ret;
            using(TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
            using(DbConnection newConnection = ProvideConnection.Connection)
            {
                newConnection.Open();
                ret = Inner(newConnection).ToList();
                transactionScope.Complete();
            }
            return ret;
        }
    }
}