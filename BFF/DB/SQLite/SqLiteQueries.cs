using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB.SQLite
{
    static class SqLiteQueries
    {
        internal static readonly int DatabaseSchemaVersion = 2;

        internal static string SetDatabaseSchemaVersion = $@"PRAGMA user_version = {DatabaseSchemaVersion};";

        #region BalanceStatements

        internal static string AllAccountsBalanceStatement =>
$@"SELECT Total({nameof(ITransaction.Sum)}) FROM (
SELECT {nameof(ITransInc.Sum)} FROM {nameof(Transaction)}s UNION ALL 
SELECT {nameof(ITransInc.Sum)} FROM {nameof(Income)}s UNION ALL 
SELECT {nameof(ISubTransInc.Sum)} FROM {nameof(SubTransaction)}s UNION ALL 
SELECT {nameof(ISubTransInc.Sum)} FROM {nameof(SubIncome)}s UNION ALL 
SELECT {nameof(IAccount.StartingBalance)} FROM {nameof(Account)}s);";

        internal static string AccountSpecificBalanceStatement =>
$@"SELECT (SELECT Total({nameof(ITransaction.Sum)}) FROM (
SELECT {nameof(ITransInc.Sum)} FROM {nameof(Transaction)}s WHERE {nameof(ITransaction.AccountId)} = @accountId UNION ALL 
SELECT {nameof(ITransInc.Sum)} FROM {nameof(Income)}s WHERE {nameof(IIncome.AccountId)} = @accountId UNION ALL
SELECT {nameof(ISubTransaction.Sum)} FROM {nameof(SubTransaction)}s INNER JOIN {nameof(ParentTransaction)}s ON {nameof(ISubTransaction.ParentId)} = {nameof(ParentTransaction)}s.{nameof(IParentTransaction.Id)} AND {nameof(IParentTransaction.AccountId)} = @accountId UNION ALL
SELECT {nameof(ISubIncome.Sum)} FROM {nameof(SubIncome)}s INNER JOIN {nameof(ParentIncome)}s ON {nameof(ISubIncome.ParentId)} = {nameof(ParentIncome)}s.{nameof(IParentIncome.Id)} AND {nameof(IParentIncome.AccountId)} = @accountId UNION ALL
SELECT {nameof(ITransInc.Sum)} FROM {nameof(Transfer)}s WHERE {nameof(ITransfer.ToAccountId)} = @accountId UNION ALL
SELECT {nameof(IAccount.StartingBalance)} FROM {nameof(Account)}s WHERE {nameof(IAccount.Id)} = @accountId)) 
- (SELECT Total({nameof(ITransInc.Sum)}) FROM {nameof(Transfer)}s WHERE {nameof(ITransfer.FromAccountId)} = @accountId);";

        #endregion BalanceStatements

        #region CreateStatements

        internal static string CreateTheTitViewStatement =>
                    $@"CREATE VIEW IF NOT EXISTS [The Tit] AS
SELECT {nameof(IDataModelBase.Id)}, {nameof(ITransIncBase.AccountId)}, {nameof(ITransIncBase.PayeeId)}, {nameof(ITransInc.CategoryId)}, {nameof(ITitBase.Date)}, {nameof(ITitLike.Memo)}, {nameof(ITransInc.Sum)}, {nameof(ITitBase.Cleared)}, '{TitType.Transaction}' AS Type FROM [{nameof(Transaction)}s]
UNION ALL
SELECT {nameof(IDataModelBase.Id)}, {nameof(ITransIncBase.AccountId)}, {nameof(ITransIncBase.PayeeId)}, -69 AS CategoryFiller, {nameof(ITitBase.Date)}, {nameof(ITitLike.Memo)}, -69 AS SumFiller, {nameof(ITitBase.Cleared)}, '{TitType.ParentTransaction}' AS Type FROM [{nameof(ParentTransaction)}s]
UNION ALL
SELECT {nameof(IDataModelBase.Id)}, {nameof(ITransIncBase.AccountId)}, {nameof(ITransIncBase.PayeeId)}, {nameof(ITransInc.CategoryId)}, {nameof(ITitBase.Date)}, {nameof(ITitLike.Memo)}, {nameof(ITransInc.Sum)}, {nameof(ITitBase.Cleared)}, '{TitType.Income}' AS Type FROM [{nameof(Income)}s]
UNION ALL
SELECT {nameof(IDataModelBase.Id)}, {nameof(ITransIncBase.AccountId)}, {nameof(ITransIncBase.PayeeId)}, -69 AS CategoryFiller, {nameof(ITitBase.Date)}, {nameof(ITitLike.Memo)}, -69 AS SumFiller, {nameof(ITitBase.Cleared)}, '{TitType.ParentIncome}' AS Type FROM [{nameof(ParentIncome)}s]
UNION ALL
SELECT {nameof(IDataModelBase.Id)}, -69 AS AccountFiller, {nameof(ITransfer.FromAccountId)}, {nameof(ITransfer.ToAccountId)}, {nameof(ITitBase.Date)}, {nameof(ITitLike.Memo)}, {nameof(ITransInc.Sum)}, {nameof(ITitBase.Cleared)}, '{TitType.Transfer}' AS Type FROM [{nameof(Transfer)}s];";

        internal static string CreateAccountTableStatement
            =>
                $@"CREATE TABLE [{nameof(Account)}s](
                        {nameof(IAccount.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(IAccount.Name)
                    } VARCHAR(100),
                        {nameof(IAccount.StartingBalance)
                    } INTEGER NOT NULL DEFAULT 0);";

        // todo: Seems not legit the Budget Table Statement
        //                private static string CreateBudgetTableStatement
        //                    =>
        //                        $@"CREATE TABLE [{nameof(Budget)}s](
        //                                {nameof(Budget.Id)
        //                            } INTEGER PRIMARY KEY,
        //                                {nameof(Budget.MonthYear)} DATE);";

        internal static string CreateCategoryTableStatement
            =>
                $@"CREATE TABLE [{nameof(Category)}s](
                        {nameof(ICategory.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(ICategory.ParentId)
                    } INTEGER,
                        {nameof(ICategory.Name)
                    } VARCHAR(100),
                        FOREIGN KEY({nameof(ICategory.ParentId)}) REFERENCES {
                    nameof(Category)}s({nameof(ICategory.Id)}) ON DELETE SET NULL);";

        internal static string CreateIncomeTableStatement
            =>
                $@"CREATE TABLE [{nameof(Income)}s](
                        {nameof(IIncome.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(IIncome.AccountId)
                    } INTEGER,
                        {nameof(IIncome.PayeeId)} INTEGER,
                        {
                    nameof(IIncome.CategoryId)} INTEGER,
                        {nameof(IIncome.Date)
                    } DATE,
                        {nameof(IIncome.Memo)} TEXT,
                        {
                    nameof(IIncome.Sum)} INTEGER,
                        {nameof(IIncome.Cleared)
                    } INTEGER,
                        FOREIGN KEY({nameof(IIncome.AccountId)}) REFERENCES {
                    nameof(Account)}s({nameof(IAccount.Id)}) ON DELETE CASCADE,
                        FOREIGN KEY({
                    nameof(IIncome.PayeeId)}) REFERENCES {nameof(Payee)}s({nameof(IPayee.Id)
                    }) ON DELETE SET NULL,
                        FOREIGN KEY({nameof(IIncome.CategoryId)
                    }) REFERENCES {nameof(Category)}s({nameof(ICategory.Id)}) ON DELETE SET NULL);";

        internal static string CreatePayeeTableStatement
            =>
                $@"CREATE TABLE [{nameof(Payee)}s](
                        {nameof(IPayee.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(IPayee.Name)} VARCHAR(100));";

        internal static string CreateParentTransactionTableStatement
            =>
                $@"CREATE TABLE [{nameof(ParentTransaction)}s](
                        {nameof(IParentTransaction.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(IParentTransaction.AccountId)
                    } INTEGER,
                        {nameof(IParentTransaction.PayeeId)
                    } INTEGER,
                        {nameof(IParentTransaction.Date)} DATE,
                        {
                    nameof(IParentTransaction.Memo)} TEXT,
                        {nameof(IParentTransaction.Cleared)} INTEGER,
                        FOREIGN KEY({
                    nameof(IParentTransaction.AccountId)}) REFERENCES {nameof(Account)}s({nameof(IAccount.Id)
                    }) ON DELETE CASCADE,
                        FOREIGN KEY({nameof(IParentTransaction.PayeeId)
                    }) REFERENCES {nameof(Payee)}s({nameof(IPayee.Id)
                    }) ON DELETE SET NULL);";

        internal static string CreateParentIncomeTableStatement
            =>
                $@"CREATE TABLE [{nameof(ParentIncome)}s](
                        {nameof(IParentIncome.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(IParentIncome.AccountId)
                    } INTEGER,
                        {nameof(IParentIncome.PayeeId)
                    } INTEGER,
                        {nameof(IParentIncome.Date)} DATE,
                        {
                    nameof(IParentIncome.Memo)} TEXT,
                        {nameof(IParentIncome.Cleared)} INTEGER,
                        FOREIGN KEY({
                    nameof(IParentIncome.AccountId)}) REFERENCES {nameof(Account)}s({nameof(IAccount.Id)
                    }) ON DELETE CASCADE,
                        FOREIGN KEY({nameof(IParentIncome.PayeeId)
                    }) REFERENCES {nameof(Payee)}s({nameof(IPayee.Id)
                    }) ON DELETE SET NULL);";

        internal static string CreateSubTransactionTableStatement
            =>
                $@"CREATE TABLE [{nameof(SubTransaction)}s](
                        {nameof(ISubTransaction.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(ISubTransaction.ParentId)
                    } INTEGER,
                        {nameof(ISubTransaction.CategoryId)
                    } INTEGER,
                        {nameof(ISubTransaction.Memo)} TEXT,
                        {
                    nameof(ISubTransaction.Sum)} INTEGER,
                        FOREIGN KEY({
                    nameof(ISubTransaction.ParentId)}) REFERENCES {nameof(ParentTransaction)}s({nameof(IParentTransaction.Id)
                    }) ON DELETE CASCADE);";

        internal static string CreateSubIncomeTableStatement
            =>
                $@"CREATE TABLE [{nameof(SubIncome)}s](
                        {nameof(ISubIncome.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(ISubIncome.ParentId)
                    } INTEGER,
                        {nameof(ISubIncome.CategoryId)
                    } INTEGER,
                        {nameof(ISubIncome.Memo)} TEXT,
                        {
                    nameof(ISubIncome.Sum)} INTEGER,
                        FOREIGN KEY({nameof(ISubIncome.ParentId)
                    }) REFERENCES {nameof(ParentIncome)}s({nameof(IParentIncome.Id)}) ON DELETE CASCADE);";

        internal static string CreateTransactionTableStatement
            =>
                $@"CREATE TABLE [{nameof(Transaction)}s](
                        {nameof(ITransaction.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(ITransaction.AccountId)
                    } INTEGER,
                        {nameof(ITransaction.PayeeId)
                    } INTEGER,
                        {nameof(ITransaction.CategoryId)
                    } INTEGER,
                        {nameof(ITransaction.Date)} DATE,
                        {
                    nameof(ITransaction.Memo)} TEXT,
                        {nameof(ITransaction.Sum)
                    } INTEGER,
                        {nameof(ITransaction.Cleared)} INTEGER,
                        FOREIGN KEY({
                    nameof(ITransaction.AccountId)}) REFERENCES {nameof(Account)}s({nameof(IAccount.Id)
                    }) ON DELETE CASCADE,
                        FOREIGN KEY({nameof(ITransaction.PayeeId)
                    }) REFERENCES {nameof(Payee)}s({nameof(IPayee.Id)
                    }) ON DELETE SET NULL,
                        FOREIGN KEY({nameof(ITransaction.CategoryId)
                    }) REFERENCES {nameof(Category)}s({nameof(ICategory.Id)}) ON DELETE SET NULL);";

        internal static string CreateTransferTableStatement
            =>
                $@"CREATE TABLE [{nameof(Transfer)}s](
                        {nameof(ITransfer.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(ITransfer.FromAccountId)
                    } INTEGER,
                        {nameof(ITransfer.ToAccountId)
                    } INTEGER,
                        {nameof(ITransfer.Date)} DATE,
                        {
                    nameof(ITransfer.Memo)} TEXT,
                        {nameof(ITransInc.Sum)
                    } INTEGER,
                        {nameof(ITransfer.Cleared)} INTEGER,
                        FOREIGN KEY({
                    nameof(ITransfer.FromAccountId)}) REFERENCES {nameof(Account)}s({nameof(IAccount.Id)
                    }) ON DELETE RESTRICT,
                        FOREIGN KEY({nameof(ITransfer.ToAccountId)
                    }) REFERENCES {nameof(Account)}s({nameof(IAccount.Id)}) ON DELETE RESTRICT);";

        internal static string CreateDbSettingTableStatement
            =>
                $@"CREATE TABLE [{nameof(DbSetting)}s]({nameof(IDbSetting.Id)} INTEGER PRIMARY KEY, {nameof(IDbSetting.CurrencyCultureName)} VARCHAR(10),
{nameof(IDbSetting.DateCultureName)} VARCHAR(10));";

        #endregion
    }
}
