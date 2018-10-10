using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Persistence.Import;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.ORM.Sqlite
{
    internal class DapperImportingOrm : IImportingOrm
    {
        private readonly IProvideConnection _provideConnection;

        public DapperImportingOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task PopulateDatabaseAsync(ImportLists importLists, ImportAssignments importAssignments)
        {
            using (TransactionScope transactionScope = new TransactionScope(new TransactionScopeOption(), TimeSpan.FromMinutes(10)))
            using (IDbConnection connection = _provideConnection.Connection)
            {
                /*  
                Hierarchical Category Inserting (which means that the ParentId is set right) is done automatically,
                because the structure of the imported csv-Entry of Categories allows to get the master category first and
                then the sub category. Thus, the parents id is known beforehand.
                */
                Queue<CategoryImportWrapper> categoriesOrder = new Queue<CategoryImportWrapper>(importLists.Categories);
                while (categoriesOrder.Count > 0)
                {
                    CategoryImportWrapper current = categoriesOrder.Dequeue();
                    var id = await connection.InsertAsync(current.Category).ConfigureAwait(false);
                    foreach (IHaveCategory currentTransAssignment in current.TransAssignments)
                    {
                        currentTransAssignment.CategoryId = id;
                    }
                    foreach (CategoryImportWrapper categoryImportWrapper in current.Categories)
                    {
                        categoryImportWrapper.Category.ParentId = id;
                        categoriesOrder.Enqueue(categoryImportWrapper);
                    }
                }
                foreach (Payee payee in importLists.Payees)
                {
                    var id = await connection.InsertAsync(payee).ConfigureAwait(false);
                    foreach (IHavePayee transIncBase in importAssignments.PayeeToTransactionBase[payee])
                    {
                        transIncBase.PayeeId = id;
                    }
                }
                foreach (Flag flag in importLists.Flags)
                {
                    var id = await connection.InsertAsync(flag).ConfigureAwait(false);
                    foreach (IHaveFlag transBase in importAssignments.FlagToTransBase[flag])
                    {
                        transBase.FlagId = id;
                    }
                }
                foreach (Account account in importLists.Accounts)
                {
                    var id = await connection.InsertAsync(account).ConfigureAwait(false);
                    foreach (IHaveAccount transIncBase in importAssignments.AccountToTransactionBase[account])
                    {
                        transIncBase.AccountId = id;
                    }
                    foreach (Trans transfer in importAssignments.FromAccountToTransfer[account])
                    {
                        transfer.PayeeId = id;
                    }
                    foreach (Trans transfer in importAssignments.ToAccountToTransfer[account])
                    {
                        transfer.CategoryId = id;
                    }
                }
                foreach (Trans parentTransaction in importLists.ParentTransactions)
                {
                    var id = await connection.InsertAsync(parentTransaction).ConfigureAwait(false);
                    foreach (SubTransaction subTransaction in importAssignments.ParentTransactionToSubTransaction[parentTransaction])
                    {
                        subTransaction.ParentId = id;
                    }
                }

                await Task.WhenAll(
                    connection.InsertAsync(importLists.Transactions),
                    connection.InsertAsync(importLists.SubTransactions),
                    connection.InsertAsync(importLists.Transfers),
                    connection.InsertAsync(importLists.BudgetEntries)).ConfigureAwait(false);

                transactionScope.Complete();
            }
        }
    }
}
