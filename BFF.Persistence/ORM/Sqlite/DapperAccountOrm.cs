using System;
using System.Data;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Core.Helper;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite.Interfaces;
using Dapper;

namespace BFF.Persistence.ORM.Sqlite
{
    internal class DapperAccountOrm : IAccountOrm
    {
        private readonly IProvideSqliteConnection _provideConnection;

        private string AllAccountsClearedBalanceStatement =>
            $@"SELECT Total({nameof(Trans.Sum)}) FROM (
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.Cleared)} == 1 UNION ALL 
            SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)} WHERE {nameof(Trans.Cleared)} == 1 UNION ALL 
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.PayeeId)} IS NULL AND {nameof(Trans.Cleared)} == 1 UNION ALL
            SELECT - {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.CategoryId)} IS NULL AND {nameof(Trans.Cleared)} == 1 UNION ALL
            SELECT {nameof(Account.StartingBalance)} FROM {nameof(Account)}s);";

        private string AllAccountsClearedBalanceUntilNowStatement =>
            $@"SELECT Total({nameof(Trans.Sum)}) FROM (
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 1 UNION ALL 
            SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)} WHERE {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 1 UNION ALL 
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.PayeeId)} IS NULL AND {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 1 UNION ALL
            SELECT - {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.CategoryId)} IS NULL AND {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 1 UNION ALL
            SELECT {nameof(Account.StartingBalance)} FROM {nameof(Account)}s WHERE {nameof(Account.StartingBalance)} <= @DateTimeNow);";

        private string AccountSpecificClearedBalanceStatement =>
            $@"SELECT (SELECT Total({nameof(Trans.Sum)}) FROM (
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.AccountId)} = @accountId AND {nameof(Trans.Cleared)} == 1 UNION ALL 
            SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)} WHERE {nameof(Trans.AccountId)} = @accountId AND {nameof(Trans.Cleared)} == 1 UNION ALL
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.CategoryId)} = @accountId AND {nameof(Trans.Cleared)} == 1 UNION ALL
            SELECT {nameof(Account.StartingBalance)} FROM {nameof(Account)}s WHERE {nameof(Account.Id)} = @accountId)) 
            - (SELECT Total({nameof(Trans.Sum)}) FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.PayeeId)} = @accountId AND {nameof(Trans.Cleared)} == 1);";

        private string AccountSpecificClearedBalanceUntilNowStatement =>
            $@"SELECT (SELECT Total({nameof(Trans.Sum)}) FROM (
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.AccountId)} = @accountId AND {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 1 UNION ALL 
            SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)} WHERE {nameof(Trans.AccountId)} = @accountId AND {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 1 UNION ALL
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.CategoryId)} = @accountId AND {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 1 UNION ALL
            SELECT {nameof(Account.StartingBalance)} FROM {nameof(Account)}s WHERE {nameof(Account.Id)} = @accountId AND {nameof(Account.StartingBalance)} <= @DateTimeNow)) 
            - (SELECT Total({nameof(Trans.Sum)}) FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.PayeeId)} = @accountId AND {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 1);";

        private string AllAccountsUnclearedBalanceStatement =>
            $@"SELECT Total({nameof(Trans.Sum)}) FROM (
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.Cleared)} == 0 UNION ALL 
            SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)} WHERE {nameof(Trans.Cleared)} == 0 UNION ALL 
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.PayeeId)} IS NULL AND {nameof(Trans.Cleared)} == 0 UNION ALL
            SELECT - {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.CategoryId)} IS NULL AND {nameof(Trans.Cleared)} == 0);";

        private string AllAccountsUnclearedBalanceUntilNowStatement =>
            $@"SELECT Total({nameof(Trans.Sum)}) FROM (
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 0 UNION ALL 
            SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)} WHERE {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 0 UNION ALL 
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.PayeeId)} IS NULL AND {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 0 UNION ALL
            SELECT - {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.CategoryId)} IS NULL AND {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 0);";

        private string AccountSpecificUnclearedBalanceStatement =>
            $@"SELECT (SELECT Total({nameof(Trans.Sum)}) FROM (
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.AccountId)} = @accountId AND {nameof(Trans.Cleared)} == 0 UNION ALL 
            SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)} WHERE {nameof(Trans.AccountId)} = @accountId AND {nameof(Trans.Cleared)} == 0 UNION ALL
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.CategoryId)} = @accountId AND {nameof(Trans.Cleared)} == 0)) 
            - (SELECT Total({nameof(Trans.Sum)}) FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.PayeeId)} = @accountId AND {nameof(Trans.Cleared)} == 0);";

        private string AccountSpecificUnclearedBalanceUntilNowStatement =>
            $@"SELECT (SELECT Total({nameof(Trans.Sum)}) FROM (
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.AccountId)} = @accountId AND {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 0 UNION ALL 
            SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)} WHERE {nameof(Trans.AccountId)} = @accountId AND {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 0 UNION ALL
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.CategoryId)} = @accountId AND {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 0)) 
            - (SELECT Total({nameof(Trans.Sum)}) FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.PayeeId)} = @accountId AND {nameof(Trans.Date)} <= @DateTimeNow AND {nameof(Trans.Cleared)} == 0);";

        public DapperAccountOrm(IProvideSqliteConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task<long?> GetClearedBalanceAsync(long id)
        {
            long? ret;

            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = await connection.QueryFirstOrDefaultAsync<long?>(AccountSpecificClearedBalanceStatement, new { accountId = id }).ConfigureAwait(false);
                transactionScope.Complete();
            }

            return ret;
        }

        public async Task<long?> GetClearedBalanceUntilNowAsync(long id)
        {
            long? ret;

            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = await connection.QueryFirstOrDefaultAsync<long?>(AccountSpecificClearedBalanceUntilNowStatement, new { accountId = id, DateTimeNow = DateTime.Now }).ConfigureAwait(false);
                transactionScope.Complete();
            }

            return ret;
        }

        public async Task<long?> GetClearedOverallBalanceAsync()
        {
            long? ret;

            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = await connection.QueryFirstOrDefaultAsync<long?>(AllAccountsClearedBalanceStatement).ConfigureAwait(false);
                transactionScope.Complete();
            }

            return ret;
        }

        public async Task<long?> GetClearedOverallBalanceUntilNowAsync()
        {
            long? ret;

            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = await connection.QueryFirstOrDefaultAsync<long?>(AllAccountsClearedBalanceUntilNowStatement, new { DateTimeNow = DateTime.Now }).ConfigureAwait(false);
                transactionScope.Complete();
            }

            return ret;
        }

        public async Task<long?> GetUnclearedBalanceAsync(long id)
        {
            long? ret;

            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = await connection.QueryFirstOrDefaultAsync<long?>(AccountSpecificUnclearedBalanceStatement, new { accountId = id }).ConfigureAwait(false);
                transactionScope.Complete();
            }

            return ret;
        }

        public async Task<long?> GetUnclearedBalanceUntilNowAsync(long id)
        {
            long? ret;

            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = await connection.QueryFirstOrDefaultAsync<long?>(AccountSpecificUnclearedBalanceUntilNowStatement, new { accountId = id, DateTimeNow = DateTime.Now }).ConfigureAwait(false);
                transactionScope.Complete();
            }

            return ret;
        }

        public async Task<long?> GetUnclearedOverallBalanceAsync()
        {
            long? ret;

            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = await connection.QueryFirstOrDefaultAsync<long?>(AllAccountsUnclearedBalanceStatement).ConfigureAwait(false);
                transactionScope.Complete();
            }

            return ret;
        }

        public async Task<long?> GetUnclearedOverallBalanceUntilNowAsync()
        {
            long? ret;

            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                ret = await connection.QueryFirstOrDefaultAsync<long?>(AllAccountsUnclearedBalanceUntilNowStatement, new { DateTimeNow = DateTime.Now }).ConfigureAwait(false);
                transactionScope.Complete();
            }

            return ret;
        }
    }
}
