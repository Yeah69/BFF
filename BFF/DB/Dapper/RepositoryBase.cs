using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Transactions;
using BFF.DB.PersistanceModels;
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

    public abstract class RepositoryBase<TDomain, TPersistance> : IDbTableRepository<TDomain>
        where TDomain : class, IDataModel
        where TPersistance : class, IPersistanceModel
    {
        protected IProvideConnection ProvideConnection { get; }

        public RepositoryBase(IProvideConnection provideConnection)
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
                    TPersistance persistanceModel = ConvertToPersistance(dataModel);
                    c.Insert(persistanceModel);
                    dataModel.Id = persistanceModel.Id;
                }, 
                ProvideConnection,
                connection);
        }

        public virtual void Update(TDomain dataModel, DbConnection connection = null)
        {
            if(dataModel.Id < 0) return;
            ConnectionHelper.ExecuteOnExistingOrNewConnection(
                c => c.Update(ConvertToPersistance(dataModel)), 
                ProvideConnection,
                connection);
        }

        public virtual void Delete(TDomain dataModel, DbConnection connection = null)
        {
            if(dataModel.Id < 0) return;
            ConnectionHelper.ExecuteOnExistingOrNewConnection(
                c => c.Delete(ConvertToPersistance(dataModel)), 
                ProvideConnection,
                connection);
        }

        public virtual TDomain Find(long id, DbConnection connection = null)
        {
            if(connection != null) return ConvertToDomain(connection.Get<TPersistance>(id));
            
            TDomain ret;
            using(TransactionScope transactionScope = new TransactionScope())
            using(DbConnection newConnection = ProvideConnection.Connection)
            {
                newConnection.Open();
                var result = newConnection.Get<TPersistance>(id);
                ret = ConvertToDomain(result);
                transactionScope.Complete();
            }
            return ret;
        }

        protected abstract Converter<TDomain, TPersistance> ConvertToPersistance { get; }
        protected abstract Converter<TPersistance, TDomain> ConvertToDomain { get; }
        public IEnumerable<TDomain> FindAll(DbConnection connection = null)
        {
            IEnumerable<TDomain> inner(DbConnection conn)
            {
                return conn.GetAll<TPersistance>().Select(p => ConvertToDomain(p));
            }

            if(connection != null) return inner(connection);
            
            IEnumerable<TDomain> ret;
            using(TransactionScope transactionScope = new TransactionScope())
            using(DbConnection newConnection = ProvideConnection.Connection)
            {
                newConnection.Open();
                ret = inner(newConnection);
                transactionScope.Complete();
            }
            return ret;
        }
    }
}