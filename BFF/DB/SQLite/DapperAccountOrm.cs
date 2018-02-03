using System;
using System.Data.Common;
using System.Linq;
using System.Transactions;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;
using Dapper;

namespace BFF.DB.SQLite
{
    class DapperAccountOrm : IAccountOrm
    {
        private readonly IProvideConnection _provideConnection;

        private string AllAccountsBalanceStatement =>
            $@"SELECT Total({nameof(Trans.Sum)}) FROM (
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' UNION ALL 
            SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s UNION ALL 
            SELECT {nameof(Account.StartingBalance)} FROM {nameof(Account)}s);";

        private string AllAccountsBalanceUntilNowStatement =>
            $@"SELECT Total({nameof(Trans.Sum)}) FROM (
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.Date)} <= @DateTimeNow UNION ALL 
            SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)} AND {nameof(Trans.Date)} <= @DateTimeNow UNION ALL 
            SELECT {nameof(Account.StartingBalance)} FROM {nameof(Account)}s WHERE {nameof(Account.StartingBalance)} <= @DateTimeNow);";

        private string AccountSpecificBalanceStatement =>
            $@"SELECT (SELECT Total({nameof(Trans.Sum)}) FROM (
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.AccountId)} = @accountId UNION ALL 
            SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)} AND {nameof(Trans.AccountId)} = @accountId UNION ALL
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.CategoryId)} = @accountId UNION ALL
            SELECT {nameof(Account.StartingBalance)} FROM {nameof(Account)}s WHERE {nameof(Account.Id)} = @accountId)) 
            - (SELECT Total({nameof(Trans.Sum)}) FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.PayeeId)} = @accountId);";

        private string AccountSpecificBalanceUntilNowStatement =>
            $@"SELECT (SELECT Total({nameof(Trans.Sum)}) FROM (
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.AccountId)} = @accountId AND {nameof(Trans.Date)} <= @DateTimeNow UNION ALL 
            SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)} AND {nameof(Trans.AccountId)} = @accountId AND {nameof(Trans.Date)} <= @DateTimeNow UNION ALL
            SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.CategoryId)} = @accountId AND {nameof(Trans.Date)} <= @DateTimeNow UNION ALL
            SELECT {nameof(Account.StartingBalance)} FROM {nameof(Account)}s WHERE {nameof(Account.Id)} = @accountId AND {nameof(Account.StartingBalance)} <= @DateTimeNow)) 
            - (SELECT Total({nameof(Trans.Sum)}) FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transfer)}' AND {nameof(Trans.PayeeId)} = @accountId AND {nameof(Trans.Date)} <= @DateTimeNow);";


        public DapperAccountOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public long? GetBalance(long id)
        {
            long? ret;

            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = connection.Query<long?>(AccountSpecificBalanceStatement, new { accountId = id }).FirstOrDefault();
                transactionScope.Complete();
            }

            return ret;
        }

        public long? GetBalanceUntilNow(long id)
        {
            long? ret;

            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = connection.Query<long?>(AccountSpecificBalanceUntilNowStatement, new { accountId = id, DateTimeNow = DateTime.Now }).FirstOrDefault();
                transactionScope.Complete();
            }

            return ret;
        }

        public long? GetOverallBalance()
        {
            long? ret;

            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = connection.Query<long?>(AllAccountsBalanceStatement).FirstOrDefault();
                transactionScope.Complete();
            }

            return ret;
        }

        public long? GetOverallBalanceUntilNow()
        {
            long? ret;

            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();
                ret = connection.Query<long?>(AllAccountsBalanceUntilNowStatement, new { DateTimeNow = DateTime.Now }).FirstOrDefault();
                transactionScope.Complete();
            }

            return ret;
        }
    }
}
