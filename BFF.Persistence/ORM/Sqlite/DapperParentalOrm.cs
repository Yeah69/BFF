using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Core.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using Dapper;

namespace BFF.Persistence.ORM.Sqlite
{
    internal class DapperParentalOrm : IParentalOrm
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
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = connection.Query<SubTransaction>(ParentalQuery, new { ParentId = parentTransactionId }).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<ISubTransactionDto>> ReadSubTransactionsOfAsync(long parentTransactionId)
        {
            IList<SubTransaction> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = (await connection.QueryAsync<SubTransaction>(ParentalQuery, new { ParentId = parentTransactionId }).ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }
    }
}
