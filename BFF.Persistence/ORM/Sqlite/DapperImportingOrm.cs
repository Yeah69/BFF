using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Core.Persistence;
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
                    var id = await connection.InsertAsync(current.Category as Category).ConfigureAwait(false);
                    foreach (IHaveCategoryDto currentTransAssignment in current.TransAssignments)
                    {
                        currentTransAssignment.CategoryId = id;
                    }
                    foreach (CategoryImportWrapper categoryImportWrapper in current.Categories)
                    {
                        categoryImportWrapper.Category.ParentId = id;
                        categoriesOrder.Enqueue(categoryImportWrapper);
                    }
                }
                foreach (IPayeeDto payee in importLists.Payees)
                {
                    var id = await connection.InsertAsync(payee as Payee).ConfigureAwait(false);
                    foreach (IHavePayeeDto transIncBase in importAssignments.PayeeToTransactionBase[payee])
                    {
                        transIncBase.PayeeId = id;
                    }
                }
                foreach (IFlagDto flag in importLists.Flags)
                {
                    var id = await connection.InsertAsync(flag as Flag).ConfigureAwait(false);
                    foreach (IHaveFlagDto transBase in importAssignments.FlagToTransBase[flag])
                    {
                        transBase.FlagId = id;
                    }
                }
                foreach (IAccountDto account in importLists.Accounts)
                {
                    var id = await connection.InsertAsync(account as Account).ConfigureAwait(false);
                    foreach (IHaveAccountDto transIncBase in importAssignments.AccountToTransactionBase[account])
                    {
                        transIncBase.AccountId = id;
                    }
                    foreach (ITransDto transfer in importAssignments.FromAccountToTransfer[account])
                    {
                        transfer.PayeeId = id;
                    }
                    foreach (ITransDto transfer in importAssignments.ToAccountToTransfer[account])
                    {
                        transfer.CategoryId = id;
                    }
                }
                foreach (ITransDto parentTransaction in importLists.ParentTransactions)
                {
                    var id = await connection.InsertAsync(parentTransaction as Trans).ConfigureAwait(false);
                    foreach (ISubTransactionDto subTransaction in importAssignments.ParentTransactionToSubTransaction[parentTransaction])
                    {
                        subTransaction.ParentId = id;
                    }
                }

                await Task.WhenAll(
                    connection.InsertAsync(importLists.Transactions.Cast<Trans>()),
                    connection.InsertAsync(importLists.SubTransactions.Cast<SubTransaction>()),
                    connection.InsertAsync(importLists.Transfers.Cast<Trans>()),
                    connection.InsertAsync(importLists.BudgetEntries.Cast<BudgetEntry>())).ConfigureAwait(false);

                transactionScope.Complete();
            }
        }
    }
}
