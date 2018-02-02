using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Transactions;
using BFF.DB.PersistenceModels;
using Dapper;

namespace BFF.DB.SQLite
{
    class DapperParentalOrm : IParentalOrm
    {
        private string ParentalQuery =>
            $"SELECT * FROM [{typeof(SubTransaction).Name}s] WHERE {nameof(SubTransaction.ParentId)} = @ParentId;";

        private readonly IProvideConnection _provideConnection;

        public DapperParentalOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }


        public IEnumerable<SubTransaction> ReadSubTransactionsOf(long parentTransactionId)
        {
            IList<SubTransaction> ret;
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
            using (DbConnection newConnection = _provideConnection.Connection)
            {
                newConnection.Open();
                ret = newConnection.Query<SubTransaction>(ParentalQuery, new { ParentId = parentTransactionId }).ToList();
                transactionScope.Complete();
            }
            return ret;
        }
    }
}
