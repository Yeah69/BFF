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

    public interface IRepositoryBase<TDomain> : IWriteOnlyRepositoryBase<TDomain>, IDbTableRepository<TDomain> where TDomain : class, IDataModel
    {
    }

    public abstract class RepositoryBase<TDomain, TPersistence> : WriteOnlyRepositoryBase<TDomain, TPersistence>, IRepositoryBase<TDomain>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModel
    {
        protected RepositoryBase(IProvideConnection provideConnection) : base(provideConnection)
        {
        }

        public abstract TDomain Create();

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

        protected abstract Converter<(TPersistence, DbConnection), TDomain> ConvertToDomain { get; }

        protected virtual IEnumerable<TPersistence> FindAllInner(DbConnection connection) => connection.GetAll<TPersistence>();

        public virtual IEnumerable<TDomain> FindAll(DbConnection connection = null)
        {
            IEnumerable<TDomain> Inner(DbConnection conn)
            {
                return FindAllInner(conn).Select(p => ConvertToDomain((p, conn)));
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