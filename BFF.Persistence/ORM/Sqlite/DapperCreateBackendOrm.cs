using System;
using System.Data;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite.Interfaces;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.ORM.Sqlite
{
    internal class DapperCreateBackendOrm : ICreateBackendOrm
    {
        private static string CreateAccountTableStatement =>
            $@"CREATE TABLE [{nameof(Account)}s](
            {nameof(Account.Id)} INTEGER PRIMARY KEY,
            {nameof(Account.Name)} VARCHAR(100),
            {nameof(Account.StartingBalance)} INTEGER NOT NULL DEFAULT 0,
            {nameof(Account.StartingDate)} DATETIME NOT NULL);";

        private static string CreateBudgetEntryTableStatement =>
            $@"CREATE TABLE [{nameof(BudgetEntry)}s](
            {nameof(BudgetEntry.Id)} INTEGER PRIMARY KEY,
            {nameof(BudgetEntry.CategoryId)} INTEGER,
            {nameof(BudgetEntry.Month)} DATE,
            {nameof(BudgetEntry.Budget)} INTEGER,
            FOREIGN KEY({nameof(BudgetEntry.CategoryId)}) REFERENCES {nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";

        private static string CreateCategoryTableStatement =>
            $@"CREATE TABLE [{nameof(Category)}s](
            {nameof(Category.Id)} INTEGER PRIMARY KEY,
            {nameof(Category.ParentId)} INTEGER,
            {nameof(Category.Name)} VARCHAR(100),
            {nameof(Category.IsIncomeRelevant)} INTEGER,
            {nameof(Category.MonthOffset)} INTEGER,
            FOREIGN KEY({nameof(Category.ParentId)}) REFERENCES {nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";

        private static string CreateDbSettingTableStatement =>
            $@"CREATE TABLE [{nameof(DbSetting)}s](
            {nameof(DbSetting.Id)} INTEGER PRIMARY KEY, 
            {nameof(DbSetting.CurrencyCultureName)} VARCHAR(10),
            {nameof(DbSetting.DateCultureName)} VARCHAR(10));";

        private static string CreateFlagTableStatement =>
            $@"CREATE TABLE [{nameof(Flag)}s](
            {nameof(Flag.Id)} INTEGER PRIMARY KEY,
            {nameof(Flag.Name)} VARCHAR(100),
            {nameof(Flag.Color)} INTEGER);";

        private static string CreatePayeeTableStatement =>
            $@"CREATE TABLE [{nameof(Payee)}s](
            {nameof(Payee.Id)} INTEGER PRIMARY KEY,
            {nameof(Payee.Name)} VARCHAR(100));";

        private static string CreateSubTransactionTableStatement =>
            $@"CREATE TABLE [{nameof(SubTransaction)}s](
            {nameof(SubTransaction.Id)} INTEGER PRIMARY KEY,
            {nameof(SubTransaction.ParentId)} INTEGER,
            {nameof(SubTransaction.CategoryId)} INTEGER,
            {nameof(SubTransaction.Memo)} TEXT,
            {nameof(SubTransaction.Sum)} INTEGER,
            FOREIGN KEY({nameof(SubTransaction.ParentId)}) REFERENCES {nameof(Trans)}s({nameof(Trans.Id)}) ON DELETE CASCADE);";

        private static string CreateTransTableStatement =>
            $@"CREATE TABLE {nameof(Trans)}s(
            {nameof(Trans.Id)} INTEGER PRIMARY KEY,
            {nameof(Trans.FlagId)} INTEGER,
            {nameof(Trans.CheckNumber)} TEXT,
            {nameof(Trans.AccountId)} INTEGER,
            {nameof(Trans.PayeeId)} INTEGER,
            {nameof(Trans.CategoryId)} INTEGER,
            {nameof(Trans.Date)} DATE,
            {nameof(Trans.Memo)} TEXT,
            {nameof(Trans.Sum)} INTEGER,
            {nameof(Trans.Cleared)} INTEGER,
            {nameof(Trans.Type)} VARCHAR(17),
            FOREIGN KEY({nameof(Trans.FlagId)}) REFERENCES {nameof(Flag)}s({nameof(Flag.Id)}) ON DELETE SET NULL);
            CREATE INDEX {nameof(Trans)}s_{nameof(Trans.AccountId)}_{nameof(Trans.PayeeId)}_index ON {nameof(Trans)}s ({nameof(Trans.AccountId)}, {nameof(Trans.PayeeId)});
            CREATE INDEX {nameof(Trans)}s_{nameof(Trans.AccountId)}_{nameof(Trans.CategoryId)}_index ON {nameof(Trans)}s ({nameof(Trans.AccountId)}, {nameof(Trans.CategoryId)});
            CREATE INDEX {nameof(Trans)}s_{nameof(Trans.Date)}_index ON {nameof(Trans)}s ({nameof(Trans.Date)});";
        
        /// <summary>
        /// First for digits are the release year, next two digits are the release counter, next three are the patch counter
        /// </summary>
        private static readonly int DatabaseSchemaVersion = 201801001;

        private static readonly string SetDatabaseSchemaVersion = $@"PRAGMA user_version = {DatabaseSchemaVersion};";

        private readonly IProvideSqliteConnection _provideConnection;
        private readonly Func<DbSetting> _dbSettingFactory;

        public DapperCreateBackendOrm(IProvideSqliteConnection provideConnection, Func<DbSetting> dbSettingFactory)
        {
            _provideConnection = provideConnection;
            _dbSettingFactory = dbSettingFactory;
        }

        public async Task CreateAsync()
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                await Task.WhenAll(
                        connection.ExecuteAsync(CreateAccountTableStatement),
                        connection.ExecuteAsync(CreateFlagTableStatement),
                        connection.ExecuteAsync(CreatePayeeTableStatement),
                        connection.ExecuteAsync(CreateCategoryTableStatement),
                        connection.ExecuteAsync(CreateTransTableStatement),
                        connection.ExecuteAsync(CreateSubTransactionTableStatement),
                        CreateDbSettingsTableAndInsertDefaultSettings(connection),
                        connection.ExecuteAsync(CreateBudgetEntryTableStatement),
                        connection.ExecuteAsync(SetDatabaseSchemaVersion))
                    .ConfigureAwait(false);
                

                transactionScope.Complete();
            }

            async Task CreateDbSettingsTableAndInsertDefaultSettings(IDbConnection conn)
            {
                await conn.ExecuteAsync(CreateDbSettingTableStatement).ConfigureAwait(false);
                await conn.InsertAsync(_dbSettingFactory()).ConfigureAwait(false);
            }
        }
    }
}
