using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Persistence.Import;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite.Interfaces;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.ORM.Sqlite
{
    internal class DapperImportingOrm : IImportingOrm
    {
        private readonly IProvideSqliteConnection _provideConnection;

        public DapperImportingOrm(IProvideSqliteConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task PopulateDatabaseAsync(ISqliteYnab4CsvImportContainerData sqliteYnab4CsvImportContainer)
        {
            using (TransactionScope transactionScope = new TransactionScope(new TransactionScopeOption(), TimeSpan.FromMinutes(10)))
            using (IDbConnection connection = _provideConnection.Connection)
            {
                var importAssignments = sqliteYnab4CsvImportContainer.ImportAssignments;

                /*  
                Hierarchical Category Inserting (which means that the ParentId is set right) is done automatically,
                because the structure of the imported csv-Entry of Categories allows to get the master category first and
                then the sub category. Thus, the parents id is known beforehand.
                */
                Queue<CategoryImportWrapper> categoriesOrder = new Queue<CategoryImportWrapper>(sqliteYnab4CsvImportContainer.Categories);
                while (categoriesOrder.Count > 0)
                {
                    CategoryImportWrapper current = categoriesOrder.Dequeue();
                    var id = await connection.InsertAsync(current.Category as Category).ConfigureAwait(false);
                    foreach (IHaveCategorySql currentTransAssignment in current.TransAssignments)
                    {
                        currentTransAssignment.CategoryId = id;
                    }
                    foreach (CategoryImportWrapper categoryImportWrapper in current.Categories)
                    {
                        categoryImportWrapper.Category.ParentId = id;
                        categoriesOrder.Enqueue(categoryImportWrapper);
                    }
                }
                foreach (IPayeeSql payee in sqliteYnab4CsvImportContainer.Payees)
                {
                    var id = await connection.InsertAsync(payee as Payee).ConfigureAwait(false);
                    foreach (IHavePayeeSql transIncBase in importAssignments.PayeeToTransactionBase[payee])
                    {
                        transIncBase.PayeeId = id;
                    }
                }
                foreach (IFlagSql flag in sqliteYnab4CsvImportContainer.Flags)
                {
                    var id = await connection.InsertAsync(flag as Flag).ConfigureAwait(false);
                    foreach (IHaveFlagSql transBase in importAssignments.FlagToTransBase[flag])
                    {
                        transBase.FlagId = id;
                    }
                }
                foreach (IAccountSql account in sqliteYnab4CsvImportContainer.Accounts)
                {
                    var id = await connection.InsertAsync(account as Account).ConfigureAwait(false);
                    foreach (IHaveAccountSql transIncBase in importAssignments.AccountToTransactionBase[account])
                    {
                        transIncBase.AccountId = id;
                    }
                    foreach (ITransSql transfer in importAssignments.FromAccountToTransfer[account])
                    {
                        transfer.PayeeId = id;
                    }
                    foreach (ITransSql transfer in importAssignments.ToAccountToTransfer[account])
                    {
                        transfer.CategoryId = id;
                    }
                }
                foreach (ITransSql parentTransaction in sqliteYnab4CsvImportContainer.ParentTransactions)
                {
                    var id = await connection.InsertAsync(parentTransaction as Trans).ConfigureAwait(false);
                    foreach (ISubTransactionSql subTransaction in importAssignments.ParentTransactionToSubTransaction[parentTransaction])
                    {
                        subTransaction.ParentId = id;
                    }
                }

                await Task.WhenAll(
                    connection.InsertAsync(sqliteYnab4CsvImportContainer.Transactions.Cast<Trans>()),
                    connection.InsertAsync(sqliteYnab4CsvImportContainer.SubTransactions.Cast<SubTransaction>()),
                    connection.InsertAsync(sqliteYnab4CsvImportContainer.Transfers.Cast<Trans>()),
                    connection.InsertAsync(sqliteYnab4CsvImportContainer.BudgetEntries.Cast<BudgetEntry>())).ConfigureAwait(false);

                transactionScope.Complete();
            }
        }
    }
}
