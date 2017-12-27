using System.Collections.Generic;
using System.Linq;
using BFF.DB.PersistenceModels;

namespace BFF.Helper.Import
{
    public interface IImportable
    {
        string SavePath { get; set; }

        string Import();
    }

    public struct ImportLists
    {
        public IList<Account> Accounts;
        public IList<Payee> Payees;
        public IList<CategoryImportWrapper> Categories;
        public IList<Flag> Flags;

        public IList<Transaction> Transactions;
        public IList<Transfer> Transfers;

        public IList<ParentTransaction> ParentTransactions;

        public IList<SubTransaction> SubTransactions;

        public IList<BudgetEntry> BudgetEntries;
    }

    public struct ImportAssignments
    {
        public IDictionary<Account, IList<IHaveAccount>> AccountToTransactionBase;
        public IDictionary<Account, IList<Transfer>> FromAccountToTransfer;
        public IDictionary<Account, IList<Transfer>> ToAccountToTransfer;

        public IDictionary<Payee, IList<IHavePayee>> PayeeToTransactionBase;

        public IDictionary<ParentTransaction, IList<SubTransaction>> ParentTransactionToSubTransaction;
        
        public IDictionary<Flag, IList<IHaveFlag>> FlagToTransBase;
    }
}
