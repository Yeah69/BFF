using System.Collections.Generic;
using System.Data.Common;
using System.Transactions;
using BFF.Helper.Import;
using Dapper.Contrib.Extensions;

namespace BFF.DB.SQLite
{
    class DapperImportingOrm : IImportingOrm
    {
        private readonly IProvideConnection _provideConnection;

        public DapperImportingOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public void PopulateDatabase(ImportLists importLists, ImportAssignments importAssignments)
        {
            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();

                /*  
                Hierarchical Category Inserting (which means that the ParentId is set right) is done automatically,
                because the structure of the imported csv-Entry of Categories allows to get the master category first and
                then the sub category. Thus, the parents id is known beforehand.
                */
                Queue<CategoryImportWrapper> categoriesOrder = new Queue<CategoryImportWrapper>(importLists.Categories);
                while (categoriesOrder.Count > 0)
                {
                    CategoryImportWrapper current = categoriesOrder.Dequeue();
                    var id = connection.Insert(current.Category);
                    foreach (PersistenceModels.IHaveCategory currentTitAssignment in current.TitAssignments)
                    {
                        currentTitAssignment.CategoryId = id;
                    }
                    foreach (CategoryImportWrapper categoryImportWrapper in current.Categories)
                    {
                        categoryImportWrapper.Category.ParentId = id;
                        categoriesOrder.Enqueue(categoryImportWrapper);
                    }
                }
                foreach (PersistenceModels.Payee payee in importLists.Payees)
                {
                    var id = connection.Insert(payee);
                    foreach (PersistenceModels.IHavePayee transIncBase in importAssignments.PayeeToTransactionBase[payee])
                    {
                        transIncBase.PayeeId = id;
                    }
                }
                foreach (PersistenceModels.Flag flag in importLists.Flags)
                {
                    var id = connection.Insert(flag);
                    foreach (PersistenceModels.IHaveFlag transBase in importAssignments.FlagToTransBase[flag])
                    {
                        transBase.FlagId = id;
                    }
                }
                foreach (PersistenceModels.Account account in importLists.Accounts)
                {
                    var id = connection.Insert(account);
                    foreach (PersistenceModels.IHaveAccount transIncBase in importAssignments.AccountToTransactionBase[account])
                    {
                        transIncBase.AccountId = id;
                    }
                    foreach (PersistenceModels.Trans transfer in importAssignments.FromAccountToTransfer[account])
                    {
                        transfer.PayeeId = id;
                    }
                    foreach (PersistenceModels.Trans transfer in importAssignments.ToAccountToTransfer[account])
                    {
                        transfer.CategoryId = id;
                    }
                }
                foreach (PersistenceModels.Trans parentTransaction in importLists.ParentTransactions)
                {
                    var id = connection.Insert(parentTransaction);
                    foreach (PersistenceModels.SubTransaction subTransaction in importAssignments.ParentTransactionToSubTransaction[parentTransaction])
                    {
                        subTransaction.ParentId = id;
                    }
                }

                connection.Insert(importLists.Transactions);

                connection.Insert(importLists.SubTransactions);

                connection.Insert(importLists.Transfers);

                connection.Insert(importLists.BudgetEntries);

                transactionScope.Complete();
            }
        }
    }
}
