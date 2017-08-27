using System;
using System.Collections.Generic;
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
    
    public class AccountComparer : Comparer<Domain.IAccount>
    {
        public override int Compare(Domain.IAccount x, Domain.IAccount y)
        {
            return Comparer<string>.Default.Compare(x.Name, y.Name);
        }
    }

    public class AccountRepository : ObservableRepositoryBase<Domain.IAccount, Persistance.Account>
    {
        public AccountRepository(IProvideConnection provideConnection) : base(provideConnection, new AccountComparer())
        { }

        public override Domain.IAccount Create() =>
            new Domain.Account(this);

        protected override Converter<Domain.IAccount, Persistance.Account> ConvertToPersistance => domainAccount => 
            new Persistance.Account
            {
                Id = domainAccount.Id,
                Name = domainAccount.Name,
                StartingBalance = domainAccount.StartingBalance
            };

        protected override Converter<(Persistance.Account, DbConnection), Domain.IAccount> ConvertToDomain => tuple =>
        {
            (Persistance.Account persistenceAccount, _) = tuple;
            return new Domain.Account(this,
                persistenceAccount.Id,
                persistenceAccount.Name,
                persistenceAccount.StartingBalance);
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