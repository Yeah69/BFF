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
    public abstract class RepositoryBase<TDomain, TPersistance> : IDbTableRepository<TDomain>
        where TDomain : class, IDataModel
        where TPersistance : class, IPersistanceModel
    {
        private readonly IProvideConnection _provideConnection;

        public RepositoryBase(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        private void executeOnExistingOrNewConnection(Action<DbConnection> action, DbConnection connection = null)
        {
            if(connection != null) action(connection);
            else
            {
                using(TransactionScope transactionScope = new TransactionScope())
                using(DbConnection newConnection = _provideConnection.Connection)
                {
                    newConnection.Open();
                    action(newConnection);
                    transactionScope.Complete();
                }
            }
        }

        public virtual void CreateTable(DbConnection connection = null)
        {
            executeOnExistingOrNewConnection(
                c => c.Execute(CreateTableStatement), 
                connection);
        }

        public virtual void Add(TDomain dataModel, DbConnection connection = null)
        {
            executeOnExistingOrNewConnection(
                c =>
                {
                    TPersistance persistanceModel = ConvertToPersistance(dataModel);
                    c.Insert(persistanceModel);
                    dataModel.Id = persistanceModel.Id;
                }, 
                connection);
        }

        public virtual void Update(TDomain dataModel, DbConnection connection = null)
        {
            executeOnExistingOrNewConnection(
                c => c.Update(ConvertToPersistance(dataModel)), 
                connection);
        }

        public virtual void Delete(TDomain dataModel, DbConnection connection = null)
        {
            executeOnExistingOrNewConnection(
                c => c.Delete(ConvertToPersistance(dataModel)), 
                connection);
        }

        public virtual TDomain Find(long id, DbConnection connection = null)
        {
            if(connection != null) return ConvertToDomain(connection.Get<TPersistance>(id));
            
            TDomain ret;
            using(TransactionScope transactionScope = new TransactionScope())
            using(DbConnection newConnection = _provideConnection.Connection)
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
        protected abstract string CreateTableStatement { get; }
        public IEnumerable<TDomain> FindAll(DbConnection connection = null)
        {
            IEnumerable<TDomain> inner(DbConnection conn)
            {
                return conn.GetAll<TPersistance>().Select(p => ConvertToDomain(p));
            }

            if(connection != null) return inner(connection);
            
            IEnumerable<TDomain> ret;
            using(TransactionScope transactionScope = new TransactionScope())
            using(DbConnection newConnection = _provideConnection.Connection)
            {
                newConnection.Open();
                ret = inner(newConnection);
                transactionScope.Complete();
            }
            return ret;
        }
    }
}