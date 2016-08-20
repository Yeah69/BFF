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
$@"SELECT Total({nameof(Transaction.Sum)}) FROM (
SELECT {nameof(TransInc.Sum)} FROM {nameof(Transaction)}s UNION ALL 
SELECT {nameof(TransInc.Sum)} FROM {nameof(Income)}s UNION ALL 
SELECT {nameof(SubTransInc.Sum)} FROM {nameof(SubTransaction)}s UNION ALL 
SELECT {nameof(SubTransInc.Sum)} FROM {nameof(SubIncome)}s UNION ALL 
SELECT {nameof(Account.StartingBalance)} FROM {nameof(Account)}s);";

        internal static string AccountSpecificBalanceStatement =>
$@"SELECT (SELECT Total({nameof(Transaction.Sum)}) FROM (
SELECT {nameof(TransInc.Sum)} FROM {nameof(Transaction)}s WHERE {nameof(Transaction.AccountId)} = @accountId UNION ALL 
SELECT {nameof(TransInc.Sum)} FROM {nameof(Income)}s WHERE {nameof(Income.AccountId)} = @accountId UNION ALL
SELECT {nameof(SubTransInc.Sum)} FROM (SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)}, {nameof(ParentTransaction)}s.{nameof(Transaction.AccountId)} FROM {nameof(SubTransaction)}s INNER JOIN {nameof(ParentTransaction)}s ON {nameof(SubIncome)}s.{nameof(SubIncome.ParentId)} = {nameof(ParentTransaction)}s.{nameof(ParentTransaction.Id)}) WHERE {nameof(Transaction.AccountId)} = @accountId UNION ALL
SELECT {nameof(SubTransInc.Sum)} FROM (SELECT {nameof(SubIncome)}s.{nameof(SubIncome.Sum)}, {nameof(ParentIncome)}s.{nameof(ParentIncome.AccountId)} FROM {nameof(SubIncome)}s INNER JOIN {nameof(ParentIncome)}s ON {nameof(SubIncome)}s.{nameof(SubIncome.ParentId)} = {nameof(ParentIncome)}s.{nameof(ParentIncome.Id)}) WHERE {nameof(Income.AccountId)} = @accountId UNION ALL
SELECT {nameof(TransInc.Sum)} FROM {nameof(Transfer)}s WHERE {nameof(Transfer.ToAccountId)} = @accountId UNION ALL
SELECT {nameof(Account.StartingBalance)} FROM {nameof(Account)}s WHERE {nameof(Account.Id)} = @accountId)) 
- (SELECT Total({nameof(TransInc.Sum)}) FROM {nameof(Transfer)}s WHERE {nameof(Transfer.FromAccountId)} = @accountId);";

        #endregion BalanceStatements

        #region CreateStatements

        internal static string CreateTheTitViewStatement =>
                    $@"CREATE VIEW IF NOT EXISTS [The Tit] AS
SELECT {nameof(DataModelBase.Id)}, {nameof(TransIncBase.AccountId)}, {nameof(TransIncBase.PayeeId)}, {nameof(TransInc.CategoryId)}, {nameof(TitBase.Date)}, {nameof(TitLike.Memo)}, {nameof(TransInc.Sum)}, {nameof(TitBase.Cleared)}, '{TitType.Transaction}' AS Type FROM [{nameof(Transaction)}s]
UNION ALL
SELECT {nameof(DataModelBase.Id)}, {nameof(TransIncBase.AccountId)}, {nameof(TransIncBase.PayeeId)}, -69 AS CategoryFiller, {nameof(TitBase.Date)}, {nameof(TitLike.Memo)}, -69 AS SumFiller, {nameof(TitBase.Cleared)}, '{TitType.ParentTransaction}' AS Type FROM [{nameof(ParentTransaction)}s]
UNION ALL
SELECT {nameof(DataModelBase.Id)}, {nameof(TransIncBase.AccountId)}, {nameof(TransIncBase.PayeeId)}, {nameof(TransInc.CategoryId)}, {nameof(TitBase.Date)}, {nameof(TitLike.Memo)}, {nameof(TransInc.Sum)}, {nameof(TitBase.Cleared)}, '{TitType.Income}' AS Type FROM [{nameof(Income)}s]
UNION ALL
SELECT {nameof(DataModelBase.Id)}, {nameof(TransIncBase.AccountId)}, {nameof(TransIncBase.PayeeId)}, -69 AS CategoryFiller, {nameof(TitBase.Date)}, {nameof(TitLike.Memo)}, -69 AS SumFiller, {nameof(TitBase.Cleared)}, '{TitType.ParentIncome}' AS Type FROM [{nameof(ParentIncome)}s]
UNION ALL
SELECT {nameof(DataModelBase.Id)}, -69 AS AccountFiller, {nameof(Transfer.FromAccountId)}, {nameof(Transfer.ToAccountId)}, {nameof(TitBase.Date)}, {nameof(TitLike.Memo)}, {nameof(TransInc.Sum)}, {nameof(TitBase.Cleared)}, '{TitType.Transfer}' AS Type FROM [{nameof(Transfer)}s];";

        internal static string CreateAccountTableStatement
            =>
                $@"CREATE TABLE [{nameof(Account)}s](
                        {nameof(Account.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(Account.Name)
                    } VARCHAR(100),
                        {nameof(Account.StartingBalance)
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
                        {nameof(Category.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(Category.ParentId)
                    } INTEGER,
                        {nameof(Category.Name)
                    } VARCHAR(100),
                        FOREIGN KEY({nameof(Category.ParentId)}) REFERENCES {
                    nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";

        internal static string CreateIncomeTableStatement
            =>
                $@"CREATE TABLE [{nameof(Income)}s](
                        {nameof(Income.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(Income.AccountId)
                    } INTEGER,
                        {nameof(Income.PayeeId)} INTEGER,
                        {
                    nameof(Income.CategoryId)} INTEGER,
                        {nameof(Income.Date)
                    } DATE,
                        {nameof(Income.Memo)} TEXT,
                        {
                    nameof(Income.Sum)} INTEGER,
                        {nameof(Income.Cleared)
                    } INTEGER,
                        FOREIGN KEY({nameof(Income.AccountId)}) REFERENCES {
                    nameof(Account)}s({nameof(Account.Id)}) ON DELETE CASCADE,
                        FOREIGN KEY({
                    nameof(Income.PayeeId)}) REFERENCES {nameof(Payee)}s({nameof(Payee.Id)
                    }) ON DELETE SET NULL,
                        FOREIGN KEY({nameof(Income.CategoryId)
                    }) REFERENCES {nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";

        internal static string CreatePayeeTableStatement
            =>
                $@"CREATE TABLE [{nameof(Payee)}s](
                        {nameof(Payee.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(Payee.Name)} VARCHAR(100));";

        internal static string CreateParentTransactionTableStatement
            =>
                $@"CREATE TABLE [{nameof(ParentTransaction)}s](
                        {nameof(ParentTransaction.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(ParentTransaction.AccountId)
                    } INTEGER,
                        {nameof(ParentTransaction.PayeeId)
                    } INTEGER,
                        {nameof(ParentTransaction.Date)} DATE,
                        {
                    nameof(ParentTransaction.Memo)} TEXT,
                        {nameof(ParentTransaction.Cleared)} INTEGER,
                        FOREIGN KEY({
                    nameof(ParentTransaction.AccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)
                    }) ON DELETE CASCADE,
                        FOREIGN KEY({nameof(ParentTransaction.PayeeId)
                    }) REFERENCES {nameof(Payee)}s({nameof(Payee.Id)
                    }) ON DELETE SET NULL);";

        internal static string CreateParentIncomeTableStatement
            =>
                $@"CREATE TABLE [{nameof(ParentIncome)}s](
                        {nameof(ParentIncome.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(ParentIncome.AccountId)
                    } INTEGER,
                        {nameof(ParentIncome.PayeeId)
                    } INTEGER,
                        {nameof(ParentIncome.Date)} DATE,
                        {
                    nameof(ParentIncome.Memo)} TEXT,
                        {nameof(ParentIncome.Cleared)} INTEGER,
                        FOREIGN KEY({
                    nameof(ParentIncome.AccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)
                    }) ON DELETE CASCADE,
                        FOREIGN KEY({nameof(ParentIncome.PayeeId)
                    }) REFERENCES {nameof(Payee)}s({nameof(Payee.Id)
                    }) ON DELETE SET NULL);";

        internal static string CreateSubTransactionTableStatement
            =>
                $@"CREATE TABLE [{nameof(SubTransaction)}s](
                        {nameof(SubTransaction.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(SubTransaction.ParentId)
                    } INTEGER,
                        {nameof(SubTransaction.CategoryId)
                    } INTEGER,
                        {nameof(SubTransaction.Memo)} TEXT,
                        {
                    nameof(SubTransaction.Sum)} INTEGER,
                        FOREIGN KEY({
                    nameof(SubTransaction.ParentId)}) REFERENCES {nameof(Transaction)}s({nameof(Transaction.Id)
                    }) ON DELETE CASCADE);";

        internal static string CreateSubIncomeTableStatement
            =>
                $@"CREATE TABLE [{nameof(SubIncome)}s](
                        {nameof(SubIncome.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(SubIncome.ParentId)
                    } INTEGER,
                        {nameof(SubIncome.CategoryId)
                    } INTEGER,
                        {nameof(SubIncome.Memo)} TEXT,
                        {
                    nameof(SubIncome.Sum)} INTEGER,
                        FOREIGN KEY({nameof(SubIncome.ParentId)
                    }) REFERENCES {nameof(Income)}s({nameof(Income.Id)}) ON DELETE CASCADE);";

        internal static string CreateTransactionTableStatement
            =>
                $@"CREATE TABLE [{nameof(Transaction)}s](
                        {nameof(Transaction.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(Transaction.AccountId)
                    } INTEGER,
                        {nameof(Transaction.PayeeId)
                    } INTEGER,
                        {nameof(Transaction.CategoryId)
                    } INTEGER,
                        {nameof(Transaction.Date)} DATE,
                        {
                    nameof(Transaction.Memo)} TEXT,
                        {nameof(Transaction.Sum)
                    } INTEGER,
                        {nameof(Transaction.Cleared)} INTEGER,
                        FOREIGN KEY({
                    nameof(Transaction.AccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)
                    }) ON DELETE CASCADE,
                        FOREIGN KEY({nameof(Transaction.PayeeId)
                    }) REFERENCES {nameof(Payee)}s({nameof(Payee.Id)
                    }) ON DELETE SET NULL,
                        FOREIGN KEY({nameof(Transaction.CategoryId)
                    }) REFERENCES {nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";

        internal static string CreateTransferTableStatement
            =>
                $@"CREATE TABLE [{nameof(Transfer)}s](
                        {nameof(Transfer.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(Transfer.FromAccountId)
                    } INTEGER,
                        {nameof(Transfer.ToAccountId)
                    } INTEGER,
                        {nameof(Transfer.Date)} DATE,
                        {
                    nameof(Transfer.Memo)} TEXT,
                        {nameof(TransInc.Sum)
                    } INTEGER,
                        {nameof(Transfer.Cleared)} INTEGER,
                        FOREIGN KEY({
                    nameof(Transfer.FromAccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)
                    }) ON DELETE RESTRICT,
                        FOREIGN KEY({nameof(Transfer.ToAccountId)
                    }) REFERENCES {nameof(Account)}s({nameof(Account.Id)}) ON DELETE RESTRICT);";

        internal static string CreateDbSettingTableStatement
            =>
                $@"CREATE TABLE [{nameof(DbSetting)}s]({nameof(DbSetting.Id)} INTEGER PRIMARY KEY, {nameof(DbSetting.CurrencyCultrureName)} VARCHAR(10),
{nameof(DbSetting.DateCultureName)} VARCHAR(10));";

        #endregion
    }
}
