using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using Dapper;

namespace BFF.Persistence.ORM.Sqlite
{
    internal class DapperParentalOrm : IParentalOrm
    {
        private string ParentalQuery =>
            $"SELECT * FROM [{typeof(SubTransactionDto).Name}s] WHERE {nameof(SubTransactionDto.ParentId)} = @ParentId;";

        private readonly IProvideConnection _provideConnection;

        public DapperParentalOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }


        public IEnumerable<SubTransactionDto> ReadSubTransactionsOf(long parentTransactionId)
        {
            IList<SubTransactionDto> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = connection.Query<SubTransactionDto>(ParentalQuery, new { ParentId = parentTransactionId }).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<SubTransactionDto>> ReadSubTransactionsOfAsync(long parentTransactionId)
        {
            IList<SubTransactionDto> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = (await connection.QueryAsync<SubTransactionDto>(ParentalQuery, new { ParentId = parentTransactionId }).ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }
    }
}
