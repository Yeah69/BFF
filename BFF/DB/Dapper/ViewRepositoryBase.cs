using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Transactions;
using BFF.DB.PersistanceModels;
using BFF.MVVM.Models.Native.Structure;
using Dapper;

namespace BFF.DB.Dapper
{
    public abstract class ViewRepositoryBase<TDomainBase, TPersistance, TSpecifying>
        : IViewRepository<TDomainBase, TSpecifying>
        where TDomainBase : class, IDataModel
        where TPersistance : class, IPersistanceModel
    {
        private IProvideConnection _provideConnection;
        
        
        
        protected ViewRepositoryBase(IProvideConnection provideConnection)
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

        private T executeOnExistingOrNewConnectionAndReturn<T>(Func<DbConnection, T> action,
                                                               DbConnection connection = null)
        {
            if(connection != null) return action(connection);

            T ret;
            using(TransactionScope transactionScope = new TransactionScope())
            using(DbConnection newConnection = _provideConnection.Connection)
            {
                newConnection.Open();
                ret = action(newConnection);
                transactionScope.Complete();
            }
            return ret;
        }
        
        public virtual IEnumerable<TDomainBase> GetPage(int offset, int pageSize, TSpecifying specifyingObject, 
                                                        DbConnection connection = null)
        {
            string query = $"SELECT * FROM [{ViewName}] {GetSpecifyingPageSuffix(specifyingObject)} {GetOrderingPageSuffix(specifyingObject)} LIMIT {offset}, {pageSize};";
            
            return executeOnExistingOrNewConnectionAndReturn(
                c => c.Query<TPersistance>(query).Select(tt => ConvertToDomain(tt))
                , connection);
        }

        public virtual int GetCount(TSpecifying specifyingObject, DbConnection connection = null)
        {
            string query = $"SELECT COUNT(*) FROM [{ViewName}] {GetSpecifyingCountSuffix(specifyingObject)};";
            
            return executeOnExistingOrNewConnectionAndReturn(
                c => c.Query<int>(query).First()
                , connection);
        }
        
        protected abstract Converter<TPersistance, TDomainBase> ConvertToDomain { get; }
        protected abstract string ViewName { get; }
        protected abstract string GetOrderingPageSuffix(TSpecifying specifyingObject);
        protected abstract string GetSpecifyingPageSuffix(TSpecifying specifyingObject);
        protected abstract string GetSpecifyingCountSuffix(TSpecifying specifyingObject);
    }
}