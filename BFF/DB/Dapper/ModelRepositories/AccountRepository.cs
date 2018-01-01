using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;
using Dapper;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateAccountTable : CreateTableBase
    {
        public CreateAccountTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Account)}s](
            {nameof(Account.Id)} INTEGER PRIMARY KEY,
            {nameof(Account.Name)} VARCHAR(100),
            {nameof(Account.StartingBalance)} INTEGER NOT NULL DEFAULT 0,
            {nameof(Account.StartingDate)} DATETIME NOT NULL);";
    }
    
    public class AccountComparer : Comparer<Domain.IAccount>
    {
        public override int Compare(Domain.IAccount x, Domain.IAccount y)
        {
            return Comparer<string>.Default.Compare(x?.Name, y?.Name);
        }
    }

    public interface IAccountRepository : IObservableRepositoryBase<Domain.IAccount>
    {
        long? GetBalance(Domain.IAccount account, DbConnection connection = null);

        long? GetBalanceUntilNow(Domain.IAccount account, DbConnection connection = null);
    }

    public sealed class AccountRepository : ObservableRepositoryBase<Domain.IAccount, Account>, IAccountRepository
    {
        public AccountRepository(IProvideConnection provideConnection) : base(provideConnection, new AccountComparer())
        { }

        public override Domain.IAccount Create() =>
            new Domain.Account(this, DateTime.Today);

        protected override Converter<Domain.IAccount, Account> ConvertToPersistence => domainAccount => 
            new Account
            {
                Id = domainAccount.Id,
                Name = domainAccount.Name,
                StartingBalance = domainAccount.StartingBalance,
                StartingDate = domainAccount.StartingDate
            };

        protected override Converter<(Account, DbConnection), Domain.IAccount> ConvertToDomain => tuple =>
        {
            (Account persistenceAccount, _) = tuple;
            return new Domain.Account(this,
                persistenceAccount.StartingDate,
                persistenceAccount.Id,
                persistenceAccount.Name,
                persistenceAccount.StartingBalance);
        };

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

        private long? GetAccountBalance(Domain.IAccount account, DbConnection connection = null) =>
            ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.Query<long>(AccountSpecificBalanceStatement, new { accountId = account?.Id ?? -1 }),
                ProvideConnection,
                connection).First();

        private long? GetAccountBalanceUntilNow(Domain.IAccount account, DbConnection connection = null) =>
            ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.Query<long>(AccountSpecificBalanceUntilNowStatement, new { accountId = account?.Id ?? -1, DateTimeNow = DateTime.Now }),
                ProvideConnection,
                connection).First();

        private long? GetSummaryAccountBalance(DbConnection connection = null) =>
            ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.Query<long>(AllAccountsBalanceStatement),
                ProvideConnection,
                connection).First();

        private long? GetSummaryAccountBalanceUntilNow(DbConnection connection = null) =>
            ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.Query<long>(AllAccountsBalanceUntilNowStatement, new { DateTimeNow = DateTime.Now }),
                ProvideConnection,
                connection).First();

        public long? GetBalance(Domain.IAccount account, DbConnection connection = null)
        {
            try
            {
                switch(account)
                {
                    case Domain.ISummaryAccount _:
                        return GetSummaryAccountBalance(connection);
                    case Domain.IAccount specificAccount:
                        return GetAccountBalance(specificAccount, connection);
                    default:
                        return null;
                }
            }
            catch (OverflowException)
            {
                return null;
            }
        }

        public long? GetBalanceUntilNow(Domain.IAccount account, DbConnection connection = null)
        {
            try
            {
                switch (account)
                {
                    case Domain.ISummaryAccount _:
                        return GetSummaryAccountBalanceUntilNow(connection);
                    case Domain.IAccount specificAccount:
                        return GetAccountBalanceUntilNow(specificAccount, connection);
                    default:
                        return null;
                }
            }
            catch (OverflowException)
            {
                return null;
            }
        }
    }
}