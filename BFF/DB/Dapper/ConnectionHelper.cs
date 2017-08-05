using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Transactions;

namespace BFF.DB.Dapper
{
    public static class ConnectionHelper
    {
        public static void ExecuteOnExistingOrNewConnection(Action<DbConnection> action, IProvideConnection provideConnection, DbConnection connection = null)
        {
            if(connection != null) action(connection);
            else
            {
                using(TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
                using(DbConnection newConnection = provideConnection.Connection)
                {
                    newConnection.Open();
                    action(newConnection);
                    transactionScope.Complete();
                }
            }
        }
        
        public static IEnumerable<T> QueryOnExistingOrNewConnection<T>(
            Func<DbConnection, IEnumerable<T>> action, 
            IProvideConnection provideConnection,
            DbConnection connection = null)
        {
            if(connection != null) return action(connection);

            IEnumerable<T> ret;
            using(TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
            using(DbConnection newConnection = provideConnection.Connection)
            {
                newConnection.Open();
                ret = action(newConnection).ToList();
                transactionScope.Complete();
            }
            return ret;
        }
    }
}