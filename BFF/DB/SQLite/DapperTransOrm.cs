using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.DB.PersistenceModels;
using Dapper;

namespace BFF.DB.SQLite
{
    class DapperTransOrm : ITransOrm
    {
        private static string OrderingPageSuffix => $"ORDER BY {nameof(Trans.Date)}";

        private static string SpecifyingPart => $"WHERE {nameof(Trans.AccountId)} = @accountId OR {nameof(Trans.AccountId)} = -69 AND {nameof(Trans.PayeeId)} = @accountId OR {nameof(Trans.AccountId)} = -69 AND {nameof(Trans.CategoryId)} = @accountId";

        private static string PagePart => $"SELECT * FROM {nameof(Trans)}s";

        private static string CountPart => $"SELECT COUNT(*) FROM {nameof(Trans)}s";

        private static string LimitPart => "LIMIT @offset, @pageSize";


        private readonly IProvideConnection _provideConnection;

        public DapperTransOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public IEnumerable<Trans> GetPageFromSpecificAccount(int offset, int pageSize, long accountId)
        {
            IList<Trans> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = connection.Query<Trans>($"{PagePart} {SpecifyingPart} {OrderingPageSuffix} {LimitPart};", new { offset, pageSize, accountId}).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public IEnumerable<Trans> GetPageFromSummaryAccount(int offset, int pageSize)
        {
            IList<Trans> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = connection.Query<Trans>($"{PagePart} {OrderingPageSuffix} {LimitPart};", new { offset, pageSize }).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public long GetCountFromSpecificAccount(long accountId)
        {
            long ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = connection.Query<long>($"{CountPart} {SpecifyingPart};", new { accountId }).First();
                transactionScope.Complete();
            }
            return ret;
        }

        public long GetCountFromSummaryAccount()
        {
            long ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = connection.Query<long>($"{CountPart};").First();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<Trans>> GetPageFromSpecificAccountAsync(int offset, int pageSize, long accountId)
        {
            IList<Trans> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = (await connection
                    .QueryAsync<Trans>($"{PagePart} {SpecifyingPart} {OrderingPageSuffix} {LimitPart};", new { offset, pageSize, accountId })
                    .ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<Trans>> GetPageFromSummaryAccountAsync(int offset, int pageSize)
        {
            IList<Trans> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = (await connection
                    .QueryAsync<Trans>($"{PagePart} {OrderingPageSuffix} {LimitPart};", new { offset, pageSize })
                    .ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<long> GetCountFromSpecificAccountAsync(long accountId)
        {
            long ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = (await connection.QueryAsync<long>($"{CountPart} {SpecifyingPart};", new { accountId }).ConfigureAwait(false)).First();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<long> GetCountFromSummaryAccountAsync()
        {
            long ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = (await connection.QueryAsync<long>($"{CountPart};").ConfigureAwait(false)).First();
                transactionScope.Complete();
            }
            return ret;
        }
    }
}
