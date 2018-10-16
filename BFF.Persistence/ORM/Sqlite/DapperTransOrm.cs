using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Core;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using Dapper;

namespace BFF.Persistence.ORM.Sqlite
{
    internal class DapperTransOrm : ITransOrm
    {
        private static string RowsPart => $"SELECT * FROM {nameof(TransDto)}s";
        private static string RowsParentTransactionPart => $"SELECT DISTINCT {nameof(TransDto)}s.{nameof(TransDto.Id)}, {nameof(TransDto)}s.{nameof(TransDto.FlagId)}, {nameof(TransDto)}s.{nameof(TransDto.CheckNumber)}, {nameof(TransDto)}s.{nameof(TransDto.AccountId)}, {nameof(TransDto)}s.{nameof(TransDto.PayeeId)}, {nameof(TransDto)}s.{nameof(TransDto.CategoryId)}, {nameof(TransDto)}s.{nameof(TransDto.Date)}, {nameof(TransDto)}s.{nameof(TransDto.Memo)}, {nameof(TransDto)}s.{nameof(TransDto.Sum)}, {nameof(TransDto)}s.{nameof(TransDto.Cleared)}, {nameof(TransDto)}s.{nameof(TransDto.Type)} FROM {nameof(TransDto)}s";

        private static string CountPart => $"SELECT COUNT(*) FROM {nameof(TransDto)}s";

        private static string SpecifyingTransactionMonthPart => $"WHERE {nameof(TransDto.Type)} = '{nameof(TransType.Transaction)}' AND strftime('%Y', {nameof(TransDto.Date)}) = @year AND strftime('%m', {nameof(TransDto.Date)}) = @month";

        private static string SpecifyingTransactionMonthCategoryPart => $"WHERE {nameof(TransDto.Type)} = '{nameof(TransType.Transaction)}' AND strftime('%Y', {nameof(TransDto.Date)}) = @year AND strftime('%m', {nameof(TransDto.Date)}) = @month AND {nameof(TransDto.CategoryId)} = @categoryId";

        private static string SpecifyingTransactionMonthCategoriesPart => $"WHERE {nameof(TransDto.Type)} = '{nameof(TransType.Transaction)}' AND strftime('%Y', {nameof(TransDto.Date)}) = @year AND strftime('%m', {nameof(TransDto.Date)}) = @month AND {nameof(TransDto.CategoryId)} IN @categoryIds";

        private static string JoinParentTransactionsWithSubTransactionsPart => $"INNER JOIN {nameof(SubTransactionDto)}s ON {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.ParentId)} = {nameof(TransDto)}s.{nameof(TransDto.Id)}";

        private static string SpecifyingParentTransactionMonthPart => $"WHERE {nameof(TransDto)}s.{nameof(TransDto.Type)} = '{nameof(TransType.ParentTransaction)}' AND strftime('%Y', {nameof(TransDto.Date)}) = @year AND strftime('%m', {nameof(TransDto.Date)}) = @month";

        private static string SpecifyingParentTransactionMonthCategoryPart => $"WHERE {nameof(TransDto)}s.{nameof(TransDto.Type)} = '{nameof(TransType.ParentTransaction)}' AND strftime('%Y', {nameof(TransDto.Date)}) = @year AND strftime('%m', {nameof(TransDto.Date)}) = @month AND {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.CategoryId)} = @categoryId";

        private static string SpecifyingParentTransactionMonthCategoriesPart => $"WHERE {nameof(TransDto)}s.{nameof(TransDto.Type)} = '{nameof(TransType.ParentTransaction)}' AND strftime('%Y', {nameof(TransDto.Date)}) = @year AND strftime('%m', {nameof(TransDto.Date)}) = @month AND {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.CategoryId)} IN @categoryIds";

        private static string SpecifyingPagePart => $"WHERE {nameof(TransDto.AccountId)} = @accountId OR {nameof(TransDto.AccountId)} = -69 AND {nameof(TransDto.PayeeId)} = @accountId OR {nameof(TransDto.AccountId)} = -69 AND {nameof(TransDto.CategoryId)} = @accountId";

        private static string LimitingPagePart => "LIMIT @offset, @pageSize";

        private static string OrderingSuffix => $"ORDER BY {nameof(TransDto.Date)}";


        private readonly IProvideConnection _provideConnection;

        public DapperTransOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public IEnumerable<TransDto> GetPageFromSpecificAccount(int offset, int pageSize, long accountId)
        {
            IList<TransDto> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = connection.Query<TransDto>($"{RowsPart} {SpecifyingPagePart} {OrderingSuffix} {LimitingPagePart};", new { offset, pageSize, accountId}).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public IEnumerable<TransDto> GetPageFromSummaryAccount(int offset, int pageSize)
        {
            IList<TransDto> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = connection.Query<TransDto>($"{RowsPart} {OrderingSuffix} {LimitingPagePart};", new { offset, pageSize }).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public long GetCountFromSpecificAccount(long accountId)
        {
            long ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = connection.Query<long>($"{CountPart} {SpecifyingPagePart};", new { accountId }).First();
                transactionScope.Complete();
            }
            return ret;
        }

        public long GetCountFromSummaryAccount()
        {
            long ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = connection.Query<long>($"{CountPart};").First();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<TransDto>> GetPageFromSpecificAccountAsync(int offset, int pageSize, long accountId)
        {
            IList<TransDto> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = (await connection
                    .QueryAsync<TransDto>($"{RowsPart} {SpecifyingPagePart} {OrderingSuffix} {LimitingPagePart};", new { offset, pageSize, accountId })
                    .ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<TransDto>> GetPageFromSummaryAccountAsync(int offset, int pageSize)
        {
            IList<TransDto> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = (await connection
                    .QueryAsync<TransDto>($"{RowsPart} {OrderingSuffix} {LimitingPagePart};", new { offset, pageSize })
                    .ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<TransDto>> GetFromMonthAsync(DateTime month)
        {
            IList<TransDto> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                string query = $@"SELECT * FROM
({RowsPart} {SpecifyingTransactionMonthPart}
UNION ALL
{RowsParentTransactionPart} {JoinParentTransactionsWithSubTransactionsPart} {
                        SpecifyingParentTransactionMonthPart
                    })
{OrderingSuffix};";
                ret = (await connection
                    .QueryAsync<TransDto>(query, new { year = $"{month.Year:0000}", month = $"{month.Month:00}" })
                    .ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<TransDto>> GetFromMonthAndCategoryAsync(DateTime month, long categoryId)
        {
            IList<TransDto> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                string query = $@"SELECT * FROM
({RowsPart} {SpecifyingTransactionMonthCategoryPart}
UNION ALL
{RowsParentTransactionPart} {JoinParentTransactionsWithSubTransactionsPart} {
                        SpecifyingParentTransactionMonthCategoryPart
                    })
{OrderingSuffix};";
                ret = (await connection
                    .QueryAsync<TransDto>(query, new{ year = $"{month.Year:0000}", month = $"{month.Month:00}", categoryId })
                    .ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<IEnumerable<TransDto>> GetFromMonthAndCategoriesAsync(DateTime month, long[] categoryIds)
        {
            IList<TransDto> ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                string query = $@"SELECT * FROM
({RowsPart} {SpecifyingTransactionMonthCategoriesPart}
UNION ALL
{RowsParentTransactionPart} {JoinParentTransactionsWithSubTransactionsPart} {
                        SpecifyingParentTransactionMonthCategoriesPart
                    })
{OrderingSuffix};";
                ret = (await connection
                    .QueryAsync<TransDto>(query, new { year = $"{month.Year:0000}", month = $"{month.Month:00}", categoryIds })
                    .ConfigureAwait(false)).ToList();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<long> GetCountFromSpecificAccountAsync(long accountId)
        {
            long ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = (await connection.QueryAsync<long>($"{CountPart} {SpecifyingPagePart};", new { accountId }).ConfigureAwait(false)).First();
                transactionScope.Complete();
            }
            return ret;
        }

        public async Task<long> GetCountFromSummaryAccountAsync()
        {
            long ret;
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = (await connection.QueryAsync<long>($"{CountPart};").ConfigureAwait(false)).First();
                transactionScope.Complete();
            }
            return ret;
        }
    }
}
