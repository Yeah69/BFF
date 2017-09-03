using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BFF.DB.PersistenceModels;
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
            {nameof(Account.StartingBalance)} INTEGER NOT NULL DEFAULT 0);";
    }
    
    public class AccountComparer : Comparer<Domain.IAccount>
    {
        public override int Compare(Domain.IAccount x, Domain.IAccount y)
        {
            return Comparer<string>.Default.Compare(x.Name, y.Name);
        }
    }

    public interface IAccountRepository : IObservableRepositoryBase<Domain.IAccount>
    {
        long? GetBalance(Domain.IAccount account, DbConnection connection = null);
    }

    public class AccountRepository : ObservableRepositoryBase<Domain.IAccount, Account>, IAccountRepository
    {
        public AccountRepository(IProvideConnection provideConnection) : base(provideConnection, new AccountComparer())
        { }

        public override Domain.IAccount Create() =>
            new Domain.Account(this);

        protected override Converter<Domain.IAccount, Account> ConvertToPersistence => domainAccount => 
            new Account
            {
                Id = domainAccount.Id,
                Name = domainAccount.Name,
                StartingBalance = domainAccount.StartingBalance
            };

        protected override Converter<(Account, DbConnection), Domain.IAccount> ConvertToDomain => tuple =>
        {
            (Account persistenceAccount, _) = tuple;
            return new Domain.Account(this,
                persistenceAccount.Id,
                persistenceAccount.Name,
                persistenceAccount.StartingBalance);
        };
        
        private string AllAccountsBalanceStatement =>
            $@"SELECT Total({nameof(Transaction.Sum)}) FROM (
            SELECT {nameof(Transaction.Sum)} FROM {nameof(Transaction)}s UNION ALL 
            SELECT {nameof(Income.Sum)} FROM {nameof(Income)}s UNION ALL 
            SELECT {nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s UNION ALL 
            SELECT {nameof(SubIncome.Sum)} FROM {nameof(SubIncome)}s UNION ALL 
            SELECT {nameof(Account.StartingBalance)} FROM {nameof(Account)}s);";

        private string AccountSpecificBalanceStatement =>
            $@"SELECT (SELECT Total({nameof(Transaction.Sum)}) FROM (
            SELECT {nameof(Transaction.Sum)} FROM {nameof(Transaction)}s WHERE {nameof(Transaction.AccountId)} = @accountId UNION ALL 
            SELECT {nameof(Income.Sum)} FROM {nameof(Income)}s WHERE {nameof(Income.AccountId)} = @accountId UNION ALL
            SELECT {nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s INNER JOIN {nameof(ParentTransaction)}s ON {nameof(SubTransaction.ParentId)} = {nameof(ParentTransaction)}s.{nameof(ParentTransaction.Id)} AND {nameof(ParentTransaction.AccountId)} = @accountId UNION ALL
            SELECT {nameof(SubIncome.Sum)} FROM {nameof(SubIncome)}s INNER JOIN {nameof(ParentIncome)}s ON {nameof(SubIncome.ParentId)} = {nameof(ParentIncome)}s.{nameof(ParentIncome.Id)} AND {nameof(ParentIncome.AccountId)} = @accountId UNION ALL
            SELECT {nameof(Transfer.Sum)} FROM {nameof(Transfer)}s WHERE {nameof(Transfer.ToAccountId)} = @accountId UNION ALL
            SELECT {nameof(Account.StartingBalance)} FROM {nameof(Account)}s WHERE {nameof(Account.Id)} = @accountId)) 
            - (SELECT Total({nameof(Transfer.Sum)}) FROM {nameof(Transfer)}s WHERE {nameof(Transfer.FromAccountId)} = @accountId);";

        private long? GetAccountBalance(Domain.IAccount account, DbConnection connection = null) => 
            ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.Query<long>(AccountSpecificBalanceStatement, new { accountId = account?.Id ?? -1 }), 
                ProvideConnection, 
                connection).First();

        private long? GetSummaryAccountBalance(DbConnection connection = null) => 
            ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.Query<long>(AllAccountsBalanceStatement), 
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
    }
}