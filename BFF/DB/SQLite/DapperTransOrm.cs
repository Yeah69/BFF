using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;
using Dapper;

namespace BFF.DB.SQLite
{
    class DapperTransOrm : ITransOrm
    {
        private static string RowsPart => $"SELECT * FROM {nameof(Trans)}s";
        private static string RowsParentTransactionPart => $"SELECT DISTINCT {nameof(Trans)}s.{nameof(Trans.Id)}, {nameof(Trans)}s.{nameof(Trans.FlagId)}, {nameof(Trans)}s.{nameof(Trans.CheckNumber)}, {nameof(Trans)}s.{nameof(Trans.AccountId)}, {nameof(Trans)}s.{nameof(Trans.PayeeId)}, {nameof(Trans)}s.{nameof(Trans.CategoryId)}, {nameof(Trans)}s.{nameof(Trans.Date)}, {nameof(Trans)}s.{nameof(Trans.Memo)}, {nameof(Trans)}s.{nameof(Trans.Sum)}, {nameof(Trans)}s.{nameof(Trans.Cleared)}, {nameof(Trans)}s.{nameof(Trans.Type)} FROM {nameof(Trans)}s";

        private static string CountPart => $"SELECT COUNT(*) FROM {nameof(Trans)}s";

        private static string SpecifyingTransactionMonthPart => $"WHERE {nameof(Trans.Type)} = '{nameof(TransType.Transaction)}' AND strftime('%Y', {nameof(Trans.Date)}) = @year AND strftime('%m', {nameof(Trans.Date)}) = @month";

        private static string SpecifyingTransactionMonthCategoryPart => $"WHERE {nameof(Trans.Type)} = '{nameof(TransType.Transaction)}' AND strftime('%Y', {nameof(Trans.Date)}) = @year AND strftime('%m', {nameof(Trans.Date)}) = @month AND {nameof(Trans.CategoryId)} = @categoryId";

        private static string SpecifyingTransactionMonthCategoriesPart => $"WHERE {nameof(Trans.Type)} = '{nameof(TransType.Transaction)}' AND strftime('%Y', {nameof(Trans.Date)}) = @year AND strftime('%m', {nameof(Trans.Date)}) = @month AND {nameof(Trans.CategoryId)} IN @categoryIds";

        private static string JoinParentTransactionsWithSubTransactionsPart => $"INNER JOIN {nameof(SubTransaction)}s ON {nameof(SubTransaction)}s.{nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)}";

        private static string SpecifyingParentTransactionMonthPart => $"WHERE {nameof(Trans)}s.{nameof(Trans.Type)} = '{nameof(TransType.ParentTransaction)}' AND strftime('%Y', {nameof(Trans.Date)}) = @year AND strftime('%m', {nameof(Trans.Date)}) = @month";

        private static string SpecifyingParentTransactionMonthCategoryPart => $"WHERE {nameof(Trans)}s.{nameof(Trans.Type)} = '{nameof(TransType.ParentTransaction)}' AND strftime('%Y', {nameof(Trans.Date)}) = @year AND strftime('%m', {nameof(Trans.Date)}) = @month AND {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)} = @categoryId";

        private static string SpecifyingParentTransactionMonthCategoriesPart => $"WHERE {nameof(Trans)}s.{nameof(Trans.Type)} = '{nameof(TransType.ParentTransaction)}' AND strftime('%Y', {nameof(Trans.Date)}) = @year AND strftime('%m', {nameof(Trans.Date)}) = @month AND {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)} IN @categoryIds";

        private static string SpecifyingPagePart => $"WHERE {nameof(Trans.AccountId)} = @accountId OR {nameof(Trans.AccountId)} = -69 AND {nameof(Trans.PayeeId)} = @accountId OR {nameof(Trans.AccountId)} = -69 AND {nameof(Trans.CategoryId)} = @accountId";

        private static string LimitingPagePart => "LIMIT @offset, @pageSize";

        private static string OrderingSuffix => $"ORDER BY {nameof(Trans.Date)}";


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
                ret = connection.Query<Trans>($"{RowsPart} {SpecifyingPagePart} {OrderingSuffix} {LimitingPagePart};", new { offset, pageSize, accountId}).ToList();
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
                ret = connection.Query<Trans>($"{RowsPart} {OrderingSuffix} {LimitingPagePart};", new { offset, pageSize }).ToList();
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
                ret = connection.Query<long>($"{CountPart} {SpecifyingPagePart};", new { accountId }).First();
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
                    .QueryAsync<Trans>($"{RowsPart} {SpecifyingPagePart} {OrderingSuffix} {LimitingPagePart};", new { offset, pageSize, accountId })
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
                    .QueryAsync<Trans>($"{RowsPart} {OrderingSuffix} {LimitingPagePart};", new { offset, pageSize })
                    .ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<Trans>> GetFromMonthAsync(DateTime month)
        {
            IList<Trans> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                string query = $@"SELECT * FROM
({RowsPart} {SpecifyingTransactionMonthPart}
UNION ALL
{RowsParentTransactionPart} {JoinParentTransactionsWithSubTransactionsPart} {
                        SpecifyingParentTransactionMonthPart
                    })
{OrderingSuffix};";

                connection.Open();
                ret = (await connection
                    .QueryAsync<Trans>(query, new { year = $"{month.Year:0000}", month = $"{month.Month:00}" })
                    .ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<Trans>> GetFromMonthAndCategoryAsync(DateTime month, long categoryId)
        {
            IList<Trans> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                string query = $@"SELECT * FROM
({RowsPart} {SpecifyingTransactionMonthCategoryPart}
UNION ALL
{RowsParentTransactionPart} {JoinParentTransactionsWithSubTransactionsPart} {
                        SpecifyingParentTransactionMonthCategoryPart
                    })
{OrderingSuffix};";

                connection.Open();
                ret = (await connection
                    .QueryAsync<Trans>(query, new{ year = $"{month.Year:0000}", month = $"{month.Month:00}", categoryId })
                    .ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<Trans>> GetFromMonthAndCategoriesAsync(DateTime month, long[] categoryIds)
        {
            IList<Trans> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                string query = $@"SELECT * FROM
({RowsPart} {SpecifyingTransactionMonthCategoriesPart}
UNION ALL
{RowsParentTransactionPart} {JoinParentTransactionsWithSubTransactionsPart} {
                        SpecifyingParentTransactionMonthCategoriesPart
                    })
{OrderingSuffix};";

                connection.Open();
                ret = (await connection
                    .QueryAsync<Trans>(query, new { year = $"{month.Year:0000}", month = $"{month.Month:00}", categoryIds })
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
                ret = (await connection.QueryAsync<long>($"{CountPart} {SpecifyingPagePart};", new { accountId }).ConfigureAwait(false)).First();
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
