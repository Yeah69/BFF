using System;
using System.Data.Common;
using System.Linq;
using Dapper;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateAccountTable : CreateTableBase
    {
        public CreateAccountTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.Account)}s](
            {nameof(Persistance.Account.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.Account.Name)} VARCHAR(100),
            {nameof(Persistance.Account.StartingBalance)} INTEGER NOT NULL DEFAULT 0);";
    }

    public class AccountRepository : RepositoryBase<Domain.Account, Persistance.Account>
    {
        public AccountRepository(IProvideConnection provideConnection) : base(provideConnection) { }

        public override Domain.Account Create() =>
            new Domain.Account(this);

        protected override Converter<Domain.Account, Persistance.Account> ConvertToPersistance => domainAccount => 
            new Persistance.Account
            {
                Id = domainAccount.Id,
                Name = domainAccount.Name,
                StartingBalance = domainAccount.StartingBalance
            };
        
        protected override Converter<Persistance.Account, Domain.Account> ConvertToDomain => persistanceAccount =>
            new Domain.Account(this)
            {
                Id = persistanceAccount.Id,
                Name = persistanceAccount.Name,
                StartingBalance = persistanceAccount.StartingBalance
            };
        
        private string AllAccountsBalanceStatement =>
            $@"SELECT Total({nameof(Persistance.Transaction.Sum)}) FROM (
            SELECT {nameof(Persistance.Transaction.Sum)} FROM {nameof(Persistance.Transaction)}s UNION ALL 
            SELECT {nameof(Persistance.Income.Sum)} FROM {nameof(Persistance.Income)}s UNION ALL 
            SELECT {nameof(Persistance.SubTransaction.Sum)} FROM {nameof(Persistance.SubTransaction)}s UNION ALL 
            SELECT {nameof(Persistance.SubIncome.Sum)} FROM {nameof(Persistance.SubIncome)}s UNION ALL 
            SELECT {nameof(Persistance.Account.StartingBalance)} FROM {nameof(Persistance.Account)}s);";

        private string AccountSpecificBalanceStatement =>
            $@"SELECT (SELECT Total({nameof(Persistance.Transaction.Sum)}) FROM (
            SELECT {nameof(Persistance.Transaction.Sum)} FROM {nameof(Persistance.Transaction)}s WHERE {nameof(Persistance.Transaction.AccountId)} = @accountId UNION ALL 
            SELECT {nameof(Persistance.Income.Sum)} FROM {nameof(Persistance.Income)}s WHERE {nameof(Persistance.Income.AccountId)} = @accountId UNION ALL
            SELECT {nameof(Persistance.SubTransaction.Sum)} FROM {nameof(Persistance.SubTransaction)}s INNER JOIN {nameof(Persistance.ParentTransaction)}s ON {nameof(Persistance.SubTransaction.ParentId)} = {nameof(Persistance.ParentTransaction)}s.{nameof(Persistance.ParentTransaction.Id)} AND {nameof(Persistance.ParentTransaction.AccountId)} = @accountId UNION ALL
            SELECT {nameof(Persistance.SubIncome.Sum)} FROM {nameof(Persistance.SubIncome)}s INNER JOIN {nameof(Persistance.ParentIncome)}s ON {nameof(Persistance.SubIncome.ParentId)} = {nameof(Persistance.ParentIncome)}s.{nameof(Persistance.ParentIncome.Id)} AND {nameof(Persistance.ParentIncome.AccountId)} = @accountId UNION ALL
            SELECT {nameof(Persistance.Transfer.Sum)} FROM {nameof(Persistance.Transfer)}s WHERE {nameof(Persistance.Transfer.ToAccountId)} = @accountId UNION ALL
            SELECT {nameof(Persistance.Account.StartingBalance)} FROM {nameof(Persistance.Account)}s WHERE {nameof(Persistance.Account.Id)} = @accountId)) 
            - (SELECT Total({nameof(Persistance.Transfer.Sum)}) FROM {nameof(Persistance.Transfer)}s WHERE {nameof(Persistance.Transfer.FromAccountId)} = @accountId);";

        private long? GetAccountBalance(Domain.Account account, DbConnection connection = null) => 
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
                    case Domain.SummaryAccount _:
                        return GetSummaryAccountBalance(connection);
                    case Domain.Account specificAccount:
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