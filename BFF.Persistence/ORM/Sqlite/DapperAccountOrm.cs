using System;
using System.Data;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Core;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using Dapper;

namespace BFF.Persistence.ORM.Sqlite
{
    internal class DapperAccountOrm : IAccountOrm
    {
        private readonly IProvideConnection _provideConnection;

        private string AllAccountsClearedBalanceStatement =>
            $@"SELECT Total({nameof(TransDto.Sum)}) FROM (
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(TransDto.Cleared)} == 1 UNION ALL 
            SELECT {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.Sum)} FROM {nameof(SubTransactionDto)}s INNER JOIN {nameof(TransDto)}s ON {nameof(SubTransactionDto.ParentId)} = {nameof(TransDto)}s.{nameof(TransDto.Id)} WHERE {nameof(TransDto.Cleared)} == 1 UNION ALL 
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.PayeeId)} IS NULL AND {nameof(TransDto.Cleared)} == 1 UNION ALL
            SELECT - {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.CategoryId)} IS NULL AND {nameof(TransDto.Cleared)} == 1 UNION ALL
            SELECT {nameof(AccountDto.StartingBalance)} FROM {nameof(AccountDto)}s);";

        private string AllAccountsClearedBalanceUntilNowStatement =>
            $@"SELECT Total({nameof(TransDto.Sum)}) FROM (
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 1 UNION ALL 
            SELECT {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.Sum)} FROM {nameof(SubTransactionDto)}s INNER JOIN {nameof(TransDto)}s ON {nameof(SubTransactionDto.ParentId)} = {nameof(TransDto)}s.{nameof(TransDto.Id)} WHERE {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 1 UNION ALL 
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.PayeeId)} IS NULL AND {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 1 UNION ALL
            SELECT - {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.CategoryId)} IS NULL AND {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 1 UNION ALL
            SELECT {nameof(AccountDto.StartingBalance)} FROM {nameof(AccountDto)}s WHERE {nameof(AccountDto.StartingBalance)} <= @DateTimeNow);";

        private string AccountSpecificClearedBalanceStatement =>
            $@"SELECT (SELECT Total({nameof(TransDto.Sum)}) FROM (
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(TransDto.AccountId)} = @accountId AND {nameof(TransDto.Cleared)} == 1 UNION ALL 
            SELECT {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.Sum)} FROM {nameof(SubTransactionDto)}s INNER JOIN {nameof(TransDto)}s ON {nameof(SubTransactionDto.ParentId)} = {nameof(TransDto)}s.{nameof(TransDto.Id)} WHERE {nameof(TransDto.AccountId)} = @accountId AND {nameof(TransDto.Cleared)} == 1 UNION ALL
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.CategoryId)} = @accountId AND {nameof(TransDto.Cleared)} == 1 UNION ALL
            SELECT {nameof(AccountDto.StartingBalance)} FROM {nameof(AccountDto)}s WHERE {nameof(AccountDto.Id)} = @accountId)) 
            - (SELECT Total({nameof(TransDto.Sum)}) FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.PayeeId)} = @accountId AND {nameof(TransDto.Cleared)} == 1);";

        private string AccountSpecificClearedBalanceUntilNowStatement =>
            $@"SELECT (SELECT Total({nameof(TransDto.Sum)}) FROM (
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(TransDto.AccountId)} = @accountId AND {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 1 UNION ALL 
            SELECT {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.Sum)} FROM {nameof(SubTransactionDto)}s INNER JOIN {nameof(TransDto)}s ON {nameof(SubTransactionDto.ParentId)} = {nameof(TransDto)}s.{nameof(TransDto.Id)} WHERE {nameof(TransDto.AccountId)} = @accountId AND {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 1 UNION ALL
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.CategoryId)} = @accountId AND {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 1 UNION ALL
            SELECT {nameof(AccountDto.StartingBalance)} FROM {nameof(AccountDto)}s WHERE {nameof(AccountDto.Id)} = @accountId AND {nameof(AccountDto.StartingBalance)} <= @DateTimeNow)) 
            - (SELECT Total({nameof(TransDto.Sum)}) FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.PayeeId)} = @accountId AND {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 1);";

        private string AllAccountsUnclearedBalanceStatement =>
            $@"SELECT Total({nameof(TransDto.Sum)}) FROM (
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(TransDto.Cleared)} == 0 UNION ALL 
            SELECT {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.Sum)} FROM {nameof(SubTransactionDto)}s INNER JOIN {nameof(TransDto)}s ON {nameof(SubTransactionDto.ParentId)} = {nameof(TransDto)}s.{nameof(TransDto.Id)} WHERE {nameof(TransDto.Cleared)} == 0 UNION ALL 
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.PayeeId)} IS NULL AND {nameof(TransDto.Cleared)} == 0 UNION ALL
            SELECT - {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.CategoryId)} IS NULL AND {nameof(TransDto.Cleared)} == 0);";

        private string AllAccountsUnclearedBalanceUntilNowStatement =>
            $@"SELECT Total({nameof(TransDto.Sum)}) FROM (
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 0 UNION ALL 
            SELECT {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.Sum)} FROM {nameof(SubTransactionDto)}s INNER JOIN {nameof(TransDto)}s ON {nameof(SubTransactionDto.ParentId)} = {nameof(TransDto)}s.{nameof(TransDto.Id)} WHERE {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 0 UNION ALL 
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.PayeeId)} IS NULL AND {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 0 UNION ALL
            SELECT - {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.CategoryId)} IS NULL AND {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 0);";

        private string AccountSpecificUnclearedBalanceStatement =>
            $@"SELECT (SELECT Total({nameof(TransDto.Sum)}) FROM (
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(TransDto.AccountId)} = @accountId AND {nameof(TransDto.Cleared)} == 0 UNION ALL 
            SELECT {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.Sum)} FROM {nameof(SubTransactionDto)}s INNER JOIN {nameof(TransDto)}s ON {nameof(SubTransactionDto.ParentId)} = {nameof(TransDto)}s.{nameof(TransDto.Id)} WHERE {nameof(TransDto.AccountId)} = @accountId AND {nameof(TransDto.Cleared)} == 0 UNION ALL
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.CategoryId)} = @accountId AND {nameof(TransDto.Cleared)} == 0)) 
            - (SELECT Total({nameof(TransDto.Sum)}) FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.PayeeId)} = @accountId AND {nameof(TransDto.Cleared)} == 0);";

        private string AccountSpecificUnclearedBalanceUntilNowStatement =>
            $@"SELECT (SELECT Total({nameof(TransDto.Sum)}) FROM (
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(TransDto.AccountId)} = @accountId AND {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 0 UNION ALL 
            SELECT {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.Sum)} FROM {nameof(SubTransactionDto)}s INNER JOIN {nameof(TransDto)}s ON {nameof(SubTransactionDto.ParentId)} = {nameof(TransDto)}s.{nameof(TransDto.Id)} WHERE {nameof(TransDto.AccountId)} = @accountId AND {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 0 UNION ALL
            SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.CategoryId)} = @accountId AND {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 0)) 
            - (SELECT Total({nameof(TransDto.Sum)}) FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(TransDto.PayeeId)} = @accountId AND {nameof(TransDto.Date)} <= @DateTimeNow AND {nameof(TransDto.Cleared)} == 0);";

        public DapperAccountOrm(IProvideConnection provideConnection)
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
