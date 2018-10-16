using System.Data;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.ORM.Sqlite
{
    internal class DapperCreateBackendOrm : ICreateBackendOrm
    {
        private static string CreateAccountTableStatement =>
            $@"CREATE TABLE [{nameof(AccountDto)}s](
            {nameof(AccountDto.Id)} INTEGER PRIMARY KEY,
            {nameof(AccountDto.Name)} VARCHAR(100),
            {nameof(AccountDto.StartingBalance)} INTEGER NOT NULL DEFAULT 0,
            {nameof(AccountDto.StartingDate)} DATETIME NOT NULL);";

        private static string CreateBudgetEntryTableStatement =>
            $@"CREATE TABLE [{nameof(BudgetEntryDto)}s](
            {nameof(BudgetEntryDto.Id)} INTEGER PRIMARY KEY,
            {nameof(BudgetEntryDto.CategoryId)} INTEGER,
            {nameof(BudgetEntryDto.Month)} DATE,
            {nameof(BudgetEntryDto.Budget)} INTEGER,
            FOREIGN KEY({nameof(BudgetEntryDto.CategoryId)}) REFERENCES {nameof(CategoryDto)}s({nameof(CategoryDto.Id)}) ON DELETE SET NULL);";

        private static string CreateCategoryTableStatement =>
            $@"CREATE TABLE [{nameof(CategoryDto)}s](
            {nameof(CategoryDto.Id)} INTEGER PRIMARY KEY,
            {nameof(CategoryDto.ParentId)} INTEGER,
            {nameof(CategoryDto.Name)} VARCHAR(100),
            {nameof(CategoryDto.IsIncomeRelevant)} INTEGER,
            {nameof(CategoryDto.MonthOffset)} INTEGER,
            FOREIGN KEY({nameof(CategoryDto.ParentId)}) REFERENCES {nameof(CategoryDto)}s({nameof(CategoryDto.Id)}) ON DELETE SET NULL);";

        private static string CreateDbSettingTableStatement =>
            $@"CREATE TABLE [{nameof(DbSettingDto)}s](
            {nameof(DbSettingDto.Id)} INTEGER PRIMARY KEY, 
            {nameof(DbSettingDto.CurrencyCultureName)} VARCHAR(10),
            {nameof(DbSettingDto.DateCultureName)} VARCHAR(10));";

        private static string CreateFlagTableStatement =>
            $@"CREATE TABLE [{nameof(FlagDto)}s](
            {nameof(FlagDto.Id)} INTEGER PRIMARY KEY,
            {nameof(FlagDto.Name)} VARCHAR(100),
            {nameof(FlagDto.Color)} INTEGER);";

        private static string CreatePayeeTableStatement =>
            $@"CREATE TABLE [{nameof(PayeeDto)}s](
            {nameof(PayeeDto.Id)} INTEGER PRIMARY KEY,
            {nameof(PayeeDto.Name)} VARCHAR(100));";

        private static string CreateSubTransactionTableStatement =>
            $@"CREATE TABLE [{nameof(SubTransactionDto)}s](
            {nameof(SubTransactionDto.Id)} INTEGER PRIMARY KEY,
            {nameof(SubTransactionDto.ParentId)} INTEGER,
            {nameof(SubTransactionDto.CategoryId)} INTEGER,
            {nameof(SubTransactionDto.Memo)} TEXT,
            {nameof(SubTransactionDto.Sum)} INTEGER,
            FOREIGN KEY({nameof(SubTransactionDto.ParentId)}) REFERENCES {nameof(TransDto)}s({nameof(TransDto.Id)}) ON DELETE CASCADE);";

        private static string CreateTransTableStatement =>
            $@"CREATE TABLE {nameof(TransDto)}s(
            {nameof(TransDto.Id)} INTEGER PRIMARY KEY,
            {nameof(TransDto.FlagId)} INTEGER,
            {nameof(TransDto.CheckNumber)} TEXT,
            {nameof(TransDto.AccountId)} INTEGER,
            {nameof(TransDto.PayeeId)} INTEGER,
            {nameof(TransDto.CategoryId)} INTEGER,
            {nameof(TransDto.Date)} DATE,
            {nameof(TransDto.Memo)} TEXT,
            {nameof(TransDto.Sum)} INTEGER,
            {nameof(TransDto.Cleared)} INTEGER,
            {nameof(TransDto.Type)} VARCHAR(17),
            FOREIGN KEY({nameof(TransDto.FlagId)}) REFERENCES {nameof(FlagDto)}s({nameof(FlagDto.Id)}) ON DELETE SET NULL);
            CREATE INDEX {nameof(TransDto)}s_{nameof(TransDto.AccountId)}_{nameof(TransDto.PayeeId)}_index ON {nameof(TransDto)}s ({nameof(TransDto.AccountId)}, {nameof(TransDto.PayeeId)});
            CREATE INDEX {nameof(TransDto)}s_{nameof(TransDto.AccountId)}_{nameof(TransDto.CategoryId)}_index ON {nameof(TransDto)}s ({nameof(TransDto.AccountId)}, {nameof(TransDto.CategoryId)});
            CREATE INDEX {nameof(TransDto)}s_{nameof(TransDto.Date)}_index ON {nameof(TransDto)}s ({nameof(TransDto.Date)});";
        
        /// <summary>
        /// First for digits are the release year, next two digits are the release counter, next three are the patch counter
        /// </summary>
        private static readonly int DatabaseSchemaVersion = 201801001;

        private static readonly string SetDatabaseSchemaVersion = $@"PRAGMA user_version = {DatabaseSchemaVersion};";

        private readonly IProvideConnection _provideConnection;

        public DapperCreateBackendOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
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
                await conn.InsertAsync(new DbSettingDto()).ConfigureAwait(false);
            }
        }
    }
}
